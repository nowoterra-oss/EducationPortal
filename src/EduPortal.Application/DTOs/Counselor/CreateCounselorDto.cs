using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Counselor;

public class CreateCounselorDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Specialization { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateCounselorFromTeacherDto
{
    [MaxLength(200)]
    public string? Specialization { get; set; }
}
