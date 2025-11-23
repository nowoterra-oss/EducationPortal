using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Teacher;

public class TeacherCreateDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? Specialization { get; set; }

    [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır")]
    public int? Experience { get; set; }

    public int? BranchId { get; set; }

    public bool IsAlsoCoach { get; set; } = false;
}
