using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using RaceDay_.API.Data;
using RaceDay_.API.Models;

namespace RaceDay_.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Organisers can view enrolments for their own events.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolments()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int organiserId))
            {
                return Unauthorized();
            }

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.Category.Event.OrganiserId == organiserId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.UserId,
                    ParticipantName = e.User.FullName,
                    e.CategoryId,
                    CategoryName = e.Category.Name,
                    EventId = e.Category.EventId,
                    EventTitle = e.Category.Event.Title,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/Enrolments
        // Participants enrol themselves in a category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int participantId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .FindAsync(request.CategoryId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "The selected category does not exist."
                });
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.UserId == participantId &&
                               e.CategoryId == request.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            var enrolment = new Enrolment
            {
                UserId = participantId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrolment created successfully.",
                enrolment.EnrolmentId,
                enrolment.UserId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
        }
    }

    public class CreateEnrolmentRequest
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}