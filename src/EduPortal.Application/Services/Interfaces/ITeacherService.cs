using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Teacher;

namespace EduPortal.Application.Services.Interfaces;

public interface ITeacherService
{
    Task<ApiResponse<PagedResponse<TeacherDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<TeacherDto>> GetByIdAsync(int id);
    Task<ApiResponse<TeacherDto>> CreateAsync(TeacherCreateDto dto);
    Task<ApiResponse<TeacherDto>> UpdateAsync(TeacherUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<List<TeacherDto>>> SearchAsync(string searchTerm);
    Task<ApiResponse<List<object>>> GetTeacherCoursesAsync(int teacherId);
}
