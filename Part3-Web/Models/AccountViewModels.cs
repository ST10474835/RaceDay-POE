using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Your email can be at most 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a password.")]
        [StringLength(72, MinimumLength = 8, ErrorMessage = "Your password must be between 8 and 72 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a role.")]
        [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Please choose Organiser or Participant.")]
        [Display(Name = "I am a")]
        public string Role { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}