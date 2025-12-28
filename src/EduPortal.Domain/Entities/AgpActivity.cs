using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// AGP Timeline aktivite entity'si (örn: "SAT Eng- IELTS", 6 saat/hf)
/// </summary>
public class AgpActivity : BaseEntity
{
    [Required]
    public int AgpPeriodId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Aktivite başlangıç tarihi
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Aktivite bitiş tarihi
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Haftalık saat (opsiyonel)
    /// </summary>
    public int? HoursPerWeek { get; set; }

    /// <summary>
    /// Owner tipi (1-8 arası, renk kodu için)
    /// </summary>
    public int OwnerType { get; set; } = 1;

    /// <summary>
    /// Aktivite durumu
    /// </summary>
    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;

    /// <summary>
    /// İnceleme gerekli mi?
    /// </summary>
    public bool NeedsReview { get; set; } = false;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    [ForeignKey(nameof(AgpPeriodId))]
    public virtual AgpPeriod Period { get; set; } = null!;
}
