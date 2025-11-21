using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class PaymentInstallment : BaseEntity
{
    [Required]
    public int PaymentPlanId { get; set; }

    [Required]
    public int InstallmentNumber { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? PaymentDate { get; set; }

    [Required]
    public PaymentStatus Status { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    [MaxLength(500)]
    public string? ReceiptUrl { get; set; }

    [ForeignKey(nameof(PaymentPlanId))]
    public virtual PaymentPlan PaymentPlan { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
