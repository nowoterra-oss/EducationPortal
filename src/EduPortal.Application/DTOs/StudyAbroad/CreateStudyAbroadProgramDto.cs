using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudyAbroad;

public class CreateStudyAbroadProgramDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TargetCountry { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TargetUniversity { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProgramName { get; set; } = string.Empty;

    [Required]
    public int Level { get; set; } // ProgramLevel enum

    [Required]
    public DateTime IntendedStartDate { get; set; }

    [MaxLength(1000)]
    public string? Requirements { get; set; }

    public decimal? EstimatedCost { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
