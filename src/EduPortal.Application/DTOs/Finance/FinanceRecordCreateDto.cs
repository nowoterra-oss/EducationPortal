using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class FinanceRecordCreateDto
{
    public FinanceType Type { get; set; }
    public FinanceCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? DocumentUrl { get; set; }
    public int? BranchId { get; set; }
    public int? RelatedStudentId { get; set; }
    public int? RelatedTeacherId { get; set; }
    public string? Notes { get; set; }
}
