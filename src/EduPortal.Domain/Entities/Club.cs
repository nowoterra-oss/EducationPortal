using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Club : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string ClubName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int? AdvisorTeacherId { get; set; }

    public int MaxMembers { get; set; } = 30;

    public int CurrentMembers { get; set; } = 0;

    [MaxLength(100)]
    public string? MeetingDay { get; set; } // "Pazartesi", "Salı"

    [MaxLength(50)]
    public string? MeetingTime { get; set; } // "15:00-16:00"

    [MaxLength(100)]
    public string? MeetingRoom { get; set; }

    public bool IsActive { get; set; } = true;

    public bool AcceptingMembers { get; set; } = true;

    [MaxLength(20)]
    public string? AcademicYear { get; set; } // "2024-2025"

    [MaxLength(500)]
    public string? Requirements { get; set; }

    [ForeignKey(nameof(AdvisorTeacherId))]
    public virtual Teacher? Advisor { get; set; }
}
