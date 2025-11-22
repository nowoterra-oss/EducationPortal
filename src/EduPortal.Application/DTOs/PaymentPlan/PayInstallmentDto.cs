using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.PaymentPlan;

public class PayInstallmentDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? TransactionReference { get; set; }
}
