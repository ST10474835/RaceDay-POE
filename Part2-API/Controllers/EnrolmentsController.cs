using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [SessionAuthorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrolmentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Enrols the logged-in participant into a chosen category, linking the participant, the event and the category. Participant only.
        /// </summary>
        [HttpPost("enrolments")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol([FromBody] EnrolmentDto dto)
        {
            var participantId = HttpContext.Session.GetUserId()!.Value;
            var categoryId = dto.CategoryId!.Value;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null)
                return BadRequest(new { message = "The selected category does not exist." });

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantID == participantId && e.CategoryID == categoryId);

            if (alreadyEnrolled)
                return Conflict(new { message = "You are already enrolled in this category." });

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = categoryId
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Created("/api/enrolments/mine", new
            {
                enrolmentId = enrolment.EnrolmentID,
                participantId = enrolment.ParticipantID,
                categoryId = category.CategoryID,
                categoryName = category.CategoryName,
                eventId = category.EventID,
                eventName = category.Event!.EventName,
                enrolmentDate = enrolment.EnrolmentDate
            });
        }

        /// <summary>
        /// Returns all of the logged-in participant's own enrolments. Participant only.
        /// </summary>
        [HttpGet("enrolments/mine")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var participantId = HttpContext.Session.GetUserId()!.Value;

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participantId)
                .OrderBy(e => e.EnrolmentDate)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    categoryId = e.CategoryID,
                    categoryName = e.Category!.CategoryName,
                    categoryType = e.Category.CategoryType,
                    eventId = e.Category.EventID,
                    eventName = e.Category.Event!.EventName,
                    eventDate = e.Category.Event.EventDate,
                    enrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Returns all enrolments for an event, with participant details. Only the organiser who created the event may do this.
        /// </summary>
        [HttpGet("events/{eventId:int}/enrolments")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only view enrolments for events that you created." });

            var enrolments = await _context.Enrolments
                .Where(e => e.Category!.EventID == eventId)
                .OrderBy(e => e.Category!.CategoryName)
                .ThenBy(e => e.Participant!.FullName)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    participantId = e.ParticipantID,
                    participantName = e.Participant!.FullName,
                    participantEmail = e.Participant.Email,
                    categoryId = e.CategoryID,
                    categoryName = e.Category!.CategoryName,
                    enrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
