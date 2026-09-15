using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;
using SA_Builders.Services;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/projects")]
    [Authorize(Roles = "Admin")]
    public class AdminProjectsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISupabaseStorageService _storage;

        public AdminProjectsController(AppDbContext db, ISupabaseStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _db.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Location,
                    p.CoveredAreaSqFt,
                    ConstructionType = p.ConstructionType.ToString(),
                    Status = p.Status.ToString(),
                    p.ShowOnPublicPortfolio,
                    p.CreatedAt,
                    ImageCount = p.Images.Count
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var adminId = GetCurrentAdminId();

            var project = new Project
            {
                Title = dto.Title,
                Location = dto.Location,
                CoveredAreaSqFt = dto.CoveredAreaSqFt,
                ConstructionType = dto.ConstructionType,
                Description = dto.Description,
                ShowOnPublicPortfolio = dto.ShowOnPublicPortfolio,
                CreatedByAdminId = adminId
            };

            _db.Projects.Add(project);

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = adminId,
                Action = "CreatedProject",
                Details = $"Created project '{project.Title}'"
            });

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = project.Id }, new { project.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project is null) return NotFound(new { message = "Project not found." });

            project.Title = dto.Title;
            project.Location = dto.Location;
            project.CoveredAreaSqFt = dto.CoveredAreaSqFt;
            project.ConstructionType = dto.ConstructionType;
            project.Description = dto.Description;
            project.Status = dto.Status;
            project.ShowOnPublicPortfolio = dto.ShowOnPublicPortfolio;

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "UpdatedProject",
                Details = $"Updated project {id}"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Project updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project is null) return NotFound(new { message = "Project not found." });

            _db.Projects.Remove(project); // cascades to Images/Assignments per DbContext config

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "DeletedProject",
                Details = $"Deleted project {id} ('{project.Title}')"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Project deleted." });
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetImages(int id)
        {
            var images = await _db.ProjectImages
                .Where(i => i.ProjectId == id)
                .OrderByDescending(i => i.UploadedAt)
                .Select(i => new { i.Id, i.ImageUrl, i.Caption, i.IsPublic, i.UploadedAt })
                .ToListAsync();

            return Ok(images);
        }

        // Image upload — now goes to Supabase Storage (project-images bucket, public).
        [HttpPost("{id}/images")]
        [RequestSizeLimit(10_000_000)] // 10MB cap
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromForm] bool isPublic = false, [FromForm] string? caption = null)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project is null) return NotFound(new { message = "Project not found." });

            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Only .jpg, .jpeg, .png, .webp files are allowed." });

            var storagePath = $"projects/{id}/{Guid.NewGuid()}{ext}";

            using var stream = file.OpenReadStream();
            var publicUrl = await _storage.UploadPublicAsync(storagePath, stream, file.ContentType);

            var image = new ProjectImage
            {
                ProjectId = id,
                ImageUrl = publicUrl, // full Supabase public URL now, not a relative path
                Caption = caption,
                IsPublic = isPublic,
                UploadedByUserId = GetCurrentAdminId()
            };

            _db.ProjectImages.Add(image);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Image uploaded.", url = publicUrl, imageId = image.Id });
        }

        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var image = await _db.ProjectImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ProjectId == id);

            if (image is null) return NotFound(new { message = "Image not found." });

            // Extract the storage path from the full public URL so we can also
            // delete the actual file from Supabase, not just the database row.
            var marker = "/project-images/";
            var markerIndex = image.ImageUrl.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                var storagePath = image.ImageUrl[(markerIndex + marker.Length)..];
                await _storage.DeleteImageAsync(storagePath);
            }
            // If the URL doesn't match this pattern (e.g. an old pre-Supabase
            // record), we still remove the database row below — just skip the
            // storage deletion since there's nothing real to delete there.

            _db.ProjectImages.Remove(image);

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "DeletedImage",
                Details = $"Deleted image {imageId} from project {id}"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Image deleted." });
        }

        private int GetCurrentAdminId()
        {
            var idClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return idClaim is null ? 0 : int.Parse(idClaim.Value);
        }
    }
}