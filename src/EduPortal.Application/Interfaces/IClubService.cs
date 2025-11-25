using EduPortal.Application.DTOs.Club;

namespace EduPortal.Application.Interfaces;

public interface IClubService
{
    Task<IEnumerable<ClubDto>> GetAllAsync();
    Task<ClubDto?> GetByIdAsync(int id);
    Task<ClubDto> CreateAsync(CreateClubDto dto);
    Task<ClubDto> UpdateAsync(int id, UpdateClubDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<ClubMemberDto> Items, int TotalCount)> GetMembersAsync(int clubId, int pageNumber, int pageSize);
    Task<IEnumerable<ClubDto>> GetByStudentAsync(int studentId);
    Task<ClubMemberDto> JoinClubAsync(int clubId, int studentId);
    Task<bool> LeaveClubAsync(int clubId, int studentId);
}
