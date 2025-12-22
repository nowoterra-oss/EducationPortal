using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Basit staj bilgileri
/// </summary>
public class SimpleInternship : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Position { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Industry { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsOngoing { get; set; } = false;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [MaxLength(255)]
    public string? CertificateFileName { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
