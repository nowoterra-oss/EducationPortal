using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Coach;

public class CreateCoachDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Qualifications { get; set; }

    [Range(0, 50)]
    public int ExperienceYears { get; set; }

    public List<int> CoachingAreas { get; set; } = new List<int>(); // Array of CoachingArea enum values

    public decimal? HourlyRate { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public bool IsAlsoTeacher { get; set; } = false;
    public int? TeacherId { get; set; }
}
