using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public int EventID { get; set; }
        public Event? Event { get; set; }

        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        // Must be 'Age' or 'Distance'
        [Required, MaxLength(10)]
        public string CategoryType { get; set; } = string.Empty;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
