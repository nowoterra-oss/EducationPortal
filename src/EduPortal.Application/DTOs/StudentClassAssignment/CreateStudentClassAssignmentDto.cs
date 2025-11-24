using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentClassAssignment;

public class CreateStudentClassAssignmentDto
{
    [Required(ErrorMessage = "Öğrenci belirtilmelidir")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Sınıf belirtilmelidir")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Akademik dönem belirtilmelidir")]
    public int AcademicTermId { get; set; }

    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}
