using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.Services;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/client")]
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IClientAccessGuard _accessGuard;
        private readonly ISupabaseStorageService _storage;

        public ClientDashboardController(AppDbContext db, IClientAccessGuard accessGuard, ISupabaseStorageService storage)
        {
            _db = db;
            _accessGuard = accessGuard;
            _storage = storage;
        }

        // List every project this client is assigned to — usually just one,
        // but a housing society exec could have several.
        [HttpGet("projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetCurrentUserId();
            var projectIds = await _accessGuard.GetAssignedProjectIdsAsync(userId);

            var projects = await _db.Projects
                .Where(p => projectIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Location,
                    p.CoveredAreaSqFt,
                    ConstructionType = p.ConstructionType.ToString(),
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Ok(projects);
        }

        // Milestone tracker for ONE project — the core progress view.
        [HttpGet("projects/{projectId}/milestones")]
        public async Task<IActionResult> GetMilestones(int projectId)
        {
            var userId = GetCurrentUserId();

            if (!await _accessGuard.UserCanAccessProjectAsync(userId, projectId))
                return Forbid(); // this client is not assigned to this project — never leak that it exists

            var project = await _db.Projects.FindAsync(projectId);
            if (project is null) return NotFound(new { message = "Project not found." });

            // Ordered stage list — the frontend can highlight everything up
            // to and including project.Status as "complete".
            var allStages = Enum.GetNames(typeof(Models.ProjectStatus));

            return Ok(new
            {
                projectId = project.Id,
                projectTitle = project.Title,
                currentStatus = project.Status.ToString(),
                stages = allStages
            });
        }

        // Private + public photo feed for ONE project, newest first —
        // the "rolling timestamped feed" from the original spec.
        [HttpGet("projects/{projectId}/photos")]
        public async Task<IActionResult> GetPhotos(int projectId)
        {
            var userId = GetCurrentUserId();

            if (!await _accessGuard.UserCanAccessProjectAsync(userId, projectId))
                return Forbid();

            var photos = await _db.ProjectImages
                .Where(pi => pi.ProjectId == projectId)
                .OrderByDescending(pi => pi.UploadedAt)
                .Select(pi => new { pi.Id, pi.ImageUrl, pi.Caption, pi.UploadedAt })
                .ToListAsync();

            return Ok(photos);
        }

        // Document list for ONE project — contracts, blueprints, NOC, floor plans.
        [HttpGet("projects/{projectId}/documents")]
        public async Task<IActionResult> GetDocuments(int projectId)
        {
            var userId = GetCurrentUserId();

            if (!await _accessGuard.UserCanAccessProjectAsync(userId, projectId))
                return Forbid();

            var documents = await _db.ProjectDocuments
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            var response = new List<object>();
            foreach (var doc in documents)
            {
                var signedUrl = await _storage.GetSignedUrlAsync(doc.FileUrl, expiresInSeconds: 600);
                response.Add(new
                {
                    doc.Id,
                    doc.FileName,
                    DocumentType = doc.DocumentType.ToString(),
                    doc.UploadedAt,
                    SignedUrl = signedUrl
                });
            }

            return Ok(response);
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return idClaim is null ? 0 : int.Parse(idClaim.Value);
        }
    }
}