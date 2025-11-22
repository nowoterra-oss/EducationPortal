using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Attendance;

namespace EduPortal.Application.Services.Interfaces;

public interface IAttendanceService
{
    Task<ApiResponse<PagedResponse<AttendanceDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<AttendanceDto>> GetByIdAsync(int id);
    Task<ApiResponse<AttendanceDto>> RecordAttendanceAsync(AttendanceCreateDto dto, int teacherId);
    Task<ApiResponse<AttendanceDto>> UpdateAsync(int id, AttendanceCreateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<List<AttendanceDto>>> GetStudentAttendanceAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<Dictionary<string, int>>> GetAttendanceSummaryAsync(int studentId);
    Task<ApiResponse<List<AttendanceDto>>> GetCourseAttendanceAsync(int courseId);
}
