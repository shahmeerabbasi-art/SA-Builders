using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;
using SA_Builders.Services;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AdminController(AppDbContext db, IEmailService emailService, IConfiguration config)
        {
            _db = db;
            _emailService = emailService;
            _config = config;
        }

        [HttpPost("clients")]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
        {
            var project = await _db.Projects.FindAsync(dto.ProjectId);
            if (project is null) return NotFound(new { message = "Project not found." });

            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists) return Conflict(new { message = "A user with this email already exists." });

            var adminId = GetCurrentAdminId();

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = UserRole.Client,
                IsActivated = false,
                PasswordHash = null
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.ProjectAssignments.Add(new ProjectAssignment
            {
                UserId = user.Id,
                ProjectId = project.Id,
                AssignedByAdminId = adminId
            });

            var token = GenerateSecureToken();
            _db.ActivationTokens.Add(new ActivationToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(72)
            });

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = adminId,
                Action = "CreatedClient",
                Details = $"Created user {user.Email} and bound to project {project.Id}"
            });

            await _db.SaveChangesAsync();

            var baseUrl = _config["Frontend:BaseUrl"] ?? "https://yoursite.com";
            var activationLink = $"{baseUrl}/activate?token={token}";
            await _emailService.SendActivationEmailAsync(user.Email, activationLink);

            return Ok(new { message = "Client created. Activation email sent.", userId = user.Id });
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _db.Users
                .Where(u => u.Role == UserRole.Client)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.IsActivated,
                    u.CreatedAt,
                    Projects = u.ProjectAssignments.Select(pa => new
                    {
                        pa.ProjectId,
                        ProjectTitle = pa.Project!.Title
                    })
                })
                .ToListAsync();

            return Ok(clients);
        }

        [HttpPost("clients/{id}/resend-activation")]
        public async Task<IActionResult> ResendActivation(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user is null || user.Role != UserRole.Client)
                return NotFound(new { message = "Client not found." });

            if (user.IsActivated)
                return BadRequest(new { message = "This client has already activated their account." });

            // Invalidate any old unused tokens for this user first — only the
            // newest link should ever work, same principle as the password
            // reset flow.
            var oldTokens = await _db.ActivationTokens
                .Where(t => t.UserId == user.Id && !t.Used)
                .ToListAsync();
            foreach (var t in oldTokens) t.Used = true;

            var token = GenerateSecureToken();
            _db.ActivationTokens.Add(new ActivationToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(72)
            });

            var adminId = GetCurrentAdminId();
            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = adminId,
                Action = "ResentActivation",
                Details = $"Resent activation link to {user.Email}"
            });

            await _db.SaveChangesAsync();

            var baseUrl = _config["Frontend:BaseUrl"] ?? "https://yoursite.com";
            var activationLink = $"{baseUrl}/activate?token={token}";
            await _emailService.SendActivationEmailAsync(user.Email, activationLink);

            return Ok(new { message = "Activation email resent." });
        }

        [HttpPut("clients/{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Client)
                return NotFound(new { message = "Client not found." });

            // If the email is changing, make sure it's not already taken by someone else.
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _db.Users.AnyAsync(u => u.Id != id && u.Email == dto.Email);
                if (emailTaken)
                    return Conflict(new { message = "A user with this email already exists." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "UpdatedClient",
                Details = $"Updated client {id} — name/email changed"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Client updated." });
        }

        [HttpDelete("clients/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Client)
                return NotFound(new { message = "Client not found." });

            _db.Users.Remove(user); // cascades to ProjectAssignments, ActivationTokens, PasswordResetTokens

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "DeletedClient",
                Details = $"Deleted client {id} ({user.Email})"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Client deleted." });
        }
        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes);
        }

        private int GetCurrentAdminId()
        {
            var idClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return idClaim is null ? 0 : int.Parse(idClaim.Value);
        }
    }
}