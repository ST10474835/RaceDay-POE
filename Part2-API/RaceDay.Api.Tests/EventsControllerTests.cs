using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class CategoriesControllerTests
    {
        [Fact]
        public async Task CreateCategory_ByEventOwner_Returns201_AndStoresCategory()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var controller = new CategoriesController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.CreateCategory(ev.EventID,
                new CategoryDto { CategoryName = "Under 20", CategoryType = "Age" });

            Assert.Equal(201, result.StatusCodeOf());
            var saved = Assert.Single(context.Categories);
            Assert.Equal("Under 20", saved.CategoryName);
            Assert.Equal("Age", saved.CategoryType);
            Assert.Equal(ev.EventID, saved.EventID);
        }

        [Fact]
        public async Task CreateCategory_ByAnotherOrganiser_Returns403_AndStoresNothing()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var other = TestHelpers.AddUser(context, "Org Two", "org2@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, owner.UserID);
            var controller = new CategoriesController(context).WithSession(other.UserID, "Organiser");

            var result = await controller.CreateCategory(ev.EventID,
                new CategoryDto { CategoryName = "10km", CategoryType = "Distance" });

            Assert.Equal(403, result.StatusCodeOf());
            Assert.Empty(context.Categories);
        }

        [Fact]
        public async Task CreateCategory_ForEventThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new CategoriesController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.CreateCategory(9999,
                new CategoryDto { CategoryName = "10km", CategoryType = "Distance" });

            Assert.Equal(404, result.StatusCodeOf());
        }

        [Fact]
        public async Task GetCategories_ReturnsOnlyTheCategoriesOfThatEvent()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var eventA = TestData.AddEvent(context, organiser.UserID, "Event A");
            var eventB = TestData.AddEvent(context, organiser.UserID, "Event B");
            TestData.AddCategory(context, eventA.EventID, "10km");
            TestData.AddCategory(context, eventA.EventID, "Under 20", "Age");
            TestData.AddCategory(context, eventB.EventID, "5km");
            var controller = new CategoriesController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.GetCategories(eventA.EventID);

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf()).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetCategories_ForEventThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new CategoriesController(context).WithSession(1, "Participant");

            var result = await controller.GetCategories(9999);

            Assert.Equal(404, result.StatusCodeOf());
        }

        [Fact]
        public void CategoryDto_WithInvalidType_FailsValidation()
        {
            var dto = new CategoryDto { CategoryName = "Heavyweight", CategoryType = "Weight" };
            var errors = new List<ValidationResult>();

            Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);

            Assert.Contains(errors, e => e.MemberNames.Contains("CategoryType"));
        }
    }
}
