namespace EduPortal.Application.DTOs.AGP;

public class AGPProgressDto
{
    public int AGPId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public int InProgressMilestones { get; set; }
    public int PendingMilestones { get; set; }
    public int OverallCompletionPercentage { get; set; }
    public List<MonthlyProgressDto> MonthlyProgress { get; set; } = new();
}

public class MonthlyProgressDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int MilestoneCount { get; set; }
    public int CompletedCount { get; set; }
    public int AverageCompletionPercentage { get; set; }
}
