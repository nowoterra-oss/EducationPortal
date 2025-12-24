using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentTeacherAssignment : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    /// <summary>
    /// Kurs ID - Danışman/Koç atamaları için opsiyonel
    /// </summary>
    public int? CourseId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Atama türü: Danışman veya Koç
    /// </summary>
    public AssignmentType AssignmentType { get; set; } = AssignmentType.Advisor;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }
}
