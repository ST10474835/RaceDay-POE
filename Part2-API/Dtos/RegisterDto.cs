using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(72)]
        public string Password { get; set; } = string.Empty;

        // "Organiser" or "Participant"
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
