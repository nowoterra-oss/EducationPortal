namespace EduPortal.Application.DTOs.Dashboard;

public class StudentDashboardStatsDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int EnrolledCourses { get; set; }
    public int PendingHomeworks { get; set; }
    public int UpcomingExams { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal AverageGrade { get; set; }
    public int UnreadMessages { get; set; }
    public List<UpcomingEventDto> UpcomingEvents { get; set; } = new();
    public List<RecentGradeDto> RecentGrades { get; set; } = new();
}
