namespace EduPortal.Application.DTOs.Class;

public class ClassSummaryDto
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? ClassTeacherName { get; set; }
    public int CurrentStudentCount { get; set; }
    public int Capacity { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
