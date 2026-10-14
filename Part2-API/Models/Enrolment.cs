namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }
        public User? Participant { get; set; }

        public int CategoryID { get; set; }
        public Category? Category { get; set; }

        public DateTime EnrolmentDate { get; set; }

        // One enrolment has at most one result (1:1)
        public Result? Result { get; set; }
    }
}
