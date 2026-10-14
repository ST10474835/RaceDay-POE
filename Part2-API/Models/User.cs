using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleID { get; set; }
        public Role? Role { get; set; }

        public DateTime DateRegistered { get; set; }

        // Events this user created (when the user is an Organiser)
        public ICollection<Event> Events { get; set; } = new List<Event>();

        // Enrolments this user made (when the user is a Participant)
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
