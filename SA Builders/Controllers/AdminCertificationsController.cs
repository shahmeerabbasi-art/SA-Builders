using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/certifications")]
    [Authorize(Roles = "Admin")]
    public class AdminCertificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminCertificationsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var certs = await _db.Certifications
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return Ok(certs);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCertificationDto dto)
        {
            var cert = new Certification
            {
                Name = dto.Name,
                IssuingBody = dto.IssuingBody,
                DisplayOnHome = dto.DisplayOnHome,
                SortOrder = dto.SortOrder
            };

            _db.Certifications.Add(cert);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Certification created.", id = cert.Id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cert = await _db.Certifications.FindAsync(id);
            if (cert is null) return NotFound(new { message = "Certification not found." });

            _db.Certifications.Remove(cert);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Certification deleted." });
        }
    }
}