using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Coach;

public class UpdateCoachDto
{
    public int? BranchId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Qualifications { get; set; }

    [Range(0, 50)]
    public int ExperienceYears { get; set; }

    public List<int> CoachingAreas { get; set; } = new List<int>();

    public bool IsAvailable { get; set; } = true;

    public decimal? HourlyRate { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }
}
