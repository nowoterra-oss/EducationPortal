namespace EduPortal.Application.DTOs.PaymentPlan;

public class PaymentPlanDto
{
    public int Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InstallmentCount { get; set; }
    public int DaysBetweenInstallments { get; set; }
    public decimal? DownPaymentDiscount { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public int TotalUsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
