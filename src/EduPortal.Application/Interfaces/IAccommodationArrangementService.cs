using EduPortal.Application.DTOs.Accommodation;

namespace EduPortal.Application.Interfaces;

public interface IAccommodationArrangementService
{
    Task<IEnumerable<AccommodationArrangementDto>> GetAllArrangementsAsync();
    Task<IEnumerable<AccommodationArrangementDto>> GetActiveArrangementsAsync();
    Task<IEnumerable<AccommodationArrangementDto>> GetArrangementsByProgramAsync(int programId);
    Task<IEnumerable<AccommodationSummaryDto>> GetArrangementSummariesAsync();
    Task<AccommodationArrangementDto?> GetArrangementByIdAsync(int id);
    Task<AccommodationArrangementDto> CreateArrangementAsync(CreateAccommodationArrangementDto dto);
    Task<AccommodationArrangementDto> UpdateArrangementAsync(int id, UpdateAccommodationArrangementDto dto);
    Task<bool> DeleteArrangementAsync(int id);
    Task<AccommodationStatisticsDto> GetStatisticsAsync();
}
