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

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32(SessionKeys.UserId) == null)
                return View();

            // Reads the logged-in user's profile from the API (proves the API session is being forwarded)
            var profile = await _api.GetAsync<ProfileInfo>("api/users/profile");
            if (profile.IsUnauthorized) return SessionExpired();

            return View(profile.Data);
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