using Microsoft.EntityFrameworkCore;
using SA_Builders.Models;

namespace SA_Builders.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<Certification> Certifications => Set<Certification>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<MaterialSpec> MaterialSpecs => Set<MaterialSpec>();
        public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
        public DbSet<ActivationToken> ActivationTokens => Set<ActivationToken>();
        public DbSet<AdminActionLog> AdminActionLogs => Set<AdminActionLog>();

        public DbSet<ServiceTier> ServiceTiers => Set<ServiceTier>();

        public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<ProjectAssignment>()
                .HasIndex(pa => new { pa.UserId, pa.ProjectId }).IsUnique();

            modelBuilder.Entity<ProjectAssignment>()
                .HasOne(pa => pa.User).WithMany(u => u.ProjectAssignments)
                .HasForeignKey(pa => pa.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectAssignment>()
                .HasOne(pa => pa.Project).WithMany(p => p.Assignments)
                .HasForeignKey(pa => pa.ProjectId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivationToken>().HasIndex(t => t.Token).IsUnique();

            modelBuilder.Entity<PasswordResetToken>().HasIndex(t => t.Token).IsUnique();

            modelBuilder.Entity<ProjectImage>()
                .HasOne(pi => pi.Project).WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProjectId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Testimonial>()
                .HasOne(t => t.Project).WithMany(p => p.Testimonials)
                .HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProjectDocument>().HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}