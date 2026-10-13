using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthControllerTests
    {
        // ---------- Register ----------

        [Fact]
        public async Task Register_WithValidDetails_Returns201_AndStoresHashedPassword()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context);

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Test Organiser",
                Email = "ORG@raceday.co.za",
                Password = "Password123",
                Role = "Organiser"
            });

            Assert.Equal(201, result.StatusCodeOf());

            var user = Assert.Single(context.Users);
            Assert.Equal("org@raceday.co.za", user.Email);
            Assert.NotEqual("Password123", user.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123", user.PasswordHash));
            Assert.Equal("Organiser", context.Roles.Single(r => r.RoleID == user.RoleID).RoleName);
        }

        [Fact]
        public async Task Register_AsParticipant_AssignsParticipantRole()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context);

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Test Participant",
                Email = "part@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            });

            Assert.Equal(201, result.StatusCodeOf());
            Assert.Equal("Participant", result.ValueOf().Prop("role"));
        }

        [Fact]
        public async Task Register_WithExistingEmail_Returns409_AndCreatesNoSecondUser()
        {
            using var context = TestHelpers.CreateContext();
            TestHelpers.AddUser(context, "Existing", "taken@raceday.co.za", "Participant");
            var controller = new AuthController(context);

            var result = await controller.Register(new RegisterDto
            {
                FullName = "New Person",
                Email = "TAKEN@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            });

            Assert.Equal(409, result.StatusCodeOf());
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_WithInvalidRole_Returns400_AndCreatesNoUser()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context);

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Sneaky",
                Email = "sneaky@raceday.co.za",
                Password = "Password123",
                Role = "Admin"
            });

            Assert.Equal(400, result.StatusCodeOf());
            Assert.Empty(context.Users);
        }

        [Fact]
        public void RegisterDto_WithShortPassword_FailsValidation()
        {
            var dto = new RegisterDto
            {
                FullName = "Test",
                Email = "test@raceday.co.za",
                Password = "short",
                Role = "Organiser"
            };

            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);

            Assert.False(isValid);
            Assert.Contains(errors, e => e.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterDto_WithInvalidEmail_FailsValidation()
        {
            var dto = new RegisterDto
            {
                FullName = "Test",
                Email = "not-an-email",
                Password = "Password123",
                Role = "Organiser"
            };

            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);

            Assert.False(isValid);
            Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
        }

        // ---------- Login ----------

        [Fact]
        public async Task Login_WithCorrectCredentials_Returns200_AndStoresUserIdAndRoleInSession()
        {
            using var context = TestHelpers.CreateContext();
            var user = TestHelpers.AddUser(context, "Sam Runner", "sam@raceday.co.za", "Participant");
            var controller = new AuthController(context).WithSession(null, null);

            var result = await controller.Login(new LoginDto
            {
                Email = "SAM@raceday.co.za",
                Password = "Password123"
            });

            Assert.Equal(200, result.StatusCodeOf());
            Assert.Equal(user.UserID, controller.HttpContext.Session.GetInt32("UserID"));
            Assert.Equal("Participant", controller.HttpContext.Session.GetString("Role"));
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401_AndCreatesNoSession()
        {
            using var context = TestHelpers.CreateContext();
            TestHelpers.AddUser(context, "Sam Runner", "sam@raceday.co.za", "Participant");
            var controller = new AuthController(context).WithSession(null, null);

            var result = await controller.Login(new LoginDto
            {
                Email = "sam@raceday.co.za",
                Password = "WrongPassword"
            });

            Assert.Equal(401, result.StatusCodeOf());
            Assert.Null(controller.HttpContext.Session.GetInt32("UserID"));
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession(null, null);

            var result = await controller.Login(new LoginDto
            {
                Email = "nobody@raceday.co.za",
                Password = "Password123"
            });

            Assert.Equal(401, result.StatusCodeOf());
        }

        // ---------- Logout ----------

        [Fact]
        public void Logout_WhenLoggedIn_Returns200_AndClearsSession()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession(1, "Organiser");

            var result = controller.Logout();

            Assert.Equal(200, result.StatusCodeOf());
            Assert.Null(controller.HttpContext.Session.GetInt32("UserID"));
            Assert.Null(controller.HttpContext.Session.GetString("Role"));
        }

        [Fact]
        public void Logout_WhenNotLoggedIn_Returns401()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession(null, null);

            var result = controller.Logout();

            Assert.Equal(401, result.StatusCodeOf());
        }
    }
}
