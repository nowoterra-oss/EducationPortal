namespace EduPortal.Application.DTOs.Dashboard;

public class TeacherDashboardStatsDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public int TodayClassCount { get; set; }
    public int PendingHomeworks { get; set; }
    public int PendingGradings { get; set; }
    public List<TodayScheduleDto> TodaySchedule { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}
