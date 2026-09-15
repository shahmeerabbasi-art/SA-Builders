using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/testimonials")]
    public class TestimonialsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TestimonialsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetApprovedTestimonials()
        {
            var testimonials = await _db.Testimonials
                .Where(t => t.Approved)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new { t.ClientName, t.Content, t.MediaUrl, t.Rating, t.ProjectId })
                .ToListAsync();

            return Ok(testimonials);
        }
    }
}