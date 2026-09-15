using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.DTOs
{
    public class CalculateEstimateDto
    {
        [Required(ErrorMessage = "Covered area is required.")]
        [Range(ValidationConstants.MinCoveredAreaSqFt, ValidationConstants.MaxCoveredAreaSqFt,
            ErrorMessage = "Covered area must be between {1} and {2} sq ft.")]
        public double CoveredAreaSqFt { get; set; }

        [Required(ErrorMessage = "A service tier must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "ServiceTierId must be a valid, positive ID.")]
        public int ServiceTierId { get; set; }

        // Optional — IDs of MaterialSpec rows the user picked (e.g. one per
        // category: a specific steel grade, a specific piping brand, etc.)
        public List<int> SelectedMaterialSpecIds { get; set; } = new();
    }

    public class EstimateBreakdownLineDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal RatePerSqFt { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class EstimateResultDto
    {
        public double CoveredAreaSqFt { get; set; }
        public string ServiceTierName { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public List<EstimateBreakdownLineDto> MaterialAddOns { get; set; } = new();
        public decimal TotalEstimatedCost { get; set; }
    }
}