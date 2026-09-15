using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.Models;
using SA_Builders.Services;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/projects/{projectId}/documents")]
    [Authorize(Roles = "Admin")]
    public class AdminDocumentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISupabaseStorageService _storage;

        public AdminDocumentsController(AppDbContext db, ISupabaseStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int projectId)
        {
            var documents = await _db.ProjectDocuments
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            var response = new List<object>();

            foreach (var doc in documents)
            {
                // Generate a 10-minute signed URL per document for admin viewing/downloading
                var signedUrl = await _storage.GetSignedUrlAsync(doc.FileUrl, expiresInSeconds: 600);

                response.Add(new
                {
                    doc.Id,
                    doc.ProjectId,
                    doc.FileName,
                    doc.DocumentType,
                    doc.UploadedAt,
                    doc.UploadedByUserId,
                    FileUrl = doc.FileUrl, // Storage path
                    SignedUrl = signedUrl
                });
            }

            return Ok(response);
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)] // 20MB
        public async Task<IActionResult> Upload(int projectId, IFormFile file, [FromForm] string fileName, [FromForm] DocumentType documentType)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project is null) return NotFound(new { message = "Project not found." });

            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".dwg" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Only .pdf, .jpg, .jpeg, .png, .dwg files are allowed." });

            // Create path inside private bucket: projects/{projectId}/{guid}.ext
            var storagePath = $"projects/{projectId}/{Guid.NewGuid()}{ext}";

            using var stream = file.OpenReadStream();

            // Upload to private Supabase bucket (returns relative storage path)
            var savedPath = await _storage.UploadPrivateAsync(storagePath, stream, file.ContentType);

            var document = new ProjectDocument
            {
                ProjectId = projectId,
                FileName = fileName,
                FileUrl = savedPath, // Store path for signed URL generation
                DocumentType = documentType,
                UploadedByUserId = GetCurrentAdminId(),
                UploadedAt = DateTime.UtcNow
            };

            _db.ProjectDocuments.Add(document);

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "UploadedDocument",
                Details = $"Uploaded '{fileName}' ({documentType}) to project {projectId}"
            });

            await _db.SaveChangesAsync();

            // Generate an initial signed URL for immediate response/preview
            var signedUrl = await _storage.GetSignedUrlAsync(savedPath, expiresInSeconds: 600);

            return Ok(new
            {
                message = "Document uploaded.",
                documentId = document.Id,
                path = savedPath,
                signedUrl
            });
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(int projectId, int documentId)
        {
            var document = await _db.ProjectDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ProjectId == projectId);

            if (document is null) return NotFound(new { message = "Document not found." });

            // Delete object from Supabase private storage
            await _storage.DeleteDocumentAsync(document.FileUrl);

            _db.ProjectDocuments.Remove(document);

            _db.AdminActionLogs.Add(new AdminActionLog
            {
                AdminUserId = GetCurrentAdminId(),
                Action = "DeletedDocument",
                Details = $"Deleted document {documentId} from project {projectId}"
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Document deleted." });
        }

        private int GetCurrentAdminId()
        {
            var idClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return idClaim is null ? 0 : int.Parse(idClaim.Value);
        }
    }
}