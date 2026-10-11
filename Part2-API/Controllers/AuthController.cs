using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;


namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Creates a new user account with a chosen role (Organiser or Participant).
        /// </summary>
        /// <param name="dto">Full name, email, password (8 to 72 characters) and role ("Organiser" or "Participant").</param>
        /// <response code="201">Account created. Returns the new user's ID, name, email and role.</response>
        /// <response code="400">Missing or invalid fields, or the role is not Organiser or Participant.</response>
        /// <response code="409">An account with this email already exists.</response>

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict(new { message = "An account with this email already exists." });

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null)
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID = role.RoleID
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                userId = user.UserID,
                fullName = user.FullName,
                email = user.Email,
                role = role.RoleName
            });
        }

        /// <summary>
        /// Authenticates a user and starts a server-side session storing their user ID and role.
        /// </summary>
        /// <param name="dto">The account's email and password.</param>
        /// <response code="200">Login successful and session created. Returns the user's ID, name, email and role.</response>
        /// <response code="401">Invalid email or password.</response>
        
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role!.RoleName);

            return Ok(new
            {
                userId = user.UserID,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role.RoleName
            });
        }

        /// <summary>
        /// Ends the current user's session.
        /// </summary>
        /// <response code="200">Session ended.</response>
        /// <response code="401">No active session, so the user is not logged in.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return Unauthorized(new { message = "You are not logged in." });

            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
