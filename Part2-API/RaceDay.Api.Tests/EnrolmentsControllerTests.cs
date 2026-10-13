using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentsControllerTests
    {
        [Fact]
        public async Task Enrol_AsParticipant_Returns201_AndRecordsTheParticipantEventAndCategoryLink()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID, "10km");
            var controller = new EnrolmentsController(context).WithSession(participant.UserID, "Participant");

            var result = await controller.Enrol(new EnrolmentDto { CategoryId = category.CategoryID });

            Assert.Equal(201, result.StatusCodeOf());

            var enrolment = Assert.Single(context.Enrolments);
            Assert.Equal(participant.UserID, enrolment.ParticipantID);
            Assert.Equal(category.CategoryID, enrolment.CategoryID);

            // The response links the participant's choice back to the category and its event
            Assert.Equal(category.CategoryID, (int)result.ValueOf().Prop("categoryId")!);
            Assert.Equal(ev.EventID, (int)result.ValueOf().Prop("eventId")!);
        }

        [Fact]
        public async Task Enrol_InTheSameCategoryTwice_Returns409_AndStoresOnlyOneEnrolment()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var controller = new EnrolmentsController(context).WithSession(participant.UserID, "Participant");
            var dto = new EnrolmentDto { CategoryId = category.CategoryID };

            var first = await controller.Enrol(dto);
            var second = await controller.Enrol(dto);

            Assert.Equal(201, first.StatusCodeOf());
            Assert.Equal(409, second.StatusCodeOf());
            Assert.Single(context.Enrolments);
        }

        [Fact]
        public async Task Enrol_InAnAgeAndADistanceCategoryOfTheSameEvent_BothSucceed()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var distance = TestData.AddCategory(context, ev.EventID, "10km", "Distance");
            var age = TestData.AddCategory(context, ev.EventID, "Under 20", "Age");
            var controller = new EnrolmentsController(context).WithSession(participant.UserID, "Participant");

            var first = await controller.Enrol(new EnrolmentDto { CategoryId = distance.CategoryID });
            var second = await controller.Enrol(new EnrolmentDto { CategoryId = age.CategoryID });

            Assert.Equal(201, first.StatusCodeOf());
            Assert.Equal(201, second.StatusCodeOf());
            Assert.Equal(2, context.Enrolments.Count());
        }

        [Fact]
        public async Task Enrol_InACategoryThatDoesNotExist_Returns400_AndStoresNothing()
        {
            using var context = TestHelpers.CreateContext();
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var controller = new EnrolmentsController(context).WithSession(participant.UserID, "Participant");

            var result = await controller.Enrol(new EnrolmentDto { CategoryId = 9999 });

            Assert.Equal(400, result.StatusCodeOf());
            Assert.Empty(context.Enrolments);
        }

        [Fact]
        public async Task GetMyEnrolments_ReturnsOnlyTheLoggedInParticipantsEnrolments()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var runnerOne = TestHelpers.AddUser(context, "Runner One", "one@raceday.co.za", "Participant");
            var runnerTwo = TestHelpers.AddUser(context, "Runner Two", "two@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var tenK = TestData.AddCategory(context, ev.EventID, "10km");
            var half = TestData.AddCategory(context, ev.EventID, "Half Marathon (21km)");
            TestData.AddEnrolment(context, runnerOne.UserID, tenK.CategoryID);
            TestData.AddEnrolment(context, runnerTwo.UserID, half.CategoryID);
            var controller = new EnrolmentsController(context).WithSession(runnerOne.UserID, "Participant");

            var result = await controller.GetMyEnrolments();

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf()).ToList();
            var only = Assert.Single(list);
            Assert.Equal("10km", (string?)only.Prop("categoryName"));
        }

        [Fact]
        public async Task GetEventEnrolments_ByEventOwner_Returns200_WithAllEnrolments()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var runnerOne = TestHelpers.AddUser(context, "Runner One", "one@raceday.co.za", "Participant");
            var runnerTwo = TestHelpers.AddUser(context, "Runner Two", "two@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var tenK = TestData.AddCategory(context, ev.EventID, "10km");
            var half = TestData.AddCategory(context, ev.EventID, "Half Marathon (21km)");
            TestData.AddEnrolment(context, runnerOne.UserID, tenK.CategoryID);
            TestData.AddEnrolment(context, runnerTwo.UserID, half.CategoryID);
            var controller = new EnrolmentsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.GetEventEnrolments(ev.EventID);

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf()).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetEventEnrolments_ByAnotherOrganiser_Returns403()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var other = TestHelpers.AddUser(context, "Org Two", "org2@raceday.co.za", "Organiser");
            var ev = TestData.AddEvent(context, owner.UserID);
            var controller = new EnrolmentsController(context).WithSession(other.UserID, "Organiser");

            var result = await controller.GetEventEnrolments(ev.EventID);

            Assert.Equal(403, result.StatusCodeOf());
        }

        [Fact]
        public async Task GetEventEnrolments_ForEventThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new EnrolmentsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.GetEventEnrolments(9999);

            Assert.Equal(404, result.StatusCodeOf());
        }
    }
}
