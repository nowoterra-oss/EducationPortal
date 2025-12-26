namespace EduPortal.Application.DTOs.CounselorDashboard;

public class CounselorNoteDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CounselorId { get; set; }
    public string CounselorName { get; set; } = string.Empty;
    public int? CounselingMeetingId { get; set; }
    public DateTime NoteDate { get; set; }
    public string? Subject { get; set; }
    public string NoteContent { get; set; } = string.Empty;
    public string? AssignedTasks { get; set; }
    public DateTime? NextMeetingDate { get; set; }
    public bool SendEmailToParent { get; set; }
    public bool SendSmsToParent { get; set; }
    public bool EmailSent { get; set; }
    public bool SmsSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public DateTime? SmsSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCounselorNoteDto
{
    public int StudentId { get; set; }
    public int? CounselingMeetingId { get; set; }
    public DateTime NoteDate { get; set; }
    public string? Subject { get; set; }
    public string NoteContent { get; set; } = string.Empty;
    public string? AssignedTasks { get; set; }
    public DateTime? NextMeetingDate { get; set; }
    public bool SendEmailToParent { get; set; }
    public bool SendSmsToParent { get; set; }
}

public class UpdateCounselorNoteDto
{
    public int Id { get; set; }
    public string? Subject { get; set; }
    public string NoteContent { get; set; } = string.Empty;
    public string? AssignedTasks { get; set; }
    public DateTime? NextMeetingDate { get; set; }
    public bool SendEmailToParent { get; set; }
    public bool SendSmsToParent { get; set; }
}
