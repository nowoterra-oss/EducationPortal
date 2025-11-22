using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.PackagePurchase;

public class CreateStudentPackagePurchaseDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int PackageId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal AmountPaid { get; set; }

    public int? RemainingSessions { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
