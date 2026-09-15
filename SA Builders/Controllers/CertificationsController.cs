using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/certifications")]
    public class CertificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CertificationsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetHomeCertifications()
        {
            var certs = await _db.Certifications
                .Where(c => c.DisplayOnHome)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return Ok(certs);
        }
    }
}