namespace EduPortal.Application.DTOs.Counselor;

public class CounselorStudentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public bool IsActive { get; set; }
    public int MeetingCount { get; set; }
    public DateTime? LastMeetingDate { get; set; }
}
