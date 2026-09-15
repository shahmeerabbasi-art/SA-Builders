using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.Models
{
    // e.g. "Grey Structure" vs "A+ Turnkey" — the base rate the estimator
    // multiplies covered area by, before material add-ons are applied.
    public class ServiceTier
    {
        public int Id { get; set; }

        [Required, StringLength(ValidationConstants.ShortTextMaxLength, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(ValidationConstants.LongTextMaxLength)]
        public string? Description { get; set; }

        [Range(0, ValidationConstants.MaxCostImpact)]
        public decimal BaseRatePerSqFt { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, 1000)]
        public int SortOrder { get; set; } = 0;
    }
}