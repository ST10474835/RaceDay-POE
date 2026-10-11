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
    [Route("api/events/{eventId:int}/categories")]
    [SessionAuthorize]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lists all age and distance categories defined for an event. Any logged-in role.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <response code="200">Returns the event's categories.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">No event exists with this ID.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!await _context.Events.AnyAsync(e => e.EventID == eventId))
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .OrderBy(c => c.CategoryName)
                .Select(c => new
                {
                    categoryId = c.CategoryID,
                    eventId = c.EventID,
                    categoryName = c.CategoryName,
                    categoryType = c.CategoryType
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Defines a new age or distance category for an event. Only the organiser who created the event may do this.
        /// </summary>
        /// <param name="eventId">The ID of the event the category belongs to.</param>
        /// <param name="dto">Category name (for example "10km" or "Under 20") and category type ("Age" or "Distance").</param>
        /// <response code="201">Category created. Returns the new category.</response>
        /// <response code="400">Missing or invalid fields, for example a category type that is not Age or Distance.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Logged in as a Participant, or the event belongs to another organiser.</response>
        /// <response code="404">No event exists with this ID.</response>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only add categories to events that you created." });

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName.Trim(),
                CategoryType = dto.CategoryType
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Created($"/api/events/{eventId}/categories", new
            {
                categoryId = category.CategoryID,
                eventId = category.EventID,
                categoryName = category.CategoryName,
                categoryType = category.CategoryType
            });
        }
    }
}
