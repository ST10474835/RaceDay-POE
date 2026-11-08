using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserController : AppController
    {
        private readonly IApiClient _api;

        public OrganiserController(IApiClient api)
        {
            _api = api;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

        // ---------- helpers ----------

        private async Task<(List<EventItem>? Items, IActionResult? Failure)> LoadMyEventsAsync()
        {
            var result = await _api.GetAsync<List<EventItem>>("api/events");
            if (result.IsUnauthorized) return (null, SessionExpired());

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "We could not load your events.";
                return (null, RedirectToAction("Index", "Home"));
            }

            var mine = result.Data
                .Where(e => e.OrganiserId == CurrentUserId)
                .OrderBy(e => e.EventDate)
                .ToList();

            return (mine, null);
        }

        private async Task<(EventItem? Event, IActionResult? Failure)> LoadOwnedEventAsync(int id)
        {
            var result = await _api.GetAsync<EventItem>($"api/events/{id}");
            if (result.IsUnauthorized) return (null, SessionExpired());

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.StatusCode == 404
                    ? "That event could not be found."
                    : result.Message ?? "We could not load that event.";
                return (null, RedirectToAction(nameof(Events)));
            }

            if (result.Data.OrganiserId != CurrentUserId)
            {
                TempData["Error"] = "You can only manage events that you created.";
                return (null, RedirectToAction(nameof(Events)));
            }

            return (result.Data, null);
        }

        private static object ToApiBody(EventFormViewModel m) => new
        {
            eventName = m.EventName,
            description = string.IsNullOrWhiteSpace(m.Description) ? null : m.Description,
            eventDate = m.EventDate!.Value.ToString("yyyy-MM-dd"),
            location = m.Location,
            distance = m.Distance!.Value,
            eventType = m.EventType
        };

        private async Task<string?> UploadBannerAsync(int eventId, IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var upload = await _api.PostFileAsync<BannerResponse>($"api/events/{eventId}/banner", file);
            return upload.Success
                ? null
                : $"The event was saved, but the banner image was not: {upload.Message}";
        }

        // ---------- dashboard ----------

        public async Task<IActionResult> Dashboard()
        {
            var (items, failure) = await LoadMyEventsAsync();
            if (failure != null) return failure;

            var rows = new List<DashboardEventRow>();
            foreach (var ev in items!)
            {
                var enrolments = await _api.GetAsync<List<EventEnrolmentItem>>($"api/events/{ev.EventId}/enrolments");
                if (enrolments.IsUnauthorized) return SessionExpired();

                var list = enrolments.Data ?? new List<EventEnrolmentItem>();
                rows.Add(new DashboardEventRow
                {
                    Event = ev,
                    EnrolmentCount = list.Count,
                    Confirmed = list.Count(x => x.Status == "Confirmed"),
                    Pending = list.Count(x => x.Status == "Pending")
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcoming = rows.Where(r => r.Event.EventDate >= today).OrderBy(r => r.Event.EventDate).ToList();

            return View(new DashboardViewModel
            {
                Rows = rows,
                TotalEvents = rows.Count,
                TotalEnrolments = rows.Sum(r => r.EnrolmentCount),
                PendingEnrolments = rows.Sum(r => r.Pending),
                UpcomingCount = upcoming.Count,
                NextEvent = upcoming.FirstOrDefault()
            });
        }

        // ---------- events ----------

        public async Task<IActionResult> Events()
        {
            var (items, failure) = await LoadMyEventsAsync();
            if (failure != null) return failure;

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View("EventForm", new EventFormViewModel
            {
                EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View("EventForm", model);

            var result = await _api.PostAsync<EventItem>("api/events", ToApiBody(model));
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "We could not create the event.");
                return View("EventForm", model);
            }

            var warning = await UploadBannerAsync(result.Data.EventId, model.BannerFile);

            TempData["Success"] = "Event created.";
            if (warning != null) TempData["Error"] = warning;
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var (ev, failure) = await LoadOwnedEventAsync(id);
            if (failure != null) return failure;

            return View("EventForm", new EventFormViewModel
            {
                EventId = ev!.EventId,
                IsEdit = true,
                EventName = ev.EventName,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Distance = ev.Distance,
                EventType = ev.EventType,
                ExistingBannerUrl = ev.BannerUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormViewModel model)
        {
            model.EventId = id;
            model.IsEdit = true;
            if (!ModelState.IsValid) return View("EventForm", model);

            var result = await _api.PutAsync<EventItem>($"api/events/{id}", ToApiBody(model));
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
                return View("EventForm", model);
            }

            var warning = await UploadBannerAsync(id, model.BannerFile);

            TempData["Success"] = "Event updated.";
            if (warning != null) TempData["Error"] = warning;
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _api.DeleteAsync($"api/events/{id}");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) TempData["Success"] = "Event deleted.";
            else TempData["Error"] = result.Message ?? "We could not delete the event.";

            return RedirectToAction(nameof(Events));
        }

        // ---------- categories ----------

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (failure != null) return failure;

            var categories = await _api.GetAsync<List<CategoryItem>>($"api/events/{eventId}/categories");
            if (categories.IsUnauthorized) return SessionExpired();

            return View(new CategoriesPageModel
            {
                Event = ev!,
                Categories = categories.Data ?? new List<CategoryItem>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, string? categoryName, string? categoryType)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(categoryType))
            {
                TempData["Error"] = "Please enter a category name and choose its type.";
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            var result = await _api.PostAsync<CategoryItem>($"api/events/{eventId}/categories",
                new { categoryName = categoryName.Trim(), categoryType });
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) TempData["Success"] = "Category added.";
            else TempData["Error"] = result.Message ?? "We could not add the category.";

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // ---------- enrolments ----------

        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (failure != null) return failure;

            var enrolments = await _api.GetAsync<List<EventEnrolmentItem>>($"api/events/{eventId}/enrolments");
            if (enrolments.IsUnauthorized) return SessionExpired();

            return View(new EnrolmentsPageModel
            {
                Event = ev!,
                Enrolments = enrolments.Data ?? new List<EventEnrolmentItem>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEnrolment(int eventId, int enrolmentId)
        {
            var result = await _api.PutAsync<object>($"api/enrolments/{enrolmentId}/confirm");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) TempData["Success"] = "Enrolment confirmed.";
            else TempData["Error"] = result.Message ?? "We could not confirm the enrolment.";

            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        // ---------- results ----------

        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (failure != null) return failure;

            var enrolments = await _api.GetAsync<List<EventEnrolmentItem>>($"api/events/{eventId}/enrolments");
            if (enrolments.IsUnauthorized) return SessionExpired();

            var results = await _api.GetAsync<List<EventResultItem>>($"api/events/{eventId}/results");
            if (results.IsUnauthorized) return SessionExpired();

            var captured = (results.Data ?? new List<EventResultItem>())
                .ToDictionary(r => r.EnrolmentId);

            var rows = (enrolments.Data ?? new List<EventEnrolmentItem>())
                .Select(e => new ResultRow
                {
                    Enrolment = e,
                    Result = captured.TryGetValue(e.EnrolmentId, out var r) ? r : null
                })
                .OrderBy(r => r.Enrolment.CategoryName)
                .ThenBy(r => r.Result?.Position ?? int.MaxValue)
                .ToList();

            return View(new ResultsPageModel { Event = ev!, Rows = rows });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, TimeOnly? finishTime, int? position)
        {
            if (finishTime == null || position == null || position < 1)
            {
                TempData["Error"] = "Please enter a finish time and a finishing position of 1 or more.";
                return RedirectToAction(nameof(Results), new { eventId });
            }

            var result = await _api.PostAsync<object>("api/results", new
            {
                enrolmentId,
                finishTime = finishTime.Value.ToString("HH:mm:ss"),
                position = position.Value
            });
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) TempData["Success"] = "Result captured and published.";
            else TempData["Error"] = result.Message ?? "We could not save the result.";

            return RedirectToAction(nameof(Results), new { eventId });
        }
    }
}
