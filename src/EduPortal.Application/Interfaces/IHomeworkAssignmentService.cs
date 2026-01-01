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
    Task<ApiResponse<List<HomeworkAssignmentDto>>> GetStudentAssignmentHistoryAsync(int studentId, int? teacherId = null);

    // Ödev Görüntüleme Logu
    Task<ApiResponse<bool>> MarkAsViewedAsync(int assignmentId, int studentId, string? ipAddress, string? userAgent);

    // Ödev Kontrol (Teslim Edilenleri Görüntüle)
    /// <summary>
    /// Kontrol bekleyen ödevleri getirir.
    /// teacherId null ise tüm öğretmenlerin ödevleri döner (Admin için).
    /// status null ise TeslimEdildi durumundakiler döner, "all" ise tüm aktif ödevler döner.
    /// </summary>
    Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetPendingReviewsAsync(int? teacherId, int pageNumber, int pageSize, string? status = null);
    Task<ApiResponse<HomeworkAssignmentDto>> GetAssignmentDetailAsync(int assignmentId);

    // Ödev Değerlendirme
    /// <summary>
    /// Ödevi değerlendirir.
    /// isAdmin true ise teacherId kontrolü yapılmaz (Admin tüm ödevleri değerlendirebilir).
    /// </summary>
    Task<ApiResponse<HomeworkAssignmentDto>> GradeAssignmentAsync(int? teacherId, GradeHomeworkDto dto, bool isAdmin = false);

    // Ödev Teslimi
    Task<ApiResponse<HomeworkAssignmentDto>> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitHomeworkDto dto);
    Task<ApiResponse<FileUploadResultDto>> UploadSubmissionFileAsync(int assignmentId, int studentId, Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Assignment'tan bağımsız dosya yükleme (frontend akışı için).
    /// Öğrenci önce dosya yükler, dönen URL ile submit çağırır.
    /// </summary>
    Task<ApiResponse<FileUploadResultDto>> UploadFileAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Test teslimi. Sadece TeslimEdildi durumundaki ödevler için test teslim edilebilir.
    /// </summary>
    Task<ApiResponse<HomeworkAssignmentDto>> SubmitTestAsync(int assignmentId, int studentId, SubmitTestDto dto);

    // Performans Analizi
    Task<ApiResponse<StudentHomeworkPerformanceDto>> GetStudentPerformanceAsync(int studentId, DateTime? startDate, DateTime? endDate);
    Task<ApiResponse<HomeworkPerformanceChartDto>> GetPerformanceChartDataAsync(int studentId, int months = 6);

    // Hatırlatma
    Task<ApiResponse<int>> SendDueDateRemindersAsync(); // Bitiş tarihine 1 gün kala

    // Takvime Ekleme
    Task<ApiResponse<bool>> AddToStudentCalendarAsync(int assignmentId);
}
