using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Filters
{
    /// <summary>
    /// Requires a logged-in session. If roles are supplied, the user's role must match one of them.
    /// Returns 401 when not logged in and 403 when the role is not allowed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;
        public string[] Roles => _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            if (session.GetUserId() == null)
            {
                context.Result = new UnauthorizedObjectResult(
                    new { message = "You must be logged in to access this resource." });
                return;
            }

            if (_roles.Length > 0)
            {
                var role = session.GetRole();
                if (role == null || !_roles.Contains(role))
                {
                    context.Result = new ObjectResult(
                        new { message = "You do not have permission to access this resource." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
    }
}
