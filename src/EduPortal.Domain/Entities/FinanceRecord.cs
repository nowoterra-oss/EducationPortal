using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class FinanceRecord : BaseAuditableEntity
{
    [Required]
    public FinanceType Type { get; set; }

    [Required]
    public FinanceCategory Category { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    public int? BranchId { get; set; }

    public int? RelatedStudentId { get; set; }

    public int? RelatedTeacherId { get; set; }

    public int? RecurringExpenseId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(RelatedStudentId))]
    public virtual Student? RelatedStudent { get; set; }

    [ForeignKey(nameof(RelatedTeacherId))]
    public virtual Teacher? RelatedTeacher { get; set; }

    [ForeignKey(nameof(RecurringExpenseId))]
    public virtual RecurringExpense? RecurringExpense { get; set; }
}
