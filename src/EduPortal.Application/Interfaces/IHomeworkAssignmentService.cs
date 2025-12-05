using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;

namespace EduPortal.Application.Interfaces;

public interface IHomeworkAssignmentService
{
    // Öğretmenin öğrencilerini getir
    Task<ApiResponse<List<StudentSummaryDto>>> GetTeacherStudentsAsync(int teacherId);

    // Ödev Atama
    Task<ApiResponse<HomeworkAssignmentDto>> CreateAssignmentAsync(int teacherId, CreateHomeworkAssignmentDto dto);
    Task<ApiResponse<List<HomeworkAssignmentDto>>> CreateBulkAssignmentsAsync(int teacherId, BulkCreateHomeworkAssignmentDto dto);

    // Ödev Listesi
    Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetTeacherAssignmentsAsync(int teacherId, int pageNumber, int pageSize);
    Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetStudentAssignmentsAsync(int studentId, int pageNumber, int pageSize);
    Task<ApiResponse<List<HomeworkAssignmentDto>>> GetStudentAssignmentHistoryAsync(int studentId, int teacherId);

    // Ödev Görüntüleme Logu
    Task<ApiResponse<bool>> MarkAsViewedAsync(int assignmentId, int studentId, string? ipAddress, string? userAgent);

    // Ödev Kontrol (Teslim Edilenleri Görüntüle)
    Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetPendingReviewsAsync(int teacherId, int pageNumber, int pageSize);
    Task<ApiResponse<HomeworkAssignmentDto>> GetAssignmentDetailAsync(int assignmentId);

    // Ödev Değerlendirme
    Task<ApiResponse<HomeworkAssignmentDto>> GradeAssignmentAsync(int teacherId, GradeHomeworkDto dto);

    // Ödev Teslimi
    Task<ApiResponse<HomeworkAssignmentDto>> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitHomeworkDto dto);
    Task<ApiResponse<FileUploadResultDto>> UploadSubmissionFileAsync(int assignmentId, int studentId, Stream fileStream, string fileName, string contentType);

    // Performans Analizi
    Task<ApiResponse<StudentHomeworkPerformanceDto>> GetStudentPerformanceAsync(int studentId, DateTime? startDate, DateTime? endDate);
    Task<ApiResponse<HomeworkPerformanceChartDto>> GetPerformanceChartDataAsync(int studentId, int months = 6);

    // Hatırlatma
    Task<ApiResponse<int>> SendDueDateRemindersAsync(); // Bitiş tarihine 1 gün kala

    // Takvime Ekleme
    Task<ApiResponse<bool>> AddToStudentCalendarAsync(int assignmentId);
}
