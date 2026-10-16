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
    public class PaymentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public PaymentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Payments
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int userId))
            {
                return Unauthorized();
            }

            var isOrganiser = User.IsInRole("Organiser");

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => isOrganiser
                    ? p.Enrolment.Category.Event.OrganiserId == userId
                    : p.Enrolment.UserId == userId)
                .ToListAsync();

            return Ok(payments);
        }

        // POST: api/Payments
        // Participants can record a payment for their own enrolment.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreatePayment(
            [FromBody] CreatePaymentRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId");

            if (claim == null ||
                !int.TryParse(claim.Value, out int participantId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId &&
                    e.UserId == participantId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Your enrolment was not found."
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    message = "Amount must be greater than zero."
                });
            }

            if (request.Amount != enrolment.Category.EntryFee)
            {
                return BadRequest(new
                {
                    message = "Amount must match the category entry fee."
                });
            }

            var payment = new Payment
            {
                EnrolmentId = request.EnrolmentId,
                Amount = request.Amount,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Pending",
                PaymentMethod = request.PaymentMethod
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Payment record created successfully.",
                payment.PaymentId,
                payment.EnrolmentId,
                payment.Amount,
                payment.PaymentDate,
                payment.PaymentStatus,
                payment.PaymentMethod
            });
        }
    }

    public class CreatePaymentRequest
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}