namespace EduPortal.Application.DTOs.PaymentPlan;

public class PaymentInstallmentDto
{
    public int Id { get; set; }
    public int StudentPaymentPlanId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentNo { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? PaymentId { get; set; }
    public string? Notes { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsOverdue => Status == "Overdue";
    public bool IsUpcoming => DueDate <= DateTime.Now.AddDays(7) && Status == "Pending";
}
