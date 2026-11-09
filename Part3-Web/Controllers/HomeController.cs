using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : AppController
    {
        private readonly IApiClient _api;

        public HomeController(IApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? search, string? type, DateOnly? fromDate, DateOnly? toDate)
        {
            var role = HttpContext.Session.GetString(SessionKeys.Role);

            // Organisers land on their dashboard, visitors see the landing page
            if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
            if (role != "Participant") return View();

            var result = await _api.GetAsync<List<EventItem>>("api/events");
            if (result.IsUnauthorized) return SessionExpired();

            if (!result.Success)
                TempData["Error"] = result.Message ?? "We could not load the events.";

            var today = DateOnly.FromDateTime(DateTime.Today);
            var events = (result.Data ?? new List<EventItem>()).Where(e => e.EventDate >= today);

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e =>
                    e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(type))
                events = events.Where(e => e.EventType == type);

            if (fromDate.HasValue)
                events = events.Where(e => e.EventDate >= fromDate.Value);

            if (toDate.HasValue)
                events = events.Where(e => e.EventDate <= toDate.Value);

            return View("Events", new EventsListViewModel
            {
                Events = events.OrderBy(e => e.EventDate).ToList(),
                Search = search,
                Type = type,
                FromDate = fromDate,
                ToDate = toDate
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewBag.Message = "Something went wrong on our side. Please try again.";
            return View();
        }

        public IActionResult Status(int code)
        {
            ViewBag.Message = code == 404
                ? "We could not find the page you were looking for."
                : "Something went wrong. Please try again.";
            return View("Error");
        }
    }
}
