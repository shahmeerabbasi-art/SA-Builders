namespace SA_Builders.Models
{
    public enum LeadSource { ContactForm, WhatsApp, CostEstimator, PhoneCall }
    public enum LeadStatus { New, Contacted, Qualified, Converted, Lost }

    public class Lead
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public LeadSource Source { get; set; }
        public string? Message { get; set; }
        public double? EstimatedCoveredArea { get; set; }
        public string? SelectedServiceTier { get; set; }
        public decimal? EstimatedCost { get; set; }
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}