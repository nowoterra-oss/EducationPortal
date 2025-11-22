using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;

namespace EduPortal.Application.Services.Interfaces;

public interface IHomeworkService
{
    Task<ApiResponse<PagedResponse<HomeworkDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<HomeworkDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<HomeworkDto>>> GetByCourseAsync(int courseId);
    Task<ApiResponse<List<HomeworkDto>>> GetByStudentAsync(int studentId);
    Task<ApiResponse<HomeworkDto>> CreateAsync(HomeworkCreateDto dto, int teacherId);
    Task<ApiResponse<HomeworkDto>> UpdateAsync(HomeworkUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);

    // Submission operations
    Task<ApiResponse<PagedResponse<HomeworkSubmissionDto>>> GetSubmissionsAsync(int homeworkId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<HomeworkSubmissionDto>> SubmitHomeworkAsync(HomeworkSubmitDto dto);
    Task<ApiResponse<HomeworkSubmissionDto>> GradeSubmissionAsync(GradeSubmissionDto dto);
    Task<ApiResponse<List<HomeworkSubmissionDto>>> GetStudentSubmissionsAsync(int studentId);
}
