using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğrenciye atanmış ödeme planı
/// </summary>
public class StudentPaymentPlan : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int PaymentPlanId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public PaymentPlanStatus Status { get; set; } = PaymentPlanStatus.Active;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// İlgili paket satın alma ID (opsiyonel)
    /// </summary>
    public int? PackagePurchaseId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(PaymentPlanId))]
    public virtual PaymentPlan PaymentPlan { get; set; } = null!;

    [ForeignKey(nameof(PackagePurchaseId))]
    public virtual StudentPackagePurchase? PackagePurchase { get; set; }

    public virtual ICollection<PaymentInstallment> Installments { get; set; } = new List<PaymentInstallment>();
}
