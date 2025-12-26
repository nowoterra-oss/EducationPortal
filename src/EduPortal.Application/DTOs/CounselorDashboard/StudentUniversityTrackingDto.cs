namespace EduPortal.Application.DTOs.CounselorDashboard;

public class StudentUniversityTrackingDto
{
    public int StudentId { get; set; }

    // Universite Basvurulari
    public List<UniversityApplicationDto> Applications { get; set; } = new();

    // Ozet Istatistikler
    public UniversitySummaryDto Summary { get; set; } = new();
}

public class UniversityApplicationDto
{
    public int Id { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Department { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime? ApplicationDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? Result { get; set; }

    // Gereksinimler ve Checklist
    public List<RequirementDto> Requirements { get; set; } = new();
    public int CompletedRequirements { get; set; }
    public int TotalRequirements { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class RequirementDto
{
    public int Id { get; set; }
    public string RequirementName { get; set; } = string.Empty;
    public string RequirementType { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysUntilDeadline { get; set; }
}

public class UniversitySummaryDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();
}
