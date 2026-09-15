namespace SA_Builders.Models
{
    public class MaterialSpec
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string BrandOption { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public decimal? CostImpactPerSqFt { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}