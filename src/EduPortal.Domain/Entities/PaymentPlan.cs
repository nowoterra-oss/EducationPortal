using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class PaymentPlan : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public int InstallmentCount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // "Aktif", "Tamamlandi", "Iptal"

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<PaymentInstallment> Installments { get; set; } = new List<PaymentInstallment>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
