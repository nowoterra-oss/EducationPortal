using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.PackagePurchase;

public class UpdateStudentPackagePurchaseDto
{
    [Required]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal AmountPaid { get; set; }

    [Required]
    public int RemainingSessions { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }
}
