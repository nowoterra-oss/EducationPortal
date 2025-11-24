namespace EduPortal.Application.DTOs.StudentClassAssignment;

public class StudentClassAssignmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int AcademicTermId { get; set; }
    public string AcademicTermName { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
