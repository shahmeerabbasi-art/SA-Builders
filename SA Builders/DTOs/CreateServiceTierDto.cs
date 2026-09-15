using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.DTOs
{
    public class CreateServiceTierDto
    {
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

    public class CreateMaterialSpecDto
    {
        [Required, StringLength(ValidationConstants.ShortTextMaxLength, MinimumLength = 2)]
        public string Category { get; set; } = string.Empty;

        [Required, StringLength(ValidationConstants.ShortTextMaxLength, MinimumLength = 2)]
        public string BrandOption { get; set; } = string.Empty;

        [Required, StringLength(ValidationConstants.LongTextMaxLength, MinimumLength = 2)]
        public string Details { get; set; } = string.Empty;

        [Range((double)ValidationConstants.MinCostImpact, (double)ValidationConstants.MaxCostImpact)]
        public decimal? CostImpactPerSqFt { get; set; }

        [Range(0, 1000)]
        public int SortOrder { get; set; } = 0;
    }
}