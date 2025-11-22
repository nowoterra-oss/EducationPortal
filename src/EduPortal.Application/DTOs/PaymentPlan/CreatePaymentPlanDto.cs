using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.PaymentPlan;

public class CreatePaymentPlanDto
{
    [Required]
    [MaxLength(200)]
    public string PlanName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 36)]
    public int InstallmentCount { get; set; }

    [Range(1, 365)]
    public int DaysBetweenInstallments { get; set; } = 30;

    [Range(0, 100)]
    public decimal? DownPaymentDiscount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
