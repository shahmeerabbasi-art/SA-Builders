using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.DTOs
{
    public class CreateTestimonialDto
    {
        [Required, StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength)]
        public string ClientName { get; set; } = string.Empty;

        [Required, StringLength(ValidationConstants.LongTextMaxLength, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty;

        [Url(ErrorMessage = "MediaUrl must be a valid URL.")]
        public string? MediaUrl { get; set; }

        [Range(ValidationConstants.MinRating, ValidationConstants.MaxRating)]
        public int Rating { get; set; } = 5;

        public int? ProjectId { get; set; }

        public bool Approved { get; set; } = false;
    }
}