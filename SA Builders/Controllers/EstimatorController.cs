using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/estimate")]
    public class EstimatorController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EstimatorController(AppDbContext db) { _db = db; }

        // Public — no auth required. This only calculates and returns a
        // number; it does NOT save a lead. The frontend calls POST /api/leads
        // separately once the visitor is ready to submit their contact info.
        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] CalculateEstimateDto dto)
        {
            var tier = await _db.ServiceTiers.FindAsync(dto.ServiceTierId);
            if (tier is null || !tier.IsActive)
                return NotFound(new { message = "Selected service tier not found or no longer available." });

            var baseCost = tier.BaseRatePerSqFt * (decimal)dto.CoveredAreaSqFt;

            var addOns = new List<EstimateBreakdownLineDto>();
            decimal addOnTotal = 0m;

            if (dto.SelectedMaterialSpecIds.Any())
            {
                var materials = await _db.MaterialSpecs
                    .Where(m => dto.SelectedMaterialSpecIds.Contains(m.Id))
                    .ToListAsync();

                foreach (var material in materials)
                {
                    var rate = material.CostImpactPerSqFt ?? 0m;
                    var lineTotal = rate * (decimal)dto.CoveredAreaSqFt;
                    addOnTotal += lineTotal;

                    addOns.Add(new EstimateBreakdownLineDto
                    {
                        Label = $"{material.Category}: {material.BrandOption}",
                        RatePerSqFt = rate,
                        LineTotal = lineTotal
                    });
                }
            }

            var result = new EstimateResultDto
            {
                CoveredAreaSqFt = dto.CoveredAreaSqFt,
                ServiceTierName = tier.Name,
                BaseCost = baseCost,
                MaterialAddOns = addOns,
                TotalEstimatedCost = baseCost + addOnTotal
            };

            return Ok(result);
        }
    }
}