using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Helpers;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [SessionAuthorize]
    public class UploadsController : ControllerBase
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private const string BannerContainer = "event-banners";
        private const string ProfileContainer = "profile-pictures";
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly AppDbContext _context;
        private readonly IBlobStorageService _blobStorage;

        public UploadsController(AppDbContext context, IBlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
        }

        private static string? ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Please choose an image file to upload.";

            if (file.Length > MaxFileSizeBytes)
                return "The image must be 5 MB or smaller.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
                return "Only JPG, PNG, GIF or WEBP images are allowed.";

            return null;
        }

        /// <summary>
        /// Uploads a banner image for an event to Azure Blob Storage and saves its URL. Only the organiser who created the event may do this.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="file">The image file (JPG, PNG, GIF or WEBP, up to 5 MB).</param>
        /// <response code="200">Banner stored. Returns the event ID and the image URL.</response>
        /// <response code="400">No file, wrong file type or file too large.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Logged in as a Participant, or the event belongs to another organiser.</response>
        /// <response code="404">No event exists with this ID.</response>
        /// <response code="502">Azure Blob Storage could not store the image.</response>
        [HttpPost("events/{eventId:int}/banner")]
        [SessionAuthorize("Organiser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> UploadEventBanner(int eventId, IFormFile? file)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != HttpContext.Session.GetUserId())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only upload banners for events that you created." });

            var error = ValidateImage(file);
            if (error != null)
                return BadRequest(new { message = error });

            try
            {
                ev.BannerUrl = await _blobStorage.UploadAsync(BannerContainer, file);
                await _context.SaveChangesAsync();
            }
            catch (RequestFailedException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "The image could not be stored. Please try again." });
            }

            return Ok(new { eventId = ev.EventID, bannerUrl = ev.BannerUrl });
        }

        /// <summary>
        /// Uploads a profile picture for the logged-in user to Azure Blob Storage and saves its URL. Any logged-in role.
        /// </summary>
        /// <param name="file">The image file (JPG, PNG, GIF or WEBP, up to 5 MB).</param>
        /// <response code="200">Picture stored. Returns the image URL.</response>
        /// <response code="400">No file, wrong file type or file too large.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="502">Azure Blob Storage could not store the image.</response>
        [HttpPost("users/profile/picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile? file)
        {
            var error = ValidateImage(file);
            if (error != null)
                return BadRequest(new { message = error });

            var userId = HttpContext.Session.GetUserId()!.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return Unauthorized(new { message = "Your account no longer exists." });

            try
            {
                user.ProfilePictureUrl = await _blobStorage.UploadAsync(ProfileContainer, file);
                await _context.SaveChangesAsync();
            }
            catch (RequestFailedException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "The image could not be stored. Please try again." });
            }

            return Ok(new { profilePictureUrl = user.ProfilePictureUrl });
        }
    }
}
