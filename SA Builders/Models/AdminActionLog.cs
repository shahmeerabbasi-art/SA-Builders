namespace SA_Builders.Models
{
    public class AdminActionLog
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}