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
    [Route("api/events")]
    [SessionAuthorize]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        private static object ToResponse(Event e) => new
        {
            eventId = e.EventID,
            organiserId = e.OrganiserID,
            organiserName = e.Organiser?.FullName,
            eventName = e.EventName,
            description = e.Description,
            eventDate = e.EventDate,
            location = e.Location,
            distance = e.Distance,
            eventType = e.EventType,
            dateCreated = e.DateCreated
        };

        /// <summary>
        /// Lists all events. Available to any logged-in user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToResponse));
        }

        /// <summary>
        /// Returns the details of a single event. Available to any logged-in user.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ToResponse(ev));
        }

        /// <summary>
        /// Creates a new event owned by the logged-in organiser. Organiser only.
        /// </summary>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
        {
            var ev = new Event
            {
                OrganiserID = HttpContext.Session.GetUserId()!.Value,
                EventName = dto.EventName.Trim(),
                Description = dto.Description?.Trim(),
                EventDate = dto.EventDate!.Value,
                Location = dto.Location.Trim(),
                Distance = dto.Distance!.Value,
                EventType = dto.EventType
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            await _context.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventID }, ToResponse(ev));
        }

        /// <summary>
        /// Updates an existing event. Only the organiser who created it may do this.
        /// </summary>
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto dto)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only modify events that you created." });

            ev.EventName = dto.EventName.Trim();
            ev.Description = dto.Description?.Trim();
            ev.EventDate = dto.EventDate!.Value;
            ev.Location = dto.Location.Trim();
            ev.Distance = dto.Distance!.Value;
            ev.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(ToResponse(ev));
        }

        /// <summary>
        /// Deletes an event and its categories, enrolments and results.
        /// Only the organiser who created it may do this.
        /// </summary>
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only delete events that you created." });

            // The database uses NO ACTION foreign keys (as in the Part 1 script),
            // so dependent rows are removed explicitly, all in one SaveChanges transaction.
            var categories = await _context.Categories.Where(c => c.EventID == id).ToListAsync();
            var categoryIds = categories.Select(c => c.CategoryID).ToList();
            var enrolments = await _context.Enrolments
                .Where(en => categoryIds.Contains(en.CategoryID)).ToListAsync();
            var enrolmentIds = enrolments.Select(en => en.EnrolmentID).ToList();
            var results = await _context.Results
                .Where(r => enrolmentIds.Contains(r.EnrolmentID)).ToListAsync();

            _context.Results.RemoveRange(results);
            _context.Enrolments.RemoveRange(enrolments);
            _context.Categories.RemoveRange(categories);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}
