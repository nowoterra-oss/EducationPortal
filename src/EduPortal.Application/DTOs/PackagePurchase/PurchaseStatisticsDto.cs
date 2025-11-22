namespace EduPortal.Application.DTOs.PackagePurchase;

public class PurchaseStatisticsDto
{
    public int TotalPurchases { get; set; }
    public int ActivePurchases { get; set; }
    public int ExpiredPurchases { get; set; }
    public int PurchasesThisMonth { get; set; }
    public int PurchasesThisYear { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal AveragePurchaseAmount { get; set; }

    public Dictionary<string, int> PurchasesByPackageType { get; set; } = new();
    public Dictionary<string, decimal> RevenueByPackageType { get; set; } = new();

    public List<MonthlyRevenueDto> MonthlyRevenueChart { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public int PurchaseCount { get; set; }
}
