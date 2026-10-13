using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RaceDay.Api.Controllers;
using RaceDay.Api.Filters;
using Xunit;

namespace RaceDay.Api.Tests
{
    /// <summary>
    /// Checks that every endpoint carries the role restriction promised in the Part 1 endpoint plan.
    /// </summary>
    public class RoleProtectionTests
    {
        public static IEnumerable<object[]> OrganiserOnlyEndpoints => new[]
        {
            new object[] { typeof(EventsController), nameof(EventsController.CreateEvent) },
            new object[] { typeof(EventsController), nameof(EventsController.UpdateEvent) },
            new object[] { typeof(EventsController), nameof(EventsController.DeleteEvent) },
            new object[] { typeof(CategoriesController), nameof(CategoriesController.CreateCategory) },
            new object[] { typeof(EnrolmentsController), nameof(EnrolmentsController.GetEventEnrolments) },
            new object[] { typeof(ResultsController), nameof(ResultsController.CaptureResult) }
        };

        public static IEnumerable<object[]> ParticipantOnlyEndpoints => new[]
        {
            new object[] { typeof(EnrolmentsController), nameof(EnrolmentsController.Enrol) },
            new object[] { typeof(EnrolmentsController), nameof(EnrolmentsController.GetMyEnrolments) },
            new object[] { typeof(ResultsController), nameof(ResultsController.GetMyResults) }
        };

        public static IEnumerable<object[]> ProtectedControllers => new[]
        {
            new object[] { typeof(UsersController) },
            new object[] { typeof(EventsController) },
            new object[] { typeof(CategoriesController) },
            new object[] { typeof(EnrolmentsController) },
            new object[] { typeof(ResultsController) }
        };

        private static bool MethodRequiresRole(Type controller, string methodName, string role)
        {
            var method = controller.GetMethod(methodName);
            Assert.NotNull(method);

            return method!.GetCustomAttributes<SessionAuthorizeAttribute>(true)
                .Any(a => a.Roles.Contains(role));
        }

        [Theory]
        [MemberData(nameof(OrganiserOnlyEndpoints))]
        public void OrganiserEndpoints_RequireOrganiserRole(Type controller, string methodName)
        {
            Assert.True(MethodRequiresRole(controller, methodName, "Organiser"),
                $"{controller.Name}.{methodName} should be restricted to Organisers.");
        }

        [Theory]
        [MemberData(nameof(ParticipantOnlyEndpoints))]
        public void ParticipantEndpoints_RequireParticipantRole(Type controller, string methodName)
        {
            Assert.True(MethodRequiresRole(controller, methodName, "Participant"),
                $"{controller.Name}.{methodName} should be restricted to Participants.");
        }

        [Theory]
        [MemberData(nameof(ProtectedControllers))]
        public void NonPublicControllers_RequireLogin(Type controller)
        {
            var attributes = controller.GetCustomAttributes<SessionAuthorizeAttribute>(false);

            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void AuthController_IsPublic_SoUsersCanRegisterAndLogIn()
        {
            var attributes = typeof(AuthController).GetCustomAttributes<SessionAuthorizeAttribute>(false);

            Assert.Empty(attributes);
        }
    }
}
