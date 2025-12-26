using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    public int? InstallmentId { get; set; }

    public int? PaymentPlanId { get; set; }

    public int? BranchId { get; set; }

    public int? PackagePurchaseId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Bekliyor;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedBy { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(InstallmentId))]
    public virtual PaymentInstallment? Installment { get; set; }

    [ForeignKey(nameof(PaymentPlanId))]
    public virtual PaymentPlan? PaymentPlan { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(PackagePurchaseId))]
    public virtual StudentPackagePurchase? PackagePurchase { get; set; }
}
