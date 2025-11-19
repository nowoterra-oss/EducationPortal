using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;

namespace EduPortal.Application.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<PagedResponse<StudentDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<StudentDto>> GetByIdAsync(int id);
    Task<ApiResponse<StudentDto>> GetByStudentNoAsync(string studentNo);
    Task<ApiResponse<StudentDto>> CreateAsync(StudentCreateDto dto);
    Task<ApiResponse<StudentDto>> UpdateAsync(StudentUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<List<StudentDto>>> SearchAsync(string searchTerm);
}
