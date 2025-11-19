using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;

namespace EduPortal.Application.Services.Interfaces;

public interface IHomeworkService
{
    Task<ApiResponse<PagedResponse<HomeworkDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<HomeworkDto>> GetByIdAsync(int id);
    Task<ApiResponse<HomeworkDto>> CreateAsync(HomeworkCreateDto dto);
    Task<ApiResponse<HomeworkSubmissionDto>> SubmitHomeworkAsync(HomeworkSubmitDto dto);
    Task<ApiResponse<HomeworkSubmissionDto>> EvaluateSubmissionAsync(int submissionId, int score, string? feedback);
    Task<ApiResponse<List<HomeworkSubmissionDto>>> GetHomeworkSubmissionsAsync(int homeworkId);
    Task<ApiResponse<List<HomeworkSubmissionDto>>> GetStudentSubmissionsAsync(int studentId);
}
