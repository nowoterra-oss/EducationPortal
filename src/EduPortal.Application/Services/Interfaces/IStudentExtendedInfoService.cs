using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;

namespace EduPortal.Application.Services.Interfaces;

/// <summary>
/// Ogrenci ek bilgileri servisi (Yabanci dil, Hobi, Aktivite, Hazir bulunusluk sinavi)
/// </summary>
public interface IStudentExtendedInfoService
{
    // Foreign Languages
    Task<ApiResponse<List<ForeignLanguageDto>>> GetForeignLanguagesAsync(int studentId);
    Task<ApiResponse<ForeignLanguageDto>> AddForeignLanguageAsync(int studentId, ForeignLanguageCreateDto dto);
    Task<ApiResponse<bool>> DeleteForeignLanguageAsync(int studentId, int id);

    // Hobbies
    Task<ApiResponse<List<HobbyDto>>> GetHobbiesAsync(int studentId);
    Task<ApiResponse<HobbyDto>> AddHobbyAsync(int studentId, HobbyCreateDto dto, string? certificateUrl = null, string? certificateFileName = null);
    Task<ApiResponse<bool>> DeleteHobbyAsync(int studentId, int id);

    // Activities
    Task<ApiResponse<List<ActivityDto>>> GetActivitiesAsync(int studentId);
    Task<ApiResponse<ActivityDto>> AddActivityAsync(int studentId, ActivityCreateDto dto, string? certificateUrl = null, string? certificateFileName = null);
    Task<ApiResponse<bool>> DeleteActivityAsync(int studentId, int id);

    // Readiness Exams
    Task<ApiResponse<List<ReadinessExamDto>>> GetReadinessExamsAsync(int studentId);
    Task<ApiResponse<ReadinessExamDto>> AddReadinessExamAsync(int studentId, ReadinessExamCreateDto dto);
    Task<ApiResponse<bool>> DeleteReadinessExamAsync(int studentId, int id);
}
