using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.SchoolRecommendation;

public class UpdateSchoolRecommendationDto
{
    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    public int SchoolLevel { get; set; }

    [Required]
    public int SchoolType { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [Required]
    public int Status { get; set; } // RecommendationStatus enum

    [MaxLength(1000)]
    public string? Reasoning { get; set; }

    public int? RankingScore { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
