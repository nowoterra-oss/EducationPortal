using EduPortal.Application.DTOs.Branch;

namespace EduPortal.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllBranchesAsync();
    Task<BranchDto?> GetBranchByIdAsync(int id);
    Task<BranchDto> CreateBranchAsync(CreateBranchDto dto);
    Task<BranchDto> UpdateBranchAsync(int id, UpdateBranchDto dto);
    Task<bool> DeleteBranchAsync(int id);
    Task<BranchStatisticsDto> GetBranchStatisticsAsync(int id);
    Task<IEnumerable<BranchStatisticsDto>> GetAllBranchesPerformanceAsync();
    Task<bool> TransferStudentAsync(TransferStudentDto dto);
}
