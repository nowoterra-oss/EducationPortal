using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Homework : BaseAuditableEntity
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    public int? ClassId { get; set; }

    public int? AcademicTermId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime AssignedDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [MaxLength(500)]
    public string? ResourceUrl { get; set; }

    public int? MaxScore { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(ClassId))]
    public virtual Class? Class { get; set; }

    [ForeignKey(nameof(AcademicTermId))]
    public virtual AcademicTerm? AcademicTerm { get; set; }

    public virtual ICollection<StudentHomeworkSubmission> Submissions { get; set; } = new List<StudentHomeworkSubmission>();
}
