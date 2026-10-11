using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [SessionAuthorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the logged-in user's own profile details.
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetUserId()!.Value;

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return Unauthorized(new { message = "Your account no longer exists." });

            return Ok(new
            {
                fullName = user.FullName,
                email = user.Email,
                role = user.Role!.RoleName,
                dateRegistered = user.DateRegistered
            });
        }

        /// <summary>
        /// Updates the logged-in user's own full name and email.
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return Unauthorized(new { message = "Your account no longer exists." });

            var email = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email == email && u.UserID != userId))
                return BadRequest(new { message = "That email address is already in use." });

            user.FullName = dto.FullName.Trim();
            user.Email = email;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                fullName = user.FullName,
                email = user.Email,
                role = user.Role!.RoleName,
                dateRegistered = user.DateRegistered
            });
        }
    }
}
