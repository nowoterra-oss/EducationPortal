namespace EduPortal.Application.DTOs.Dashboard;

public class AdminDashboardStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalClasses { get; set; }
    public int PendingPayments { get; set; }
    public decimal PendingPaymentAmount { get; set; }
    public int UnreadMessages { get; set; }
    public int TodayAttendanceCount { get; set; }
    public decimal AttendanceRate { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}
