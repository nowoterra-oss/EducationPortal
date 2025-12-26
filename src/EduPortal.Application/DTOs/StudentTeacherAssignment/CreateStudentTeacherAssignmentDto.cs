using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.StudentTeacherAssignment;

public class CreateStudentTeacherAssignmentDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    /// <summary>
    /// Kurs ID - Danışman/Rehber atamaları için opsiyonel
    /// </summary>
    public int? CourseId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Atama türü: Advisor (Danışman) veya Counselor (Rehber)
    /// </summary>
    public AssignmentType AssignmentType { get; set; } = AssignmentType.Advisor;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
