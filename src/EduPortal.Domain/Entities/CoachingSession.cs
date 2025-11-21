using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CoachingSession : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    public int? BranchId { get; set; }

    [Required]
    public CoachingArea CoachingArea { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime SessionDate { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    [Required]
    public SessionType SessionType { get; set; }

    [Required]
    public SessionStatus Status { get; set; }

    [MaxLength(3000)]
    public string? SessionNotes { get; set; }

    [MaxLength(2000)]
    public string? ActionItems { get; set; }

    [MaxLength(2000)]
    public string? StudentFeedback { get; set; }

    public int? Rating { get; set; }

    public DateTime? NextSessionDate { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CoachId))]
    public virtual Coach Coach { get; set; } = null!;

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }
}
