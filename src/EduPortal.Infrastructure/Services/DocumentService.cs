using EduPortal.Application.DTOs.Document;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<DocumentDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.StudentDocuments
            .Include(d => d.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDto(d))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var document = await _context.StudentDocuments
            .Include(d => d.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return document == null ? null : MapToDto(document);
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto)
    {
        var document = new StudentDocument
        {
            StudentId = dto.StudentId,
            DocumentType = dto.DocumentType,
            Title = dto.Title,
            AcademicYear = dto.AcademicYear,
            DocumentUrl = dto.DocumentUrl,
            Status = dto.Status,
            ReviewNotes = dto.ReviewNotes
        };

        _context.StudentDocuments.Add(document);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(document.Id) ?? throw new InvalidOperationException("Belge oluşturulamadı");
    }

    public async Task<DocumentDto> UpdateAsync(int id, UpdateDocumentDto dto)
    {
        var document = await _context.StudentDocuments.FindAsync(id);

        if (document == null)
            throw new KeyNotFoundException("Belge bulunamadı");

        document.DocumentType = dto.DocumentType;
        document.Title = dto.Title;
        document.AcademicYear = dto.AcademicYear;
        if (!string.IsNullOrEmpty(dto.DocumentUrl))
            document.DocumentUrl = dto.DocumentUrl;
        document.Status = dto.Status;
        document.ReviewNotes = dto.ReviewNotes;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Belge güncellenemedi");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await _context.StudentDocuments.FindAsync(id);

        if (document == null)
            return false;

        _context.StudentDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<DocumentDto>> GetByStudentAsync(int studentId)
    {
        var documents = await _context.StudentDocuments
            .Include(d => d.Student)
                .ThenInclude(s => s.User)
            .Where(d => d.StudentId == studentId)
            .OrderByDescending(d => d.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<(IEnumerable<DocumentDto> Items, int TotalCount)> GetByTypeAsync(DocumentType documentType, int pageNumber, int pageSize)
    {
        var query = _context.StudentDocuments
            .Include(d => d.Student)
                .ThenInclude(s => s.User)
            .Where(d => d.DocumentType == documentType)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDto(d))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(byte[] FileContent, string FileName, string ContentType)?> DownloadAsync(int id)
    {
        var document = await _context.StudentDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return null;

        // In a real implementation, this would fetch the file from storage (Azure Blob, S3, etc.)
        // For now, we return a placeholder indicating the file URL
        var fileName = $"{document.Title}_{document.Id}";
        var extension = GetFileExtension(document.DocumentUrl);
        var contentType = GetContentType(extension);

        // Placeholder - in production, fetch actual file bytes from storage
        var placeholderContent = System.Text.Encoding.UTF8.GetBytes($"Document URL: {document.DocumentUrl}");

        return (placeholderContent, $"{fileName}{extension}", contentType);
    }

    public async Task<DocumentShareResultDto> ShareAsync(int documentId, ShareDocumentDto dto)
    {
        var document = await _context.StudentDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            throw new KeyNotFoundException("Belge bulunamadı");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        // In a real implementation, this would create a share record in a DocumentShare table
        // and generate a unique share URL. For now, we return a simulated result.
        var shareToken = Guid.NewGuid().ToString("N")[..8];

        return new DocumentShareResultDto
        {
            DocumentId = documentId,
            SharedWithUserId = dto.UserId,
            SharedWithUserName = $"{user.FirstName} {user.LastName}",
            ShareUrl = $"/api/documents/shared/{shareToken}",
            SharedAt = DateTime.UtcNow,
            ExpiresAt = dto.ExpiresAt,
            CanEdit = dto.CanEdit
        };
    }

    private static DocumentDto MapToDto(StudentDocument document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            StudentId = document.StudentId,
            StudentName = document.Student?.User != null
                ? $"{document.Student.User.FirstName} {document.Student.User.LastName}"
                : string.Empty,
            DocumentType = document.DocumentType,
            Title = document.Title,
            AcademicYear = document.AcademicYear,
            DocumentUrl = document.DocumentUrl,
            Status = document.Status,
            ReviewNotes = document.ReviewNotes,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static string GetFileExtension(string url)
    {
        if (string.IsNullOrEmpty(url))
            return ".pdf";

        var extension = Path.GetExtension(url);
        return string.IsNullOrEmpty(extension) ? ".pdf" : extension;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
