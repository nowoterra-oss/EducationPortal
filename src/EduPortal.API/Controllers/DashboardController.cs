using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Dashboard;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Dashboard endpoints for admin, teacher, and student dashboards
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetAdminDashboard()
    {
        try
        {
            var result = await _dashboardService.GetAdminDashboardStatsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAdminDashboard hatası");
            return StatusCode(500, ApiResponse<AdminDashboardStatsDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get teacher dashboard statistics
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<TeacherDashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TeacherDashboardStatsDto>>> GetTeacherDashboard(int teacherId)
    {
        try
        {
            var result = await _dashboardService.GetTeacherDashboardStatsAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTeacherDashboard hatası - TeacherId: {TeacherId}", teacherId);
            return StatusCode(500, ApiResponse<TeacherDashboardStatsDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get student dashboard statistics (legacy)
    /// </summary>
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<StudentDashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentDashboardStatsDto>>> GetStudentDashboard(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentDashboardStatsAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStudentDashboard hatası - StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentDashboardStatsDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get student dashboard data (optimized - single endpoint for all dashboard data)
    /// </summary>
    /// <remarks>
    /// Bu endpoint, öğrenci dashboard'u için gerekli tüm verileri tek seferde döndürür.
    /// Frontend'de birden fazla API çağrısı yerine bu tek endpoint kullanılmalıdır.
    /// </remarks>
    [HttpGet("student/{studentId}/data")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<StudentDashboardDataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentDashboardDataDto>>> GetStudentDashboardData(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentDashboardDataAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStudentDashboardData hatası - StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentDashboardDataDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get enrollment chart data
    /// </summary>
    [HttpGet("charts/enrollment")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ChartDataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartDataDto>>> GetEnrollmentChart(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _dashboardService.GetEnrollmentChartAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEnrollmentChart hatası");
            return StatusCode(500, ApiResponse<ChartDataDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get revenue chart data
    /// </summary>
    [HttpGet("charts/revenue")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<ChartDataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartDataDto>>> GetRevenueChart(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _dashboardService.GetRevenueChartAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRevenueChart hatası");
            return StatusCode(500, ApiResponse<ChartDataDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get attendance chart data
    /// </summary>
    [HttpGet("charts/attendance")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<ChartDataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartDataDto>>> GetAttendanceChart(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _dashboardService.GetAttendanceChartAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAttendanceChart hatası");
            return StatusCode(500, ApiResponse<ChartDataDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Get recent activities
    /// </summary>
    [HttpGet("activities")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<RecentActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RecentActivityDto>>>> GetRecentActivities([FromQuery] int count = 10)
    {
        try
        {
            var result = await _dashboardService.GetRecentActivitiesAsync(count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRecentActivities hatası");
            return StatusCode(500, ApiResponse<List<RecentActivityDto>>.ErrorResponse("Sunucu hatası"));
        }
    }
}
