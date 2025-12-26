using EduPortal.Application.Common;
using EduPortal.Application.DTOs.CounselorDashboard;

namespace EduPortal.Application.Interfaces;

public interface ICounselorDashboardService
{
    // Ogrenci Tam Profil
    Task<ApiResponse<StudentFullProfileDto>> GetStudentFullProfileAsync(int studentId);

    // Akademik Performans
    Task<ApiResponse<StudentAcademicPerformanceDto>> GetStudentAcademicPerformanceAsync(int studentId);

    // Yurtdisi Egitim Bilgileri
    Task<ApiResponse<StudentInternationalEducationDto>> GetStudentInternationalEducationAsync(int studentId);

    // Aktiviteler ve Oduller
    Task<ApiResponse<StudentActivitiesAndAwardsDto>> GetStudentActivitiesAndAwardsAsync(int studentId);

    // Universite Basvuru Takibi
    Task<ApiResponse<StudentUniversityTrackingDto>> GetStudentUniversityTrackingAsync(int studentId);

    // Sinav Takvimi
    Task<ApiResponse<List<ExamCalendarItemDto>>> GetStudentExamCalendarAsync(int studentId);

    // Gorusme Notlari
    Task<ApiResponse<List<CounselorNoteDto>>> GetStudentCounselorNotesAsync(int studentId);
    Task<ApiResponse<CounselorNoteDto>> CreateCounselorNoteAsync(int counselorId, CreateCounselorNoteDto dto);
    Task<ApiResponse<CounselorNoteDto>> UpdateCounselorNoteAsync(int counselorId, UpdateCounselorNoteDto dto);
    Task<ApiResponse<bool>> DeleteCounselorNoteAsync(int noteId);

    // Deadline Kontrol (7 gun icindeki deadline'lar)
    Task<ApiResponse<List<UpcomingDeadlineDto>>> GetUpcomingDeadlinesAsync(int studentId);

    // Dashboard Ozet
    Task<ApiResponse<CounselorDashboardSummaryDto>> GetDashboardSummaryAsync(int studentId);

    // Danismanin tum ogrencileri
    Task<ApiResponse<List<CounselorDashboardSummaryDto>>> GetCounselorStudentsSummaryAsync(int counselorId);
}
