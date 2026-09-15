using Microsoft.AspNetCore.Mvc;
using SA_Builders.Data;
using SA_Builders.DTOs;
using SA_Builders.Models;

namespace SA_Builders.Controllers
{
    [ApiController]
    [Route("api/leads")]
    public class LeadsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LeadsController(AppDbContext db) { _db = db; }

        // Public — contact form, WhatsApp click, and cost estimator all post here.
        [HttpPost]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadDto dto)
        {
            var lead = new Lead
            {
                Name = dto.Name,
                Contact = dto.Contact,
                Source = dto.Source,
                Message = dto.Message,
                EstimatedCoveredArea = dto.EstimatedCoveredArea,
                SelectedServiceTier = dto.SelectedServiceTier,
                EstimatedCost = dto.EstimatedCost,
                Status = LeadStatus.New
            };

            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Thanks — we'll be in touch shortly." });
        }
    }
}