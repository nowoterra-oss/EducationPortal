using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Dashboard;

namespace EduPortal.Application.Interfaces;

public interface IDashboardService
{
    // Admin Dashboard
    Task<ApiResponse<AdminDashboardStatsDto>> GetAdminDashboardStatsAsync();

    // Teacher Dashboard
    Task<ApiResponse<TeacherDashboardStatsDto>> GetTeacherDashboardStatsAsync(int teacherId);

    // Student Dashboard
    Task<ApiResponse<StudentDashboardStatsDto>> GetStudentDashboardStatsAsync(int studentId);
    Task<ApiResponse<StudentDashboardDataDto>> GetStudentDashboardDataAsync(int studentId);

    // Charts
    Task<ApiResponse<ChartDataDto>> GetEnrollmentChartAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<ChartDataDto>> GetRevenueChartAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<ChartDataDto>> GetAttendanceChartAsync(DateTime startDate, DateTime endDate);

    // Recent Activities
    Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count = 10);
}
