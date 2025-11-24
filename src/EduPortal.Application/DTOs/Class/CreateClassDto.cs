using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Class;

public class CreateClassDto
{
    [Required]
    [MaxLength(50)]
    public string ClassName { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int Grade { get; set; }

    [Required]
    [MaxLength(10)]
    public string Branch { get; set; } = string.Empty;

    public int? ClassTeacherId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    public bool IsActive { get; set; } = true;
}
