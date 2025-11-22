using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudyAbroad;

public class UpdateStudyAbroadProgramDto
{
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
    public int Level { get; set; }

    [Required]
    public DateTime IntendedStartDate { get; set; }

    [Required]
    public int Status { get; set; } // StudyAbroadStatus enum

    [MaxLength(1000)]
    public string? Requirements { get; set; }

    public decimal? EstimatedCost { get; set; }

    public decimal? ActualCost { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
