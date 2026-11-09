using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : AppController
    {
        private readonly IApiClient _api;

        public AccountController(IApiClient api)
        {
            _api = api;
        }

        private bool IsLoggedIn => HttpContext.Session.GetInt32(SessionKeys.UserId) != null;

        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PostAsync<object>("api/auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "We could not create your account.");
                return View(model);
            }

            TempData["Success"] = "Your account has been created. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            HttpContext.Session.Remove(SessionKeys.ApiCookie);

            var result = await _api.PostAsync<UserInfo>("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!result.Success || result.Data == null)
            {
                var message = result.StatusCode == 401
                    ? "Incorrect email or password."
                    : result.Message ?? "We could not log you in.";
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            HttpContext.Session.SetInt32(SessionKeys.UserId, result.Data.UserId);
            HttpContext.Session.SetString(SessionKeys.Role, result.Data.Role);
            HttpContext.Session.SetString(SessionKeys.FullName, result.Data.FullName);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.PostAsync<object>("api/auth/logout");
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
