using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers
{
    public abstract class AppController : Controller
    {
        /// <summary>Used when the API says the login has expired (for example after the API restarted).</summary>
        protected IActionResult SessionExpired()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }
    }
}
