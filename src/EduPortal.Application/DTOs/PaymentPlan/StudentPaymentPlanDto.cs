namespace EduPortal.Application.DTOs.PaymentPlan;

public class StudentPaymentPlanDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentNo { get; set; }
    public int PaymentPlanId { get; set; }
    public string? PaymentPlanName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int TotalInstallments { get; set; }
    public int PaidInstallments { get; set; }
    public int OverdueInstallments { get; set; }
    public decimal CompletionPercentage => TotalAmount > 0 ? (PaidAmount / TotalAmount) * 100 : 0;
    public DateTime CreatedAt { get; set; }
}
