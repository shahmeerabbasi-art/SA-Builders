using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;
using SA_Builders.Validation;

namespace SA_Builders.DTOs
{
    public class UpdateClientDto
    {
        [Required, StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string FullName { get; set; } = string.Empty;

        [Required, InternationalEmail]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateAdminProfileDto
    {
        [Required, StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string FullName { get; set; } = string.Empty;

        [Required, InternationalEmail]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength)]
        [StrongPassword]
        public string NewPassword { get; set; } = string.Empty;
    }
}