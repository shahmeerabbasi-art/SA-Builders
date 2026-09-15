using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/testimonials")]
    [Authorize(Roles = "Admin")]
    public class AdminTestimonialsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminTestimonialsController(AppDbContext db) { _db = db; }

        // Admin sees pending AND approved — unlike the public endpoint (approved only).
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var testimonials = await _db.Testimonials
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.ClientName,
                    t.Content,
                    t.MediaUrl,
                    t.Rating,
                    t.Approved,
                    t.ProjectId,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(testimonials);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _db.Testimonials.FindAsync(id);
            if (testimonial is null) return NotFound(new { message = "Testimonial not found." });

            testimonial.Approved = true;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Testimonial approved and now visible publicly." });
        }

        [HttpPut("{id}/unapprove")]
        public async Task<IActionResult> Unapprove(int id)
        {
            var testimonial = await _db.Testimonials.FindAsync(id);
            if (testimonial is null) return NotFound(new { message = "Testimonial not found." });

            testimonial.Approved = false;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Testimonial hidden from public view." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await _db.Testimonials.FindAsync(id);
            if (testimonial is null) return NotFound(new { message = "Testimonial not found." });

            _db.Testimonials.Remove(testimonial);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Testimonial deleted." });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTestimonialDto dto)
        {
            var testimonial = new Testimonial
            {
                ClientName = dto.ClientName,
                Content = dto.Content,
                MediaUrl = dto.MediaUrl,
                Rating = dto.Rating,
                ProjectId = dto.ProjectId,
                Approved = dto.Approved
            };

            _db.Testimonials.Add(testimonial);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Testimonial created.", id = testimonial.Id, approved = testimonial.Approved });
        }
    }
}