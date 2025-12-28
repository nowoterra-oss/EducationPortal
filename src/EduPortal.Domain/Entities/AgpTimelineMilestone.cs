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

    // Navigation property
    [ForeignKey(nameof(AgpPeriodId))]
    public virtual AgpPeriod Period { get; set; } = null!;
}
