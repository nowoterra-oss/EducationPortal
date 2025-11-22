using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;

namespace EduPortal.Application.Services.Interfaces;

public interface IInternalExamService
{
    Task<ApiResponse<PagedResponse<InternalExamDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<InternalExamDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<InternalExamDto>>> GetByCourseAsync(int courseId);
    Task<ApiResponse<List<InternalExamDto>>> GetByStudentAsync(int studentId);
    Task<ApiResponse<InternalExamDto>> CreateAsync(InternalExamCreateDto dto, int teacherId);
    Task<ApiResponse<InternalExamDto>> UpdateAsync(InternalExamUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);

    // Result operations
    Task<ApiResponse<PagedResponse<ExamResultDto>>> GetResultsAsync(int examId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<ExamResultDto>> AddResultAsync(ExamResultCreateDto dto);
    Task<ApiResponse<ExamStatisticsDto>> GetStatisticsAsync(int examId);
}
