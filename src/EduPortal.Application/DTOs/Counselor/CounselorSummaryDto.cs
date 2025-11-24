namespace EduPortal.Application.DTOs.Counselor;

public class CounselorSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public bool IsActive { get; set; }
    public int ActiveStudentCount { get; set; }
}
