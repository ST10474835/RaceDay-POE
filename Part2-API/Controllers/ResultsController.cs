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
    [Route("api/results")]
    [SessionAuthorize]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        
        /// Captures a finish time and finishing position for a participant's enrolment. Only the organiser who created the event may do this.
        
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult([FromBody] ResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentId!.Value);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.Category!.Event!.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only capture results for events that you created." });

            if (await _context.Results.AnyAsync(r => r.EnrolmentID == enrolment.EnrolmentID))
                return Conflict(new { message = "A result has already been captured for this enrolment." });

            var result = new Result
            {
                EnrolmentID = enrolment.EnrolmentID,
                FinishTime = dto.FinishTime!.Value,
                Position = dto.Position!.Value
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Created("/api/results/mine", new
            {
                resultId = result.ResultID,
                enrolmentId = result.EnrolmentID,
                participantName = enrolment.Participant!.FullName,
                eventName = enrolment.Category.Event.EventName,
                categoryName = enrolment.Category.CategoryName,
                finishTime = result.FinishTime,
                position = result.Position,
                dateCaptured = result.DateCaptured
            });
        }

        
        /// Returns the logged-in participant's own results. Participant only.
       
        [HttpGet("mine")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = HttpContext.Session.GetUserId()!.Value;

            var results = await _context.Results
                .Where(r => r.Enrolment!.ParticipantID == participantId)
                .OrderBy(r => r.Enrolment!.Category!.Event!.EventDate)
                .Select(r => new
                {
                    resultId = r.ResultID,
                    enrolmentId = r.EnrolmentID,
                    eventId = r.Enrolment!.Category!.EventID,
                    eventName = r.Enrolment.Category.Event!.EventName,
                    eventDate = r.Enrolment.Category.Event.EventDate,
                    categoryName = r.Enrolment.Category.CategoryName,
                    finishTime = r.FinishTime,
                    position = r.Position,
                    dateCaptured = r.DateCaptured
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
