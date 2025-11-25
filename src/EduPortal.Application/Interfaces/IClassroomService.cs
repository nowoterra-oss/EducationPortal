using EduPortal.Application.DTOs.Classroom;

namespace EduPortal.Application.Interfaces;

public interface IClassroomService
{
    Task<(IEnumerable<ClassroomDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? buildingName = null, bool? isLab = null);
    Task<ClassroomDto?> GetByIdAsync(int id);
    Task<ClassroomDto> CreateAsync(CreateClassroomDto dto);
    Task<ClassroomDto> UpdateAsync(int id, UpdateClassroomDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ClassroomDto>> GetAvailableAsync(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    Task<IEnumerable<ClassroomScheduleDto>> GetScheduleAsync(int classroomId);
}
