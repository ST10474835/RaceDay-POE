using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [RoleAuthorize("Participant")]
    public class ParticipantController : AppController
    {
        private readonly IApiClient _api;

        public ParticipantController(IApiClient api)
        {
            _api = api;
        }

        // ---------- event details and enrolment ----------

        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            var ev = await _api.GetAsync<EventItem>($"api/events/{id}");
            if (ev.IsUnauthorized) return SessionExpired();

            if (!ev.Success || ev.Data == null)
            {
                TempData["Error"] = ev.StatusCode == 404
                    ? "That event could not be found."
                    : ev.Message ?? "We could not load that event.";
                return RedirectToAction("Index", "Home");
            }

            var categories = await _api.GetAsync<List<CategoryItem>>($"api/events/{id}/categories");
            if (categories.IsUnauthorized) return SessionExpired();

            var mine = await _api.GetAsync<List<MyEnrolmentItem>>("api/enrolments/mine");
            if (mine.IsUnauthorized) return SessionExpired();

            var enrolled = (mine.Data ?? new List<MyEnrolmentItem>())
                .Where(e => e.EventId == id)
                .ToDictionary(e => e.CategoryId, e => e.Status);

            return View(new EventDetailsViewModel
            {
                Event = ev.Data,
                Categories = categories.Data ?? new List<CategoryItem>(),
                EnrolledStatus = enrolled,
                IsPast = ev.Data.EventDate < DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var result = await _api.PostAsync<object>("api/enrolments", new { categoryId });
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "We could not enter you into that event.";
                return RedirectToAction(nameof(EventDetails), new { id = eventId });
            }

            TempData["Success"] = "You are entered! Your enrolment has been recorded.";
            return RedirectToAction(nameof(MyEvents));
        }

        // ---------- my events and results ----------

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var result = await _api.GetAsync<List<MyEnrolmentItem>>("api/enrolments/mine");
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
                TempData["Error"] = result.Message ?? "We could not load your events.";

            var list = (result.Data ?? new List<MyEnrolmentItem>())
                .OrderBy(e => e.EventDate)
                .ToList();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var result = await _api.GetAsync<List<ResultItem>>("api/results/mine");
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
                TempData["Error"] = result.Message ?? "We could not load your results.";

            var list = (result.Data ?? new List<ResultItem>())
                .OrderByDescending(r => r.EventDate)
                .ToList();

            return View(list);
        }

        // ---------- profile ----------

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _api.GetAsync<ProfileInfo>("api/users/profile");
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "We could not load your profile.";
                return RedirectToAction("Index", "Home");
            }

            return View(new ProfileViewModel
            {
                FullName = result.Data.FullName,
                Email = result.Data.Email,
                Role = result.Data.Role,
                ProfilePictureUrl = result.Data.ProfilePictureUrl,
                DateRegistered = result.Data.DateRegistered
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PutAsync<ProfileInfo>("api/users/profile", new
            {
                fullName = model.FullName,
                email = model.Email
            });
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
                return View(model);
            }

            HttpContext.Session.SetString(SessionKeys.FullName, model.FullName.Trim());
            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilePicture(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please choose an image to upload.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _api.PostFileAsync<PictureResponse>("api/users/profile/picture", file);
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) TempData["Success"] = "Your profile picture has been updated.";
            else TempData["Error"] = result.Message ?? "We could not upload your picture.";

            return RedirectToAction(nameof(Profile));
        }
    }
}
