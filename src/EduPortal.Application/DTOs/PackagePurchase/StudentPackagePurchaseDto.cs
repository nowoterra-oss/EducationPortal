namespace EduPortal.Application.DTOs.PackagePurchase;

public class StudentPackagePurchaseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? TotalSessions { get; set; }
    public int RemainingSessions { get; set; }
    public int UsedSessions { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int? DaysRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
