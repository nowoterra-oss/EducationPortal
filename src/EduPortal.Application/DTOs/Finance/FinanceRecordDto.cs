using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class FinanceRecordDto
{
    public int Id { get; set; }
    public FinanceType Type { get; set; }
    public string TypeName => Type.ToString();

    public FinanceCategory Category { get; set; }
    public string CategoryName => Category.ToString();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName => PaymentMethod?.ToString();

    public string? TransactionId { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? DocumentUrl { get; set; }

    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    public int? RelatedStudentId { get; set; }
    public string? RelatedStudentName { get; set; }

    public int? RelatedTeacherId { get; set; }
    public string? RelatedTeacherName { get; set; }

    public int? RecurringExpenseId { get; set; }
    public string? RecurringExpenseTitle { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
