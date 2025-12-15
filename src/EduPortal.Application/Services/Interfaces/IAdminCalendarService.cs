using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Calendar;

namespace EduPortal.Application.Services.Interfaces;

public interface IAdminCalendarService
{
    Task<ApiResponse<List<AdminCalendarEventDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<AdminCalendarEventDto>> GetByIdAsync(int id);
    Task<ApiResponse<AdminCalendarEventDto>> CreateAsync(AdminCalendarEventCreateDto dto);
    Task<ApiResponse<AdminCalendarEventDto>> UpdateAsync(int id, AdminCalendarEventUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
