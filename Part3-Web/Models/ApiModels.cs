namespace RaceDay.Web.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ProfileInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class EventItem
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string? OrganiserName { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string? BannerUrl { get; set; }
    }

    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
    }

    public class MyEnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EventEnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ResultItem
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TimeOnly? FinishTime { get; set; }
        public int? Position { get; set; }
        public DateTime DateCaptured { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class BannerResponse
    {
        public int EventId { get; set; }
        public string? BannerUrl { get; set; }
    }

    public class PictureResponse
    {
        public string? ProfilePictureUrl { get; set; }
    }
}