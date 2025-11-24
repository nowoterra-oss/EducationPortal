using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }

    public int? InstallmentId { get; set; }
    public int? PaymentPlanId { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    public PaymentStatus Status { get; set; }
    public string StatusName => Status.ToString();

    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();

    public string? TransactionId { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
