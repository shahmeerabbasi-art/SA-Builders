using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;
using SA_Builders.Validation;

namespace SA_Builders.Models
{
    public enum UserRole { Admin, Client }

    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string FullName { get; set; } = string.Empty;

        [Required, InternationalEmail, StringLength(ValidationConstants.EmailMaxLength)]
        public string Email { get; set; } = string.Empty;

        // Null until the client completes activation and sets their own password.
        public string? PasswordHash { get; set; }

        [Required, EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; } = UserRole.Client;

        public bool IsActivated { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    }
}