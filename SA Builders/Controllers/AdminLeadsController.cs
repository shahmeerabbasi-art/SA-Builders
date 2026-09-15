using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/admin/leads")]
    [Authorize(Roles = "Admin")]
    public class AdminLeadsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminLeadsController(AppDbContext db) { _db = db; }

        // Optional ?status=New filter — lets the future UI show a
        // "New leads only" tab without fetching everything every time.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] LeadStatus? status)
        {
            var query = _db.Leads.AsQueryable();

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            var leads = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.Contact,
                    Source = l.Source.ToString(),
                    l.Message,
                    l.EstimatedCoveredArea,
                    l.SelectedServiceTier,
                    l.EstimatedCost,
                    Status = l.Status.ToString(),
                    l.CreatedAt
                })
                .ToListAsync();

            return Ok(leads);
        }

        public class UpdateLeadStatusDto
        {
            public LeadStatus Status { get; set; }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLeadStatusDto dto)
        {
            var lead = await _db.Leads.FindAsync(id);
            if (lead is null) return NotFound(new { message = "Lead not found." });

            lead.Status = dto.Status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Lead status updated.", status = lead.Status.ToString() });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _db.Leads.FindAsync(id);
            if (lead is null) return NotFound(new { message = "Lead not found." });

            _db.Leads.Remove(lead);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Lead deleted." });
        }
    }
}