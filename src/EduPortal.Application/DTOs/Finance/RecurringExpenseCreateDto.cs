using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class RecurringExpenseCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FinanceCategory Category { get; set; }
    public decimal Amount { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public int RecurrenceDay { get; set; } = 1;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? BranchId { get; set; }
    public string? Notes { get; set; }
}
