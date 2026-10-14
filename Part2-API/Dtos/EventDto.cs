
       using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class EventDto
    {
        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateOnly? EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(0.01, 999.99)]
        public decimal? Distance { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "EventType must be 'Run', 'Walk' or 'Cycle'.")]
        public string EventType { get; set; } = string.Empty;
    }
}
