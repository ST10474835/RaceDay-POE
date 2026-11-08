using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class EventFormViewModel
    {
        public int EventId { get; set; }
        public bool IsEdit { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(150, ErrorMessage = "The event name can be at most 150 characters.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "The description can be at most 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [Display(Name = "Date")]
        public DateOnly? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(150, ErrorMessage = "The location can be at most 150 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the distance in kilometres.")]
        [Range(0.01, 999.99, ErrorMessage = "The distance must be between 0.01 and 999.99 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Please choose the event type.")]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Please choose Run, Walk or Cycle.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "Banner image")]
        public IFormFile? BannerFile { get; set; }

        public string? ExistingBannerUrl { get; set; }
    }

    public class DashboardEventRow
    {
        public EventItem Event { get; set; } = new();
        public int EnrolmentCount { get; set; }
        public int Confirmed { get; set; }
        public int Pending { get; set; }
    }

    public class DashboardViewModel
    {
        public List<DashboardEventRow> Rows { get; set; } = new();
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int PendingEnrolments { get; set; }
        public int UpcomingCount { get; set; }
        public DashboardEventRow? NextEvent { get; set; }
    }

    public class CategoriesPageModel
    {
        public EventItem Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
    }

    public class EnrolmentsPageModel
    {
        public EventItem Event { get; set; } = new();
        public List<EventEnrolmentItem> Enrolments { get; set; } = new();
    }

    public class EventResultItem
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeOnly? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class ResultRow
    {
        public EventEnrolmentItem Enrolment { get; set; } = new();
        public EventResultItem? Result { get; set; }
    }

    public class ResultsPageModel
    {
        public EventItem Event { get; set; } = new();
        public List<ResultRow> Rows { get; set; } = new();
    }
}