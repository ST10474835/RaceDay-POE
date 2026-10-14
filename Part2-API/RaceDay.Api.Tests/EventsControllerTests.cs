using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsControllerTests
    {
        private static EventDto ValidDto(string eventType = "Run", string location = "Johannesburg, Gauteng") => new()
        {
            EventName = "Johannesburg City Marathon",
            Description = "Annual road marathon.",
            EventDate = new DateOnly(2026, 11, 15),
            Location = location,
            Distance = 42.2m,
            EventType = eventType
        };

        // ---------- Create ----------

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201_AndLinksEventToLoggedInOrganiser()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.CreateEvent(ValidDto());

            Assert.Equal(201, result.StatusCodeOf());
            var saved = Assert.Single(context.Events);
            Assert.Equal(organiser.UserID, saved.OrganiserID);
            Assert.Equal("Run", saved.EventType);
            Assert.Equal("Johannesburg City Marathon", saved.EventName);
        }

        // ---------- View ----------

        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            TestData.AddEvent(context, organiser.UserID, "Event A");
            TestData.AddEvent(context, organiser.UserID, "Event B");
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.GetEvents();

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf()).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetEvent_ThatExists_Returns200()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.GetEvent(ev.EventID);

            Assert.Equal(200, result.StatusCodeOf());
            Assert.Equal("Test Marathon", (string?)result.ValueOf().Prop("eventName"));
        }

        [Fact]
        public async Task GetEvent_ThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new EventsController(context).WithSession(1, "Organiser");

            var result = await controller.GetEvent(9999);

            Assert.Equal(404, result.StatusCodeOf());
        }

        // ---------- Update ----------

        [Fact]
        public async Task UpdateEvent_ByOwner_Returns200_AndSavesChanges()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.UpdateEvent(ev.EventID, ValidDto("Run", "Sandton, Gauteng"));

            Assert.Equal(200, result.StatusCodeOf());
            Assert.Equal("Sandton, Gauteng", context.Events.Single().Location);
        }

        [Fact]
        public async Task UpdateEvent_ByAnotherOrganiser_Returns403_AndChangesNothing()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var other = TestHelpers.AddUser(context, "Org Two", "org2@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, owner.UserID);
            var controller = new EventsController(context).WithSession(other.UserID, "Organiser");

            var result = await controller.UpdateEvent(ev.EventID, ValidDto("Run", "Sandton, Gauteng"));

            Assert.Equal(403, result.StatusCodeOf());
            Assert.Equal("Johannesburg, Gauteng", context.Events.Single().Location);
        }

        [Fact]
        public async Task UpdateEvent_ThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.UpdateEvent(9999, ValidDto());

            Assert.Equal(404, result.StatusCodeOf());
        }

        // ---------- Delete ----------

        [Fact]
        public async Task DeleteEvent_ByOwner_Returns200_AndRemovesCategoriesEnrolmentsAndResults()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var enrolment = TestData.AddEnrolment(context, participant.UserID, category.CategoryID);
            TestData.AddResult(context, enrolment.EnrolmentID, new TimeOnly(0, 48, 32), 5);
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.DeleteEvent(ev.EventID);

            Assert.Equal(200, result.StatusCodeOf());
            Assert.Empty(context.Events);
            Assert.Empty(context.Categories);
            Assert.Empty(context.Enrolments);
            Assert.Empty(context.Results);
            Assert.Equal(2, context.Users.Count());   // people are never deleted
        }

        [Fact]
        public async Task DeleteEvent_ByAnotherOrganiser_Returns403_AndKeepsTheEvent()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var other = TestHelpers.AddUser(context, "Org Two", "org2@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, owner.UserID);
            var controller = new EventsController(context).WithSession(other.UserID, "Organiser");

            var result = await controller.DeleteEvent(ev.EventID);

            Assert.Equal(403, result.StatusCodeOf());
            Assert.Single(context.Events);
        }

        [Fact]
        public async Task DeleteEvent_ThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new EventsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.DeleteEvent(9999);

            Assert.Equal(404, result.StatusCodeOf());
        }

        // ---------- Validation rules ----------

        private static List<ValidationResult> Validate(EventDto dto)
        {
            var errors = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);
            return errors;
        }

        [Fact]
        public void EventDto_WithValidDetails_PassesValidation()
        {
            Assert.Empty(Validate(ValidDto()));
        }

        [Fact]
        public void EventDto_WithInvalidEventType_FailsValidation()
        {
            var errors = Validate(ValidDto("Swim"));

            Assert.Contains(errors, e => e.MemberNames.Contains("EventType"));
        }

        [Fact]
        public void EventDto_WithZeroDistance_FailsValidation()
        {
            var dto = ValidDto();
            dto.Distance = 0m;

            var errors = Validate(dto);

            Assert.Contains(errors, e => e.MemberNames.Contains("Distance"));
        }
    }
}
