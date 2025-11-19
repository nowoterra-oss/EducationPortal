using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class UniversityApplication : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UniversityName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RequirementsUrl { get; set; }

    public DateTime? ApplicationStartDate { get; set; }

    [Required]
    public DateTime ApplicationDeadline { get; set; }

    public DateTime? DecisionDate { get; set; }

    [Required]
    public ApplicationStatus Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<UniversitySpecificExam> SpecificExams { get; set; } = new List<UniversitySpecificExam>();
}
