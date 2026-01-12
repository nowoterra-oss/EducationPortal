using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class TeacherSalary : BaseAuditableEntity
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Deduction { get; set; } = 0;

    [NotMapped]
    public decimal NetSalary => BaseSalary + Bonus - Deduction;

    [Required]
    public int Year { get; set; }

    [Required]
    public int Month { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    public SalaryStatus Status { get; set; } = SalaryStatus.Bekliyor;

    public PaymentMethod? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? ProcessedBy { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;
}
