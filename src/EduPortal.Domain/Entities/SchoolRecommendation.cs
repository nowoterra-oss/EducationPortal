using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class SchoolRecommendation : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    public SchoolLevel SchoolLevel { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [Required]
    public SchoolType SchoolType { get; set; }

    [MaxLength(1000)]
    public string? Reasoning { get; set; }

    public int? RankingScore { get; set; }

    [Required]
    public RecommendationStatus Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CoachId))]
    public virtual Coach Coach { get; set; } = null!;
}
