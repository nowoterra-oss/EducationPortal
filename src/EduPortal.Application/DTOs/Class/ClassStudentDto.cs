namespace EduPortal.Application.DTOs.Class;

public class ClassStudentDto
{
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime AssignmentDate { get; set; }
    public bool IsActive { get; set; }
}
