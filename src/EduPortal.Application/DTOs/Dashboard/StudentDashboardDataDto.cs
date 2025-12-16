namespace EduPortal.Application.DTOs.Dashboard;

public class StudentDashboardDataDto
{
    public StudentBasicInfoDto Student { get; set; } = new();
    public DashboardStatsDto Stats { get; set; } = new();
    public List<RecentHomeworkDto> RecentHomeworks { get; set; } = new();
    public List<UpcomingExamDto> UpcomingExams { get; set; } = new();
}

public class StudentBasicInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public string? StudentNo { get; set; }
    public string? Email { get; set; }
}

public class DashboardStatsDto
{
    public int PendingHomeworkCount { get; set; }
    public int UpcomingExamCount { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal AverageGrade { get; set; }
}

public class RecentHomeworkDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public string? TeacherName { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "pending";
    public string Priority { get; set; } = "medium";
}

public class UpcomingExamDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public DateTime ExamDate { get; set; }
    public string? ExamType { get; set; }
}
