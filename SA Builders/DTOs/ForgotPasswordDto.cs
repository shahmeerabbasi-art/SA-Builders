using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;
using SA_Builders.Validation;

namespace SA_Builders.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [InternationalEmail]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength)]
        [StrongPassword]
        public string NewPassword { get; set; } = string.Empty;
    }
}