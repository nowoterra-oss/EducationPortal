using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentTeacherAssignment;

public class UpdateStudentTeacherAssignmentDto
{
    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
