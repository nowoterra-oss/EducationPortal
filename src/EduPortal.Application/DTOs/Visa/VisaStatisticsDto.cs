namespace EduPortal.Application.DTOs.Visa;

public class VisaStatisticsDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int InProcessApplications { get; set; }

    public Dictionary<string, int> ApplicationsByCountry { get; set; } = new();
    public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();

    public decimal TotalApplicationFees { get; set; }
    public decimal AverageProcessingDays { get; set; }
    public decimal ApprovalRate { get; set; }
}
