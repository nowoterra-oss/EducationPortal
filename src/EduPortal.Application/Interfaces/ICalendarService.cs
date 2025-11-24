using EduPortal.Application.DTOs.Calendar;

namespace EduPortal.Application.Interfaces;

public interface ICalendarService
{
    Task<(IEnumerable<CalendarEventDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<CalendarEventDto?> GetByIdAsync(int id);
    Task<CalendarEventDto> CreateAsync(CreateCalendarEventDto dto);
    Task<CalendarEventDto> UpdateAsync(int id, UpdateCalendarEventDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CalendarEventDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<CalendarEventDto>> GetByStudentAsync(int studentId);
    Task<IEnumerable<CalendarEventDto>> GetUpcomingAsync(int days);
    Task<IEnumerable<CalendarEventDto>> GetByClassAsync(int classId);
}
