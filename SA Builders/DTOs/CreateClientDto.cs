using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;
using SA_Builders.Models;
using SA_Builders.Validation;

namespace SA_Builders.DTOs
{
    public class CreateClientDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [InternationalEmail]
        [StringLength(ValidationConstants.EmailMaxLength)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A project must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a valid, positive ID.")]
        public int ProjectId { get; set; }
    }

    // Used by BOTH the client activation flow AND (if you later add an
    // admin "change my password" endpoint) admin password updates —
    // same strength rule for every account type.
    public class ActivateAccountDto
    {
        [Required(ErrorMessage = "Activation token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength,
            ErrorMessage = "Password must be at least {2} characters long.")]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [InternationalEmail]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
        // No [StrongPassword] here on purpose — login just checks against
        // the stored hash. Composition rules only apply when a password is
        // being CREATED, not every time someone types it in to log in.
    }

    public class CreateLeadDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact info is required.")]
        [StringLength(ValidationConstants.ShortTextMaxLength)]
        public string Contact { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lead source is required.")]
        [EnumDataType(typeof(LeadSource), ErrorMessage = "Invalid lead source.")]
        public LeadSource Source { get; set; }

        [StringLength(ValidationConstants.LongTextMaxLength)]
        public string? Message { get; set; }

        [Range(ValidationConstants.MinCoveredAreaSqFt, ValidationConstants.MaxCoveredAreaSqFt,
            ErrorMessage = "Covered area must be between {1} and {2} sq ft.")]
        public double? EstimatedCoveredArea { get; set; }

        [StringLength(ValidationConstants.ShortTextMaxLength)]
        public string? SelectedServiceTier { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Estimated cost cannot be negative.")]
        public decimal? EstimatedCost { get; set; }
    }
}