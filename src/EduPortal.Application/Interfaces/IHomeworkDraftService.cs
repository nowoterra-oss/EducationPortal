using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;

namespace EduPortal.Application.Interfaces;

/// <summary>
/// Taslak ödev yönetimi servisi
/// </summary>
public interface IHomeworkDraftService
{
    /// <summary>
    /// Öğretmenin tüm taslaklarını getir
    /// </summary>
    Task<ApiResponse<List<HomeworkDraftDto>>> GetDraftsByTeacherAsync(int teacherId, bool? isSent = null);

    /// <summary>
    /// Belirli bir taslağı getir
    /// </summary>
    Task<ApiResponse<HomeworkDraftDto>> GetDraftByIdAsync(int draftId);

    /// <summary>
    /// LessonId ile taslak getir
    /// </summary>
    Task<ApiResponse<HomeworkDraftDto>> GetDraftByLessonIdAsync(int teacherId, string lessonId);

    /// <summary>
    /// Taslak oluştur veya güncelle (upsert - lessonId + teacherId unique)
    /// </summary>
    Task<ApiResponse<HomeworkDraftDto>> CreateOrUpdateDraftAsync(int teacherId, CreateHomeworkDraftDto dto);

    /// <summary>
    /// Taslağı güncelle (partial update)
    /// </summary>
    Task<ApiResponse<HomeworkDraftDto>> UpdateDraftAsync(int draftId, UpdateHomeworkDraftDto dto);

    /// <summary>
    /// Taslağa ders içeriği dosyası yükle
    /// </summary>
    Task<ApiResponse<DraftFileDto>> UploadContentFileAsync(int draftId, Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Taslağa test dosyası yükle
    /// </summary>
    Task<ApiResponse<DraftFileDto>> UploadTestFileAsync(int draftId, Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Taslaktan dosya sil
    /// </summary>
    Task<ApiResponse<bool>> RemoveFileAsync(int draftId, string downloadUrl, string fileType);

    /// <summary>
    /// Taslağı öğrencilere gönder (HomeworkAssignment'lar oluşturulur)
    /// </summary>
    Task<ApiResponse<SendDraftResultDto>> SendDraftAsync(int draftId);

    /// <summary>
    /// Taslağı sil
    /// </summary>
    Task<ApiResponse<bool>> DeleteDraftAsync(int draftId);
}
