using EduPortal.Application.Common;
using EduPortal.Application.DTOs.AGP;

namespace EduPortal.Application.Services.Interfaces;

public interface IAgpPeriodService
{
    /// <summary>
    /// Yeni dönem oluştur
    /// </summary>
    Task<ApiResponse<AgpPeriodResponseDto>> CreateAsync(AgpPeriodCreateDto dto);

    /// <summary>
    /// Dönem güncelle
    /// </summary>
    Task<ApiResponse<AgpPeriodResponseDto>> UpdateAsync(int id, AgpPeriodUpdateDto dto);

    /// <summary>
    /// Dönem sil
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int id);

    /// <summary>
    /// Tek dönem getir
    /// </summary>
    Task<ApiResponse<AgpPeriodResponseDto>> GetByIdAsync(int id);

    /// <summary>
    /// AGP'ye ait tüm dönemleri getir
    /// </summary>
    Task<ApiResponse<List<AgpPeriodResponseDto>>> GetByAgpIdAsync(int agpId);

    /// <summary>
    /// Timeline view için veri getir (Gantt Chart)
    /// </summary>
    Task<ApiResponse<AgpTimelineViewDto>> GetTimelineViewAsync(int agpId, int monthsToShow = 6);

    /// <summary>
    /// Öğrencinin tüm dönemlerini getir (tüm AGP'ler dahil)
    /// </summary>
    Task<ApiResponse<List<AgpPeriodResponseDto>>> GetByStudentIdAsync(int studentId);
}
