using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class ResultDto
    {
        [Required, Range(1, int.MaxValue)]
        public int? EnrolmentId { get; set; }

        // Format: HH:mm:ss, for example "00:48:32"
        [Required]
        public TimeOnly? FinishTime { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int? Position { get; set; }
    }
}
