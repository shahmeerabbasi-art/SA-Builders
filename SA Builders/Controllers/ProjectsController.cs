using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProjectsController(AppDbContext db) { _db = db; }

        // Public portfolio — only published projects, only public images.
        [HttpGet]
        public async Task<IActionResult> GetPublicPortfolio()
        {
            var projects = await _db.Projects
                .Where(p => p.ShowOnPublicPortfolio)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Location,
                    p.CoveredAreaSqFt,
                    ConstructionType = p.ConstructionType.ToString(),
                    p.Description,
                    Images = p.Images.Where(i => i.IsPublic).Select(i => new { i.ImageUrl, i.Caption })
                })
                .ToListAsync();

            return Ok(projects);
        }
    }
}