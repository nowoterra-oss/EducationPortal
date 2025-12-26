using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Characterix sonuc, analiz ve yorumlari
/// </summary>
public class CharacterixResult : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    public DateTime AssessmentDate { get; set; }

    [MaxLength(100)]
    public string? AssessmentVersion { get; set; }

    public string? ResultsJson { get; set; } // Detayli sonuclar JSON

    [MaxLength(2000)]
    public string? Analysis { get; set; }

    [MaxLength(2000)]
    public string? Interpretation { get; set; }

    [MaxLength(2000)]
    public string? Recommendations { get; set; }

    [MaxLength(500)]
    public string? ReportUrl { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
