using EduPortal.Application.DTOs.Visa;

namespace EduPortal.Application.Interfaces;

public interface IVisaProcessService
{
    Task<IEnumerable<VisaProcessDto>> GetAllVisaProcessesAsync();
    Task<IEnumerable<VisaProcessDto>> GetActiveVisaProcessesAsync();
    Task<IEnumerable<VisaProcessDto>> GetVisaProcessesByProgramAsync(int programId);
    Task<IEnumerable<VisaProcessDto>> GetPendingVisaProcessesAsync();
    Task<IEnumerable<VisaProcessDto>> GetExpiringVisasAsync(int days = 90);
    Task<VisaProcessDto?> GetVisaProcessByIdAsync(int id);
    Task<VisaTimelineDto> GetVisaTimelineAsync(int id);
    Task<VisaProcessDto> CreateVisaProcessAsync(CreateVisaProcessDto dto);
    Task<VisaProcessDto> UpdateVisaProcessAsync(int id, UpdateVisaProcessDto dto);
    Task<bool> DeleteVisaProcessAsync(int id);
    Task<VisaStatisticsDto> GetStatisticsAsync();
}
