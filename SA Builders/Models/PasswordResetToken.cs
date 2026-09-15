using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        [Required, StringLength(128, MinimumLength = 32)]
        public string Token { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "UserId must reference a valid user.")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool Used { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}