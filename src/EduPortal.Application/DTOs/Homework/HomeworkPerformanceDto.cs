namespace EduPortal.Application.DTOs.Homework;

public class StudentHomeworkPerformanceDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedOnTime { get; set; }
    public int CompletedLate { get; set; }
    public int Pending { get; set; }
    public int Overdue { get; set; }
    public double AverageCompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public double OnTimeRate { get; set; } // Zamanında teslim oranı
    public List<MonthlyPerformanceDto> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyPerformanceDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int Completed { get; set; }
    public double AverageScore { get; set; }
}

public class HomeworkPerformanceChartDto
{
    public List<string> Labels { get; set; } = new(); // Ay isimleri
    public List<int> CompletedData { get; set; } = new();
    public List<int> PendingData { get; set; } = new();
    public List<int> OverdueData { get; set; } = new();
    public List<double> AverageScores { get; set; } = new();
}
