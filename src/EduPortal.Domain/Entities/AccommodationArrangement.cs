using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class AccommodationArrangement : BaseAuditableEntity
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    public AccommodationType Type { get; set; }

    [MaxLength(300)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyRent { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Deposit { get; set; }

    [Required]
    public AccommodationStatus Status { get; set; }

    [MaxLength(500)]
    public string? ContractUrl { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ProgramId))]
    public virtual StudyAbroadProgram Program { get; set; } = null!;
}
