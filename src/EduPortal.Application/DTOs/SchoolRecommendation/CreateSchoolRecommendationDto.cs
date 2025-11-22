using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.SchoolRecommendation;

public class CreateSchoolRecommendationDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    public int SchoolLevel { get; set; } // SchoolLevel enum

    [Required]
    public int SchoolType { get; set; } // SchoolType enum

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(1000)]
    public string? Reasoning { get; set; }

    public int? RankingScore { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
