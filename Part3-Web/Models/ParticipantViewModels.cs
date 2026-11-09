using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class EventsListViewModel
    {
        public List<EventItem> Events { get; set; } = new();
        public string? Search { get; set; }
        public string? Type { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

    public class EventDetailsViewModel
    {
        public EventItem Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();

        // CategoryId -> enrolment status, for categories this participant has already entered
        public Dictionary<int, string> EnrolledStatus { get; set; } = new();

        public bool IsPast { get; set; }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Your email can be at most 150 characters.")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}