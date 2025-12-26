namespace EduPortal.Application.DTOs.CounselorDashboard;

public class StudentAcademicPerformanceDto
{
    public int StudentId { get; set; }

    // Odev Performansi
    public HomeworkPerformanceDto HomeworkPerformance { get; set; } = new();

    // Sinav Sonuclari (Unite + Deneme)
    public List<ExamResultSummaryDto> ExamResults { get; set; } = new();

    // Hazir Bulunusluk Sinavlari
    public List<ReadinessExamDto> ReadinessExams { get; set; } = new();

    // AGP Ozeti
    public AgpSummaryDto? AgpSummary { get; set; }
}

public class HomeworkPerformanceDto
{
    public int TotalAssigned { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Late { get; set; }
    public decimal CompletionRate { get; set; } // Yuzde
    public decimal AverageScore { get; set; }
}

public class ExamResultSummaryDto
{
    public string ExamType { get; set; } = string.Empty; // "Unite", "Deneme"
    public string Subject { get; set; } = string.Empty;
    public int ExamCount { get; set; }
    public decimal AverageScore { get; set; }
    public List<ExamScoreDataPoint> ScoreHistory { get; set; } = new(); // Grafik icin
}

public class ExamScoreDataPoint
{
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
}

public class AgpSummaryDto
{
    public int AgpId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public decimal OverallProgress { get; set; }
    public List<AgpPeriodSummaryDto> Periods { get; set; } = new();
}

public class AgpPeriodSummaryDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Color { get; set; }
    public List<string> Milestones { get; set; } = new();
    public List<string> Activities { get; set; } = new();
}
