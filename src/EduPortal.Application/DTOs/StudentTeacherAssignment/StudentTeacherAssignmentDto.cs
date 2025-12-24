using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.StudentTeacherAssignment;

public class StudentTeacherAssignmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentNo { get; set; }
    public int TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public AssignmentType AssignmentType { get; set; }
    public string AssignmentTypeName => AssignmentType == AssignmentType.Advisor ? "Danışman" : "Koç";
    public string? Notes { get; set; }
    public int DurationDays => EndDate.HasValue
        ? (EndDate.Value - StartDate).Days
        : (DateTime.Now - StartDate).Days;
}
