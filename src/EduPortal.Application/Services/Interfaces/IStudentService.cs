using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;

namespace EduPortal.Application.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<PagedResponse<StudentDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false);
    Task<ApiResponse<StudentDto>> GetByIdAsync(int id);
    Task<ApiResponse<StudentDto>> GetByStudentNoAsync(string studentNo);
    Task<ApiResponse<StudentDto>> CreateAsync(StudentCreateDto dto);
    Task<ApiResponse<StudentDto>> UpdateAsync(StudentUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<List<StudentDto>>> SearchAsync(string searchTerm, bool includeInactive = false);

    /// <summary>
    /// Yeni öğrenci için otomatik öğrenci numarası oluşturur
    /// </summary>
    Task<string> GenerateStudentNoAsync();

    /// <summary>
    /// Oluşturulacak bir sonraki öğrenci numarasını önizleme olarak döner
    /// </summary>
    Task<ApiResponse<string>> GetNextStudentNoPreviewAsync();
}
