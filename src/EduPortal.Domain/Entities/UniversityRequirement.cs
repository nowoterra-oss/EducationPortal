using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Universite basvuru gereksinimleri ve checklist
/// </summary>
public class UniversityRequirement : BaseEntity
{
    [Required]
    public int UniversityApplicationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string RequirementName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string RequirementType { get; set; } = string.Empty; // "Document", "Exam", "Essay", "Other"

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedDate { get; set; }

    public DateTime? Deadline { get; set; }

    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(UniversityApplicationId))]
    public virtual UniversityApplication UniversityApplication { get; set; } = null!;
}
