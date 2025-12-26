namespace EduPortal.Application.DTOs.CounselorDashboard;

public class UpcomingDeadlineDto
{
    public string DeadlineType { get; set; } = string.Empty; // "UniversityApplication", "ExamRegistration", "DocumentSubmission"
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsUrgent { get; set; } // 7 gun veya daha az
    public bool IsOverdue { get; set; }
    public string? RelatedEntityType { get; set; } // "UniversityApplication", "StudentExamCalendar"
    public int? RelatedEntityId { get; set; }
}

public class CounselorDashboardSummaryDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    // Yaklaşan Deadline'lar (7 gun icinde)
    public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();

    // Son Gorusme Bilgisi
    public DateTime? LastMeetingDate { get; set; }
    public string? LastMeetingNotes { get; set; }

    // Sonraki Gorusme
    public DateTime? NextMeetingDate { get; set; }

    // Odev Durumu
    public int PendingHomeworks { get; set; }
    public int OverdueHomeworks { get; set; }

    // AGP Durumu
    public decimal? AgpProgress { get; set; }

    // Basvuru Durumu
    public int PendingApplications { get; set; }
    public int UpcomingExams { get; set; }
}
