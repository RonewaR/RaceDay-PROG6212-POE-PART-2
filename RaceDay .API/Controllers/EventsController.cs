using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using RaceDay_.API.Data;
using RaceDay_.API.Models;

namespace RaceDay_.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/Events
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            [FromBody] CreateEventRequest request)
        {
            var organiser = await _context.Users
                .FindAsync(request.OrganiserId);

            if (organiser == null || organiser.Role != "Organiser")
            {
                return BadRequest(new
                {
                    message = "A valid Organiser account is required. Check organiserId."
                });
            }

            var newEvent = new Event
            {
                OrganiserId = organiser.UserId,
                Organiser = organiser,
                Title = request.Title,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.Now
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Event created successfully.",
                eventId = newEvent.EventId,
                title = newEvent.Title,
                organiserId = newEvent.OrganiserId
            });
        }

        // PUT: api/Events/{id}
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, [FromBody] UpdateEventRequest request)
        {
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            existingEvent.Title = request.Title;
            existingEvent.Description = request.Description;
            existingEvent.EventDate = request.EventDate;
            existingEvent.Location = request.Location;
            existingEvent.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventId = existingEvent.EventId,
                title = existingEvent.Title
            });
        }

        // DELETE: api/Events/{id}
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }

    public class CreateEventRequest
    {
        [Required]
        public int OrganiserId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }

    public class UpdateEventRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }
}