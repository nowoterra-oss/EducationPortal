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
    /// Dekont dosya yolu
    /// </summary>
    [MaxLength(500)]
    public string? ReceiptPath { get; set; }

    /// <summary>
    /// Dekont yükleme tarihi
    /// </summary>
    public DateTime? ReceiptUploadDate { get; set; }

    /// <summary>
    /// Onaylayan admin ID (ApplicationUser Id - string)
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Onay tarihi
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Red sebebi
    /// </summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Ödeme yöntemi (cash, bank_transfer, pos, other, receipt)
    /// </summary>
    [MaxLength(20)]
    public string? PaymentMethod { get; set; }

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

    [ForeignKey(nameof(ApprovedBy))]
    public virtual ApplicationUser? ApprovedByUser { get; set; }
}
