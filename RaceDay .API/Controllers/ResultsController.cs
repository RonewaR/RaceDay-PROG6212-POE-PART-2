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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Participants view their own results.
        [HttpGet]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetResults()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int participantId))
            {
                return Unauthorized();
            }

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.UserId == participantId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    EventTitle = r.Enrolment.Category.Event.Title,
                    CategoryName = r.Enrolment.Category.Name,
                    r.FinishTime,
                    r.Position,
                    r.IsDnf,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/Results
        // Organisers record results for their own events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            [FromBody] CreateResultRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int organiserId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            if (enrolment.Category.Event.OrganiserId != organiserId)
            {
                return Forbid();
            }

            var existingResult = await _context.Results
                .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

            if (existingResult)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            if (request.Position.HasValue && request.Position.Value < 1)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            if (request.IsDnf)
            {
                request.FinishTime = null;
                request.Position = null;
            }

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                IsDnf = request.IsDnf,
                CreatedAt = DateTime.Now
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.IsDnf,
                result.CreatedAt
            });
        }
    }

    public class CreateResultRequest
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        public TimeOnly? FinishTime { get; set; }

        public int? Position { get; set; }

        public bool IsDnf { get; set; }
    }
}