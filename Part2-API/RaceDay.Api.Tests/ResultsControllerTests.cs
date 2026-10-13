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
    public class ResultsControllerTests
    {
        [Fact]
        public async Task CaptureResult_ByEventOwner_Returns201_AndStoresTimeAndPosition()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var enrolment = TestData.AddEnrolment(context, participant.UserID, category.CategoryID);
            var controller = new ResultsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.CaptureResult(new ResultDto
            {
                EnrolmentId = enrolment.EnrolmentID,
                FinishTime = new TimeOnly(0, 48, 32),
                Position = 5
            });

            Assert.Equal(201, result.StatusCodeOf());
            var saved = Assert.Single(context.Results);
            Assert.Equal(enrolment.EnrolmentID, saved.EnrolmentID);
            Assert.Equal(new TimeOnly(0, 48, 32), saved.FinishTime);
            Assert.Equal(5, saved.Position);
        }

        [Fact]
        public async Task CaptureResult_ForTheSameEnrolmentTwice_Returns409_AndStoresOnlyOne()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var enrolment = TestData.AddEnrolment(context, participant.UserID, category.CategoryID);
            var controller = new ResultsController(context).WithSession(organiser.UserID, "Organiser");
            var dto = new ResultDto
            {
                EnrolmentId = enrolment.EnrolmentID,
                FinishTime = new TimeOnly(0, 48, 32),
                Position = 5
            };

            var first = await controller.CaptureResult(dto);
            var second = await controller.CaptureResult(dto);

            Assert.Equal(201, first.StatusCodeOf());
            Assert.Equal(409, second.StatusCodeOf());
            Assert.Single(context.Results);
        }

        [Fact]
        public async Task CaptureResult_ByAnotherOrganiser_Returns403_AndStoresNothing()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var other = TestHelpers.AddUser(context, "Org Two", "org2@raceday.co.za", "Organiser");
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, owner.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var enrolment = TestData.AddEnrolment(context, participant.UserID, category.CategoryID);
            var controller = new ResultsController(context).WithSession(other.UserID, "Organiser");

            var result = await controller.CaptureResult(new ResultDto
            {
                EnrolmentId = enrolment.EnrolmentID,
                FinishTime = new TimeOnly(0, 48, 32),
                Position = 5
            });

            Assert.Equal(403, result.StatusCodeOf());
            Assert.Empty(context.Results);
        }

        [Fact]
        public async Task CaptureResult_ForEnrolmentThatDoesNotExist_Returns404()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var controller = new ResultsController(context).WithSession(organiser.UserID, "Organiser");

            var result = await controller.CaptureResult(new ResultDto
            {
                EnrolmentId = 9999,
                FinishTime = new TimeOnly(0, 48, 32),
                Position = 5
            });

            Assert.Equal(404, result.StatusCodeOf());
        }

        [Fact]
        public async Task GetMyResults_ReturnsOnlyTheLoggedInParticipantsResults()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org One", "org1@raceday.co.za", "Organiser");
            var runnerOne = TestHelpers.AddUser(context, "Runner One", "one@raceday.co.za", "Participant");
            var runnerTwo = TestHelpers.AddUser(context, "Runner Two", "two@raceday.co.za", "Participant");
            var ev = TestData.AddEvent(context, organiser.UserID);
            var category = TestData.AddCategory(context, ev.EventID);
            var enrolmentOne = TestData.AddEnrolment(context, runnerOne.UserID, category.CategoryID);
            var enrolmentTwo = TestData.AddEnrolment(context, runnerTwo.UserID, category.CategoryID);
            TestData.AddResult(context, enrolmentOne.EnrolmentID, new TimeOnly(0, 48, 32), 5);
            TestData.AddResult(context, enrolmentTwo.EnrolmentID, new TimeOnly(0, 55, 10), 12);
            var controller = new ResultsController(context).WithSession(runnerOne.UserID, "Participant");

            var result = await controller.GetMyResults();

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf()).ToList();
            var only = Assert.Single(list);
            Assert.Equal(5, (int)only.Prop("position")!);
        }

        [Fact]
        public async Task GetMyResults_WhenNoResultsCaptured_ReturnsAnEmptyList()
        {
            using var context = TestHelpers.CreateContext();
            var participant = TestHelpers.AddUser(context, "Runner", "runner@raceday.co.za", "Participant");
            var controller = new ResultsController(context).WithSession(participant.UserID, "Participant");

            var result = await controller.GetMyResults();

            Assert.Equal(200, result.StatusCodeOf());
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result.ValueOf());
            Assert.Empty(list);
        }

        [Fact]
        public void ResultDto_WithPositionZero_FailsValidation()
        {
            var dto = new ResultDto { EnrolmentId = 1, FinishTime = new TimeOnly(0, 48, 32), Position = 0 };
            var errors = new List<ValidationResult>();

            Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);

            Assert.Contains(errors, e => e.MemberNames.Contains("Position"));
        }

        [Fact]
        public void ResultDto_WithoutEnrolmentId_FailsValidation()
        {
            var dto = new ResultDto { FinishTime = new TimeOnly(0, 48, 32), Position = 5 };
            var errors = new List<ValidationResult>();

            Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true);

            Assert.Contains(errors, e => e.MemberNames.Contains("EnrolmentId"));
        }
    }
}
