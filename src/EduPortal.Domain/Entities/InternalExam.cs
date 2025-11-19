using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class InternalExam : BaseAuditableEntity
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ExamType { get; set; } = string.Empty; // "Deneme", "UniteSonu", "Donem"

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ExamDate { get; set; }

    public int? Duration { get; set; } // Minutes

    [Required]
    public int MaxScore { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    public virtual ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
