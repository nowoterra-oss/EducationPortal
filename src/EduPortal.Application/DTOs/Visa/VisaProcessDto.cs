namespace EduPortal.Application.DTOs.Visa;

public class VisaProcessDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Embassy { get; set; }
    public decimal? ApplicationFee { get; set; }
    public string? VisaNumber { get; set; }
    public string? Notes { get; set; }
    public int DaysInProcess { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
