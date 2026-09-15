using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/service-tiers")]
    [Authorize(Roles = "Admin")]
    public class AdminServiceTiersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminServiceTiersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tiers = await _db.ServiceTiers.OrderBy(t => t.SortOrder).ToListAsync();
            return Ok(tiers);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceTierDto dto)
        {
            var tier = new ServiceTier
            {
                Name = dto.Name,
                Description = dto.Description,
                BaseRatePerSqFt = dto.BaseRatePerSqFt,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            };

            _db.ServiceTiers.Add(tier);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Service tier created.", id = tier.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateServiceTierDto dto)
        {
            var tier = await _db.ServiceTiers.FindAsync(id);
            if (tier is null) return NotFound(new { message = "Service tier not found." });

            tier.Name = dto.Name;
            tier.Description = dto.Description;
            tier.BaseRatePerSqFt = dto.BaseRatePerSqFt;
            tier.IsActive = dto.IsActive;
            tier.SortOrder = dto.SortOrder;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Service tier updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tier = await _db.ServiceTiers.FindAsync(id);
            if (tier is null) return NotFound(new { message = "Service tier not found." });

            _db.ServiceTiers.Remove(tier);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Service tier deleted." });
        }
    }
}