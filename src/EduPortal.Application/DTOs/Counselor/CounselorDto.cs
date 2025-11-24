namespace EduPortal.Application.DTOs.Counselor;

public class CounselorDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Specialization { get; set; }
    public bool IsActive { get; set; }

    // Statistics
    public int ActiveStudentCount { get; set; }
    public int TotalMeetingsCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
