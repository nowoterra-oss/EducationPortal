namespace EduPortal.Application.DTOs.UniversityApplication;

public class ApplicationStatisticsDto
{
    public int TotalApplications { get; set; }
    public int PlanningCount { get; set; }
    public int AppliedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }
    public double AcceptanceRate { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
    public List<CountryStatisticsDto> ByCountry { get; set; } = new();
    public List<MonthlyStatisticsDto> ByMonth { get; set; } = new();
}

public class CountryStatisticsDto
{
    public string Country { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
}

public class MonthlyStatisticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
    public int DeadlineCount { get; set; }
}
