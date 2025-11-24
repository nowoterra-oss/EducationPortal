using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Counselor;

public class UpdateCounselorDto
{
    [MaxLength(200)]
    public string? Specialization { get; set; }

    public bool IsActive { get; set; }
}
