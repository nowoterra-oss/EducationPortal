using EduPortal.Application.DTOs.Course;

namespace EduPortal.Application.Interfaces;

public interface ICourseService
{
    Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<CourseDto?> GetByIdAsync(int id);
    Task<CourseDto> CreateAsync(CreateCourseDto dto);
    Task<CourseDto> UpdateAsync(int id, UpdateCourseDto dto);
    Task<bool> DeleteAsync(int id);

    // Curriculum operations
    Task<IEnumerable<CurriculumDto>> GetCurriculumAsync(int courseId);
    Task<IEnumerable<CurriculumDto>> UpdateCurriculumAsync(int courseId, UpdateCurriculumDto dto);

    // Resource operations
    Task<IEnumerable<CourseResourceDto>> GetResourcesAsync(int courseId);
    Task<CourseResourceDto> AddResourceAsync(int courseId, CreateCourseResourceDto dto);
    Task<bool> DeleteResourceAsync(int courseId, int resourceId);
}
