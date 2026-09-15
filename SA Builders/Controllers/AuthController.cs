using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;
using SA_Builders.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext db, IConfiguration config, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateAccountDto dto)
        {
            var token = await _db.ActivationTokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.Token);

            if (token is null || token.Used || token.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "This activation link is invalid or has expired. Please request a new one." });

            var user = token.User!;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.IsActivated = true;
            token.Used = true;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Account activated. You can now log in." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null || !user.IsActivated || user.PasswordHash is null)
                return Unauthorized(new { message = "Invalid credentials or account not yet activated." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = GenerateJwt(user.Id, user.Email, user.Role.ToString());
            return Ok(new { token, role = user.Role.ToString() });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            var genericResponse = new
            {
                message = "If an account with that email exists, a password reset link has been sent."
            };

            if (user is null || !user.IsActivated)
            {
                return Ok(genericResponse);
            }

            var token = GenerateSecureToken();
            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
            await _db.SaveChangesAsync();

            var baseUrl = _config["Frontend:BaseUrl"] ?? "https://yoursite.com";
            var resetLink = $"{baseUrl}/reset-password?token={token}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return Ok(genericResponse);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var token = await _db.PasswordResetTokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.Token);

            if (token is null || token.Used || token.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "This reset link is invalid or has expired. Please request a new one." });

            var user = token.User!;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            token.Used = true;

            var otherTokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.Used && t.Id != token.Id)
                .ToListAsync();
            foreach (var t in otherTokens) t.Used = true;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Password reset successfully. You can now log in with your new password." });
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes);
        }

        private string GenerateJwt(int userId, string email, string role)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}