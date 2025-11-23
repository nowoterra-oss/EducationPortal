using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Teacher;

public class TeacherUpdateDto
{
    [Required]
    public int Id { get; set; }

    [StringLength(200)]
    public string? Specialization { get; set; }

    [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır")]
    public int? Experience { get; set; }

    public bool? IsActive { get; set; }

    public int? BranchId { get; set; }

    public bool? IsAlsoCoach { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
