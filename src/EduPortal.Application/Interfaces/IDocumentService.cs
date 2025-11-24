using EduPortal.Application.DTOs.Document;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IDocumentService
{
    Task<(IEnumerable<DocumentDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto);
    Task<DocumentDto> UpdateAsync(int id, UpdateDocumentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<DocumentDto>> GetByStudentAsync(int studentId);
    Task<(IEnumerable<DocumentDto> Items, int TotalCount)> GetByTypeAsync(DocumentType documentType, int pageNumber, int pageSize);
    Task<(byte[] FileContent, string FileName, string ContentType)?> DownloadAsync(int id);
    Task<DocumentShareResultDto> ShareAsync(int documentId, ShareDocumentDto dto);
}
