using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.PaymentPlan;

public class CreateStudentPaymentPlanDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int PaymentPlanId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public int? PackagePurchaseId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// İlk taksit tutarı (farklı ise)
    /// </summary>
    public decimal? FirstInstallmentAmount { get; set; }
}
