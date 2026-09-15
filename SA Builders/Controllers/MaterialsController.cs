using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/materials")]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MaterialsController(AppDbContext db) { _db = db; }

        // Grouped by category so the frontend can render the
        // "Material Transparency Board" tabs directly from this response.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var materials = await _db.MaterialSpecs
                .OrderBy(m => m.Category).ThenBy(m => m.SortOrder)
                .ToListAsync();

            var grouped = materials
                .GroupBy(m => m.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Options = g.Select(m => new
                    {
                        m.Id,
                        m.BrandOption,
                        m.Details,
                        m.CostImpactPerSqFt
                    })
                });

            return Ok(grouped);
        }
    }
}