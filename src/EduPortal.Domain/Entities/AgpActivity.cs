using EduPortal.Domain.Common;
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

    [Required]
    public int HoursPerWeek { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    [ForeignKey(nameof(AgpPeriodId))]
    public virtual AgpPeriod Period { get; set; } = null!;
}
