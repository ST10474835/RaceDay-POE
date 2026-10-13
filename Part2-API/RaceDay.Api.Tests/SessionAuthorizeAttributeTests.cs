using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.Api.Filters;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class SessionAuthorizeAttributeTests
    {
        private static AuthorizationFilterContext BuildContext(int? userId, string? role)
        {
            var httpContext = new DefaultHttpContext { Session = new FakeSession() };

            if (userId.HasValue)
                httpContext.Session.SetInt32("UserID", userId.Value);
            if (role != null)
                httpContext.Session.SetString("Role", role);

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void NotLoggedIn_OnOrganiserEndpoint_Returns401()
        {
            var context = BuildContext(null, null);

            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void NotLoggedIn_OnAnyUserEndpoint_Returns401()
        {
            var context = BuildContext(null, null);

            new SessionAuthorizeAttribute().OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void Participant_OnOrganiserEndpoint_Returns403()
        {
            var context = BuildContext(5, "Participant");

            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void Organiser_OnParticipantEndpoint_Returns403()
        {
            var context = BuildContext(1, "Organiser");

            new SessionAuthorizeAttribute("Participant").OnAuthorization(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void Organiser_OnOrganiserEndpoint_IsAllowed()
        {
            var context = BuildContext(1, "Organiser");

            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void Participant_OnParticipantEndpoint_IsAllowed()
        {
            var context = BuildContext(5, "Participant");

            new SessionAuthorizeAttribute("Participant").OnAuthorization(context);

            Assert.Null(context.Result);
        }

        [Theory]
        [InlineData("Organiser")]
        [InlineData("Participant")]
        public void AnyLoggedInRole_OnAnyUserEndpoint_IsAllowed(string role)
        {
            var context = BuildContext(1, role);

            new SessionAuthorizeAttribute().OnAuthorization(context);

            Assert.Null(context.Result);
        }
    }
}
