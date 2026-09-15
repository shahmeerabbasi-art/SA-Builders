namespace SA_Builders.Models
{
    public class ProjectImage
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }

        // false = private progress photo (client + admin only)
        // true  = public portfolio image
        public bool IsPublic { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int UploadedByUserId { get; set; }
    }
}