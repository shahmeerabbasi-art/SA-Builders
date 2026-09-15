using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/service-tiers")]
    public class ServiceTiersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ServiceTiersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var tiers = await _db.ServiceTiers
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .Select(t => new { t.Id, t.Name, t.Description, t.BaseRatePerSqFt })
                .ToListAsync();

            return Ok(tiers);
        }
    }
}