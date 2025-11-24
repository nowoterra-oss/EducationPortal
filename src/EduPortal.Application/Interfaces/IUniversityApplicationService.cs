using EduPortal.Application.DTOs.UniversityApplication;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IUniversityApplicationService
{
    Task<(IEnumerable<UniversityApplicationDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<UniversityApplicationDto?> GetByIdAsync(int id);
    Task<UniversityApplicationDto> CreateAsync(CreateUniversityApplicationDto dto);
    Task<UniversityApplicationDto> UpdateAsync(int id, UpdateUniversityApplicationDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<UniversityApplicationDto>> GetByStudentAsync(int studentId);
    Task<(IEnumerable<UniversityApplicationDto> Items, int TotalCount)> GetByStatusAsync(ApplicationStatus status, int pageNumber, int pageSize);
    Task<UniversityApplicationDto> UpdateStatusAsync(int id, ApplicationStatusDto dto);
    Task<ApplicationDocumentResultDto> AddDocumentAsync(int applicationId, AddApplicationDocumentDto dto);
    Task<IEnumerable<ApplicationTimelineDto>> GetTimelineAsync(int applicationId);
    Task<ApplicationStatisticsDto> GetStatisticsAsync();
}
