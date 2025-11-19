using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentDocument : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    [Required]
    public DocumentStatus Status { get; set; }

    [MaxLength(2000)]
    public string? ReviewNotes { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
