namespace EduPortal.Application.DTOs.Payment;

public class PaymentSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
}
