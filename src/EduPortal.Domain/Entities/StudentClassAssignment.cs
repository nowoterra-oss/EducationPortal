using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentClassAssignment : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int ClassId { get; set; }

    [Required]
    public int AcademicTermId { get; set; }

    [Required]
    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(ClassId))]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey(nameof(AcademicTermId))]
    public virtual AcademicTerm AcademicTerm { get; set; } = null!;
}
