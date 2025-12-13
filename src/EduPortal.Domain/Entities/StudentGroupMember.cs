using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Grup Üyeliği - Hangi öğrenci hangi grupta
/// </summary>
public class StudentGroupMember : BaseAuditableEntity
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Gruba katılma tarihi
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gruptan ayrılma tarihi (aktif değilse)
    /// </summary>
    public DateTime? LeftAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    [ForeignKey(nameof(GroupId))]
    public virtual StudentGroup Group { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
