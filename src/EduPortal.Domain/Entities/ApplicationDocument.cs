using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class ApplicationDocument : BaseAuditableEntity
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    [MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    [Required]
    public DocumentStatus Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ProgramId))]
    public virtual StudyAbroadProgram Program { get; set; } = null!;
}
