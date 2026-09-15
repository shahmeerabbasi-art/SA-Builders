namespace SA_Builders.Models
{
    // Explicit join table so a user (e.g. a housing society exec) can be
    // linked to more than one project without restructuring later.
    public class ProjectAssignment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public int AssignedByAdminId { get; set; }
    }
}