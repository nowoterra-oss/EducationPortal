using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// AGP Timeline sınav/hedef entity'si (örn: "SAT Ağustos", "IELTS 29 Ağu.")
/// </summary>
public class AgpTimelineMilestone : BaseEntity
{
    [Required]
    public int AgpPeriodId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "exam"; // "exam", "goal", "event"

    /// <summary>
    /// Milestone olarak işaretlensin mi? (Timeline'da elmas şeklinde gösterilir)
    /// </summary>
    public bool IsMilestone { get; set; } = false;

    /// <summary>
    /// Başvuru başlangıç tarihi (opsiyonel)
    /// </summary>
    public DateTime? ApplicationStartDate { get; set; }

    /// <summary>
    /// Başvuru bitiş tarihi (opsiyonel)
    /// </summary>
    public DateTime? ApplicationEndDate { get; set; }

    /// <summary>
    /// Sınav/hedef puanı (opsiyonel)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Score { get; set; }

    /// <summary>
    /// Maksimum puan (opsiyonel)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Sonuç notları (opsiyonel)
    /// </summary>
    [MaxLength(1000)]
    public string? ResultNotes { get; set; }

    /// <summary>
    /// Tamamlandı mı? (opsiyonel)
    /// </summary>
    public bool? IsCompleted { get; set; }

    // Navigation property
    [ForeignKey(nameof(AgpPeriodId))]
    public virtual AgpPeriod Period { get; set; } = null!;
}
