namespace EduPortal.Application.DTOs.PackagePurchase;

public class PurchaseSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal AmountPaid { get; set; }
    public bool IsActive { get; set; }
    public int RemainingSessions { get; set; }
}
