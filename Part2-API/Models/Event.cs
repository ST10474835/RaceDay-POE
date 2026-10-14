using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }
        public User? Organiser { get; set; }

        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateOnly EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        public decimal Distance { get; set; }

        // Must be 'Run', 'Walk' or 'Cycle'
        [Required, MaxLength(10)]
        public string EventType { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
