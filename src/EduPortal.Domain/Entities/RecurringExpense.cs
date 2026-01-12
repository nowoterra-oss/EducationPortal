using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class RecurringExpense : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public FinanceCategory Category { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public RecurrenceType RecurrenceType { get; set; }

    public int RecurrenceDay { get; set; } = 1;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? LastProcessedDate { get; set; }

    public DateTime? NextDueDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int? BranchId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    public virtual ICollection<FinanceRecord> FinanceRecords { get; set; } = new List<FinanceRecord>();
}
