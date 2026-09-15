using SA_Builders.Models;

namespace SA_Builders.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(AppDbContext db, IConfiguration config)
        {
            if (db.Users.Any(u => u.Role == UserRole.Admin)) return; // already seeded

            var adminEmail = config["Seed:AdminEmail"] ?? "admin@sabuilders.local";
            var adminPassword = config["Seed:AdminPassword"] ?? "ChangeMe123!";

            var admin = new User
            {
                FullName = "Site Administrator",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = UserRole.Admin,
                IsActivated = true
            };

            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}