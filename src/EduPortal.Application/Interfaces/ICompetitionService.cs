using EduPortal.Application.DTOs.Competition;

namespace EduPortal.Application.Interfaces;

public interface ICompetitionService
{
    Task<(IEnumerable<CompetitionDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<CompetitionDto?> GetByIdAsync(int id);
    Task<CompetitionDto> CreateAsync(CreateCompetitionDto dto);
    Task<CompetitionDto> UpdateAsync(int id, UpdateCompetitionDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CompetitionDto>> GetByStudentAsync(int studentId);
}
