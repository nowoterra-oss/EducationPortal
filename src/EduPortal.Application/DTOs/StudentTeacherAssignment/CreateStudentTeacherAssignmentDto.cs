using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.StudentTeacherAssignment;

public class CreateStudentTeacherAssignmentDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Atama türü: Advisor (Danışman) veya Coach (Koç)
    /// </summary>
    public AssignmentType AssignmentType { get; set; } = AssignmentType.Advisor;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
