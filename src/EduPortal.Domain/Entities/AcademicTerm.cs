using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class AcademicTerm : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string TermName { get; set; } = string.Empty; // "2024-2025 1. Dönem"

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty; // "2024-2025"

    [Required]
    public int TermNumber { get; set; } // 1, 2

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public DateTime? MidtermStartDate { get; set; }

    public DateTime? MidtermEndDate { get; set; }

    public DateTime? FinalStartDate { get; set; }

    public DateTime? FinalEndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCurrent { get; set; } = false;

    [MaxLength(500)]
    public string? Description { get; set; }
}
