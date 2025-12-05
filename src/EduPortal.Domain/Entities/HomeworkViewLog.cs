using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğrencinin ödevi ne zaman gördüğünün logu
/// </summary>
public class HomeworkViewLog : BaseAuditableEntity
{
    [Required]
    public int HomeworkAssignmentId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HomeworkAssignmentId))]
    public virtual HomeworkAssignment HomeworkAssignment { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
