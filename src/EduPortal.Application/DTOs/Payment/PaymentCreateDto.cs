using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Payment;

public class PaymentCreateDto
{
    [Required]
    public int StudentId { get; set; }

    public int? InstallmentId { get; set; }

    public int? PaymentPlanId { get; set; }

    public int? BranchId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalı")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
