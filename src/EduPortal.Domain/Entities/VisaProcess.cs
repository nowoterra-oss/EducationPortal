using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class VisaProcess : BaseAuditableEntity
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(100)]
    public string VisaType { get; set; } = string.Empty;

    [Required]
    public DateTime ApplicationDate { get; set; }

    public DateTime? InterviewDate { get; set; }

    public DateTime? DecisionDate { get; set; }

    [Required]
    public VisaStatus Status { get; set; }

    [MaxLength(500)]
    public string? ConsulateLocation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApplicationFee { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? VisaDocumentUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ProgramId))]
    public virtual StudyAbroadProgram Program { get; set; } = null!;
}
