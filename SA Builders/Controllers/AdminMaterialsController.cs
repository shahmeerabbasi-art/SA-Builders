using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/materials")]
    [Authorize(Roles = "Admin")]
    public class AdminMaterialsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminMaterialsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var materials = await _db.MaterialSpecs
                .OrderBy(m => m.Category).ThenBy(m => m.SortOrder)
                .ToListAsync();

            return Ok(materials);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaterialSpecDto dto)
        {
            var material = new MaterialSpec
            {
                Category = dto.Category,
                BrandOption = dto.BrandOption,
                Details = dto.Details,
                CostImpactPerSqFt = dto.CostImpactPerSqFt,
                SortOrder = dto.SortOrder
            };

            _db.MaterialSpecs.Add(material);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Material spec created.", id = material.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateMaterialSpecDto dto)
        {
            var material = await _db.MaterialSpecs.FindAsync(id);
            if (material is null) return NotFound(new { message = "Material spec not found." });

            material.Category = dto.Category;
            material.BrandOption = dto.BrandOption;
            material.Details = dto.Details;
            material.CostImpactPerSqFt = dto.CostImpactPerSqFt;
            material.SortOrder = dto.SortOrder;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Material spec updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _db.MaterialSpecs.FindAsync(id);
            if (material is null) return NotFound(new { message = "Material spec not found." });

            _db.MaterialSpecs.Remove(material);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Material spec deleted." });
        }
    }
}