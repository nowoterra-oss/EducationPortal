using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class RecurringExpenseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public FinanceCategory Category { get; set; }
    public string CategoryName => Category.ToString();

    public decimal Amount { get; set; }

    public RecurrenceType RecurrenceType { get; set; }
    public string RecurrenceTypeName => RecurrenceType.ToString();

    public int RecurrenceDay { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LastProcessedDate { get; set; }
    public DateTime? NextDueDate { get; set; }

    public bool IsActive { get; set; }

    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
