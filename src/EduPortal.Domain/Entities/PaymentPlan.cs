using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ödeme planı şablonu (Örn: "10 Taksit", "12 Taksit")
/// </summary>
public class PaymentPlan : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string PlanName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int InstallmentCount { get; set; }

    /// <summary>
    /// Taksitler arası gün sayısı (varsayılan 30 gün)
    /// </summary>
    public int DaysBetweenInstallments { get; set; } = 30;

    /// <summary>
    /// Peşin indirim oranı (%)
    /// </summary>
    public decimal? DownPaymentDiscount { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual ICollection<StudentPaymentPlan> StudentPaymentPlans { get; set; } = new List<StudentPaymentPlan>();
}
