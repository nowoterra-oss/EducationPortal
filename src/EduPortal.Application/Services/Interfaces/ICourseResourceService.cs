using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;

namespace EduPortal.Application.Services.Interfaces;

public interface ICourseResourceService
{
    /// <summary>
    /// Tum ders kaynaklarini sayfalanmis olarak getirir
    /// </summary>
    Task<ApiResponse<PagedResponse<CourseResourceDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Belirli bir derse ait kaynaklari getirir
    /// </summary>
    Task<ApiResponse<PagedResponse<CourseResourceDto>>> GetByCourseAsync(int courseId, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// ID'ye gore kaynak getirir
    /// </summary>
    Task<ApiResponse<CourseResourceDto>> GetByIdAsync(int id);

    /// <summary>
    /// Yeni kaynak olusturur
    /// </summary>
    Task<ApiResponse<CourseResourceDto>> CreateAsync(int courseId, CreateCourseResourceDto dto);

    /// <summary>
    /// Kaynak gunceller
    /// </summary>
    Task<ApiResponse<CourseResourceDto>> UpdateAsync(int id, CreateCourseResourceDto dto);

    /// <summary>
    /// Kaynak siler (soft delete)
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int id);

    /// <summary>
    /// Kaynagi indirilmek uzere getirir (dosya bilgisi)
    /// </summary>
    Task<ApiResponse<CourseResourceDto>> GetForDownloadAsync(int id);

    /// <summary>
    /// Dosya bilgileri ile kaynak olusturur
    /// </summary>
    Task<ApiResponse<CourseResourceDto>> CreateWithFileAsync(int courseId, CreateCourseResourceDto dto, string? filePath, string? fileName, long? fileSize, string? mimeType);

    /// <summary>
    /// Download bilgilerini getirir
    /// </summary>
    Task<ApiResponse<CourseResourceDownloadDto>> GetDownloadInfoAsync(int id, string baseUrl);
}
