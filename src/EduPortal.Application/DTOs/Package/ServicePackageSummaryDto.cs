namespace EduPortal.Application.DTOs.Package;

public class ServicePackageSummaryDto
{
    public int Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? SessionCount { get; set; }
    public bool IsActive { get; set; }
    public int TotalPurchases { get; set; }
}
