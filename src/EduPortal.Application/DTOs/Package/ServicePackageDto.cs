namespace EduPortal.Application.DTOs.Package;

public class ServicePackageDto
{
    public int Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? SessionCount { get; set; }
    public int? ValidityMonths { get; set; }
    public string? Includes { get; set; }
    public bool IsActive { get; set; }

    // Statistics
    public int TotalPurchases { get; set; }
    public int ActivePurchases { get; set; }
    public decimal TotalRevenue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
