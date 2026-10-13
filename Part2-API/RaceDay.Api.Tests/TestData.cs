using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{
    /// <summary>
    /// Helpers that insert sample events, categories, enrolments and results into a test database.
    /// </summary>
    public static class TestData
    {
        public static Event AddEvent(AppDbContext context, int organiserId, string name = "Test Marathon")
        {
            var ev = new Event
            {
                OrganiserID = organiserId,
                EventName = name,
                Description = "Test event",
                EventDate = new DateOnly(2026, 11, 15),
                Location = "Johannesburg, Gauteng",
                Distance = 42.2m,
                EventType = "Run"
            };

            context.Events.Add(ev);
            context.SaveChanges();
            return ev;
        }

        public static Category AddCategory(AppDbContext context, int eventId,
            string name = "10km", string type = "Distance")
        {
            var category = new Category
            {
                EventID = eventId,
                CategoryName = name,
                CategoryType = type
            };

            context.Categories.Add(category);
            context.SaveChanges();
            return category;
        }

        public static Enrolment AddEnrolment(AppDbContext context, int participantId, int categoryId)
        {
            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = categoryId
            };

            context.Enrolments.Add(enrolment);
            context.SaveChanges();
            return enrolment;
        }

        public static Result AddResult(AppDbContext context, int enrolmentId, TimeOnly finishTime, int position)
        {
            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = finishTime,
                Position = position
            };

            context.Results.Add(result);
            context.SaveChanges();
            return result;
        }
    }
}
