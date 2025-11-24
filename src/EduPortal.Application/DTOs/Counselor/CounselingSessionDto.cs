namespace EduPortal.Application.DTOs.Counselor;

public class CounselingSessionDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CounselorId { get; set; }
    public string CounselorName { get; set; } = string.Empty;

    public DateTime MeetingDate { get; set; }
    public int? Duration { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Assignments { get; set; }
    public DateTime? NextMeetingDate { get; set; }

    public bool SendEmailToParent { get; set; }
    public bool SendSMSToParent { get; set; }

    public DateTime CreatedAt { get; set; }
}
