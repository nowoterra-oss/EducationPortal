using EduPortal.Application.DTOs.Counselor;

namespace EduPortal.Application.Interfaces;

public interface ICounselorService
{
    Task<IEnumerable<CounselorDto>> GetAllCounselorsAsync();
    Task<(IEnumerable<CounselorSummaryDto> Items, int TotalCount)> GetCounselorsPagedAsync(int pageNumber, int pageSize);
    Task<CounselorDto?> GetCounselorByIdAsync(int id);
    Task<CounselorDto?> GetCounselorByUserIdAsync(string userId);
    Task<CounselorDto> CreateCounselorAsync(CreateCounselorDto dto);
    Task<CounselorDto> UpdateCounselorAsync(int id, UpdateCounselorDto dto);
    Task<bool> DeleteCounselorAsync(int id);

    // Student assignments
    Task<IEnumerable<CounselorStudentDto>> GetCounselorStudentsAsync(int counselorId);
    Task<bool> AssignStudentAsync(int counselorId, int studentId);
    Task<bool> UnassignStudentAsync(int counselorId, int studentId);

    // Counseling sessions/meetings
    Task<(IEnumerable<CounselingSessionDto> Items, int TotalCount)> GetCounselorSessionsPagedAsync(int counselorId, int pageNumber, int pageSize);
    Task<IEnumerable<CounselorDto>> GetActiveCounselorsAsync();
}
