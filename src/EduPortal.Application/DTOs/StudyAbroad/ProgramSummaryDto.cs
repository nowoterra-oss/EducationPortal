namespace EduPortal.Application.DTOs.StudyAbroad;

public class ProgramSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IntendedStartDate { get; set; }
    public int ProgressPercentage { get; set; }
}
