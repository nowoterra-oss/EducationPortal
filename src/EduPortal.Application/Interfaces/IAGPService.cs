using EduPortal.Application.DTOs.AGP;

namespace EduPortal.Application.Interfaces;

public interface IAGPService
{
    Task<(IEnumerable<AGPDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<AGPDto?> GetByIdAsync(int id);
    Task<AGPDto> CreateAsync(CreateAGPDto dto);
    Task<AGPDto> UpdateAsync(int id, UpdateAGPDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AGPDto>> GetByStudentAsync(int studentId);

    // Goal/Milestone operations
    Task<IEnumerable<AGPGoalDto>> GetGoalsAsync(int agpId);
    Task<AGPGoalDto> AddGoalAsync(int agpId, CreateAGPGoalDto dto);
    Task<AGPGoalDto> UpdateGoalAsync(int agpId, int goalId, UpdateAGPGoalDto dto);
    Task<bool> DeleteGoalAsync(int agpId, int goalId);

    // Progress
    Task<AGPProgressDto> GetProgressAsync(int agpId);
}
