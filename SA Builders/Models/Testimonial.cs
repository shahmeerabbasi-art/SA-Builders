namespace SA_Builders.Models
{
    public class Testimonial
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public int Rating { get; set; } = 5;
        public bool Approved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}