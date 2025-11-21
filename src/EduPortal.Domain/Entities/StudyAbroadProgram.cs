using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudyAbroadProgram : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TargetCountry { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TargetUniversity { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProgramName { get; set; } = string.Empty;

    [Required]
    public ProgramLevel Level { get; set; }

    [Required]
    public DateTime IntendedStartDate { get; set; }

    [Required]
    public StudyAbroadStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Requirements { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CoachId))]
    public virtual Coach Coach { get; set; } = null!;

    public virtual ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public virtual ICollection<VisaProcess> VisaProcesses { get; set; } = new List<VisaProcess>();
    public virtual ICollection<AccommodationArrangement> Accommodations { get; set; } = new List<AccommodationArrangement>();
}
