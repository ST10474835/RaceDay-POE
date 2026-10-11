using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class EnrolmentDto
    {
        [Required, Range(1, int.MaxValue)]
        public int? CategoryId { get; set; }
    }
}
