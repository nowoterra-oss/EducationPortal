namespace EduPortal.Application.DTOs.Visa;

public class VisaSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? InterviewDate { get; set; }
}
