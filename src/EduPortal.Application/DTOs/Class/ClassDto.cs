namespace EduPortal.Application.DTOs.Class;

public class ClassDto
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string Branch { get; set; } = string.Empty;

    public int? ClassTeacherId { get; set; }
    public string? ClassTeacherName { get; set; }

    public int Capacity { get; set; }
    public int CurrentStudentCount { get; set; }
    public int AvailableCapacity => Capacity - CurrentStudentCount;

    public string AcademicYear { get; set; } = string.Empty;

    public int? BranchId { get; set; }
    public string? BranchLocationName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
