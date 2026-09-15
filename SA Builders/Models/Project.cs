using System.ComponentModel.DataAnnotations;

namespace SA_Builders.Models
{
    public enum ProjectStatus { Foundation, Columns, Slab, Brickwork, Finishing, Completed }
    public enum ConstructionType { CustomVilla, HousingSociety, Commercial }

    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public double CoveredAreaSqFt { get; set; }
        public ConstructionType ConstructionType { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Foundation;
        public bool ShowOnPublicPortfolio { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByAdminId { get; set; }

        public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
        public ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
        public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
    }
}