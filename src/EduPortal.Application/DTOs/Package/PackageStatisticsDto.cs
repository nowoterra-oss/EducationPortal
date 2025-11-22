namespace EduPortal.Application.DTOs.Package;

public class PackageStatisticsDto
{
    public int TotalPackages { get; set; }
    public int ActivePackages { get; set; }
    public int TotalPurchases { get; set; }
    public int ActivePurchases { get; set; }

    public Dictionary<string, int> PackagesByType { get; set; } = new();
    public Dictionary<string, int> PurchasesByPackage { get; set; } = new();

    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal AveragePackagePrice { get; set; }

    public List<TopSellingPackageDto> TopSellingPackages { get; set; } = new();
}

public class TopSellingPackageDto
{
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal Revenue { get; set; }
}
