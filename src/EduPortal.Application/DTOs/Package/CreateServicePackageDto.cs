using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Package;

public class CreateServicePackageDto
{
    [Required]
    [MaxLength(200)]
    public string PackageName { get; set; } = string.Empty;

    [Required]
    public int Type { get; set; } // PackageType enum

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public int? SessionCount { get; set; }

    [Range(1, 36)]
    public int? ValidityMonths { get; set; }

    [MaxLength(1000)]
    public string? Includes { get; set; }

    public bool IsActive { get; set; } = true;
}
