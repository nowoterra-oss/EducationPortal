using EduPortal.Application.DTOs.Document;

namespace EduPortal.Application.Interfaces;

public interface IApplicationDocumentService
{
    Task<IEnumerable<ApplicationDocumentDto>> GetAllDocumentsAsync();
    Task<IEnumerable<ApplicationDocumentDto>> GetDocumentsByProgramAsync(int programId);
    Task<DocumentChecklistDto> GetDocumentChecklistAsync(int programId);
    Task<ApplicationDocumentDto?> GetDocumentByIdAsync(int id);
    Task<ApplicationDocumentDto> CreateDocumentAsync(CreateApplicationDocumentDto dto);
    Task<ApplicationDocumentDto> UpdateDocumentAsync(int id, UpdateApplicationDocumentDto dto);
    Task<bool> DeleteDocumentAsync(int id);
    Task<DocumentStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<ApplicationDocumentDto>> GetExpiringDocumentsAsync(int days = 30);
}
