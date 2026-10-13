using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{
    public static class TestHelpers
    {
        
        public static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

       
        public static User AddUser(AppDbContext context, string fullName, string email,
            string roleName, string password = "Password123")
        {
            var role = context.Roles.First(r => r.RoleName == roleName);
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 4),
                RoleID = role.RoleID
            };

            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

       
        public static T WithSession<T>(this T controller, int? userId, string? role) where T : ControllerBase
        {
            var httpContext = new DefaultHttpContext { Session = new FakeSession() };

            if (userId.HasValue)
                httpContext.Session.SetInt32("UserID", userId.Value);
            if (role != null)
                httpContext.Session.SetString("Role", role);

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        
        public static int? StatusCodeOf(this IActionResult result)
            => (result as ObjectResult)?.StatusCode;

     
        public static object? ValueOf(this IActionResult result)
            => (result as ObjectResult)?.Value;

        
        public static object? Prop(this object? value, string name)
            => value?.GetType().GetProperty(name)?.GetValue(value);
    }
}
