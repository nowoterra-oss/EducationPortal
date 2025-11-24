using EduPortal.Application.DTOs.WeeklySchedule;

namespace EduPortal.Application.Interfaces;

public interface IWeeklyScheduleService
{
    Task<(IEnumerable<WeeklyScheduleDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<WeeklyScheduleDto?> GetByIdAsync(int id);
    Task<WeeklyScheduleDto> CreateAsync(CreateWeeklyScheduleDto dto);
    Task<IEnumerable<WeeklyScheduleDto>> CreateBulkAsync(IEnumerable<CreateWeeklyScheduleDto> dtos);
    Task<WeeklyScheduleDto> UpdateAsync(int id, UpdateWeeklyScheduleDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<WeeklyScheduleDto>> GetByClassAsync(int classId);
    Task<IEnumerable<WeeklyScheduleDto>> GetByTeacherAsync(int teacherId);
    Task<IEnumerable<WeeklyScheduleDto>> GetByClassroomAsync(int classroomId);
    Task<IEnumerable<WeeklyScheduleDto>> GetTodayAsync();
}
