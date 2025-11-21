using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Coach : BaseAuditableEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Qualifications { get; set; }

    public int ExperienceYears { get; set; }

    [MaxLength(500)]
    public string? AreasJson { get; set; } // JSON array of CoachingArea

    public bool IsAvailable { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public bool IsAlsoTeacher { get; set; } = false;
    public int? TeacherId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? TeacherProfile { get; set; }

    public virtual ICollection<StudentCoachAssignment> Students { get; set; } = new List<StudentCoachAssignment>();
    public virtual ICollection<CoachingSession> Sessions { get; set; } = new List<CoachingSession>();
}
