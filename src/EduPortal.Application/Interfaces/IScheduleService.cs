using EduPortal.Application.DTOs.Schedule;

namespace EduPortal.Application.Interfaces;

public interface IScheduleService
{
    Task<(IEnumerable<ScheduleDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<ScheduleDto?> GetByIdAsync(int id);
    Task<ScheduleDto> CreateAsync(CreateScheduleDto dto);
    Task<ScheduleDto> UpdateAsync(int id, UpdateScheduleDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ScheduleDto>> GetByStudentAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ScheduleDto>> GetByTeacherAsync(int teacherId, DateTime? startDate = null, DateTime? endDate = null);
}
