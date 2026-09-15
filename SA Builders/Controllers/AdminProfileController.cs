using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/me")]
    [Authorize(Roles = "Admin")]
    public class AdminProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminProfileController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound();

            return Ok(new { user.Id, user.FullName, user.Email, user.CreatedAt });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound();

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _db.Users.AnyAsync(u => u.Id != userId && u.Email == dto.Email);
                if (emailTaken)
                    return Conflict(new { message = "A user with this email already exists." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user is null || user.PasswordHash is null) return NotFound();

            // Require the current password before allowing a change —
            // stops someone with a stolen, still-logged-in session token
            // from silently locking the real admin out by changing the
            // password without knowing it first.
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return idClaim is null ? 0 : int.Parse(idClaim.Value);
        }
    }
}