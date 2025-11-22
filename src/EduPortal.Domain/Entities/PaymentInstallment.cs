using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Taksit detayları
/// </summary>
public class PaymentInstallment : BaseAuditableEntity
{
    [Required]
    public int StudentPaymentPlanId { get; set; }

    [Required]
    public int InstallmentNumber { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;

    /// <summary>
    /// İlgili ödeme ID (Payment tablosundan)
    /// </summary>
    public int? PaymentId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Gecikme günü
    /// </summary>
    public int DaysOverdue => Status == InstallmentStatus.Overdue && DueDate < DateTime.Now
        ? (DateTime.Now - DueDate).Days
        : 0;

    // Navigation Properties
    [ForeignKey(nameof(StudentPaymentPlanId))]
    public virtual StudentPaymentPlan StudentPaymentPlan { get; set; } = null!;

    [ForeignKey(nameof(PaymentId))]
    public virtual Payment? Payment { get; set; }
}
