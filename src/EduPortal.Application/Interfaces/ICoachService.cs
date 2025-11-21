using EduPortal.Application.DTOs.Coach;

namespace EduPortal.Application.Interfaces;

public interface ICoachService
{
    Task<IEnumerable<CoachDto>> GetAllCoachesAsync();
    Task<IEnumerable<CoachSummaryDto>> GetAvailableCoachesAsync();
    Task<CoachDto?> GetCoachByIdAsync(int id);
    Task<CoachDto> CreateCoachAsync(CreateCoachDto dto);
    Task<CoachDto> UpdateCoachAsync(int id, UpdateCoachDto dto);
    Task<bool> DeleteCoachAsync(int id);
    Task<CoachPerformanceDto> GetCoachPerformanceAsync(int id, DateTime startDate, DateTime endDate);
    Task<IEnumerable<CoachDto>> GetCoachesByBranchAsync(int branchId);
}
