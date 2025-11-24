using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Services.Interfaces;

public interface IInternationalExamService
{
    Task<ApiResponse<PagedResponse<InternationalExamDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<InternationalExamDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<InternationalExamDto>>> GetByStudentAsync(int studentId);
    Task<ApiResponse<PagedResponse<InternationalExamDto>>> GetByExamTypeAsync(ExamType examType, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<InternationalExamDto>> CreateAsync(InternationalExamCreateDto dto);
    Task<ApiResponse<InternationalExamDto>> UpdateAsync(int id, InternationalExamUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
