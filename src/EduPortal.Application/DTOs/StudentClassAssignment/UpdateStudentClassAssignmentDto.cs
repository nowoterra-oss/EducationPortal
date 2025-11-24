using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentClassAssignment;

public class UpdateStudentClassAssignmentDto
{
    [Required(ErrorMessage = "Sınıf belirtilmelidir")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Akademik dönem belirtilmelidir")]
    public int AcademicTermId { get; set; }

    [Required(ErrorMessage = "Atama tarihi belirtilmelidir")]
    public DateTime AssignmentDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}
