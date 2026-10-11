using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class CategoryDto
    {
        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Age|Distance)$",
            ErrorMessage = "CategoryType must be 'Age' or 'Distance'.")]
        public string CategoryType { get; set; } = string.Empty;
    }
}
