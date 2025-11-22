using EduPortal.Application.DTOs.Document;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ApplicationDocumentService : IApplicationDocumentService
{
    private readonly ApplicationDbContext _context;

    public ApplicationDocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApplicationDocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _context.ApplicationDocuments
            .Include(d => d.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<ApplicationDocumentDto>> GetDocumentsByProgramAsync(int programId)
    {
        var documents = await _context.ApplicationDocuments
            .Include(d => d.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(d => d.ProgramId == programId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<DocumentChecklistDto> GetDocumentChecklistAsync(int programId)
    {
        var program = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted);

        if (program == null)
            throw new Exception("Program not found");

        var documents = program.Documents.Where(d => !d.IsDeleted).ToList();
        var totalDocs = documents.Count;
        var completedDocs = documents.Count(d => d.Status == DocumentStatus.Approved);
        var pendingDocs = documents.Count(d => d.Status == DocumentStatus.NotStarted || d.Status == DocumentStatus.InProgress);

        return new DocumentChecklistDto
        {
            ProgramId = programId,
            StudentName = $"{program.Student.User.FirstName} {program.Student.User.LastName}",
            TargetUniversity = program.TargetUniversity,
            Documents = documents.Select(d => new DocumentChecklistItemDto
            {
                Id = d.Id,
                Name = d.DocumentName,
                Type = d.DocumentType.ToString(),
                Status = d.Status.ToString(),
                IsCompleted = d.Status == DocumentStatus.Approved,
                SubmissionDate = d.SubmissionDate,
                ExpiryDate = d.ExpiryDate
            }).ToList(),
            TotalDocuments = totalDocs,
            CompletedDocuments = completedDocs,
            PendingDocuments = pendingDocs,
            ProgressPercentage = totalDocs > 0 ? (completedDocs * 100) / totalDocs : 0
        };
    }

    public async Task<ApplicationDocumentDto?> GetDocumentByIdAsync(int id)
    {
        var document = await _context.ApplicationDocuments
            .Include(d => d.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return document != null ? MapToDto(document) : null;
    }

    public async Task<ApplicationDocumentDto> CreateDocumentAsync(CreateApplicationDocumentDto dto)
    {
        var document = new ApplicationDocument
        {
            ProgramId = dto.ProgramId,
            DocumentName = dto.DocumentName,
            DocumentType = (DocumentType)dto.DocumentType,
            DocumentUrl = dto.DocumentUrl,
            Status = DocumentStatus.NotStarted,
            SubmissionDate = dto.SubmissionDate,
            ExpiryDate = dto.ExpiryDate,
            Notes = dto.Notes
        };

        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        return (await GetDocumentByIdAsync(document.Id))!;
    }

    public async Task<ApplicationDocumentDto> UpdateDocumentAsync(int id, UpdateApplicationDocumentDto dto)
    {
        var document = await _context.ApplicationDocuments.FindAsync(id);
        if (document == null || document.IsDeleted)
            throw new Exception("Document not found");

        document.DocumentName = dto.DocumentName;
        document.Status = (DocumentStatus)dto.Status;
        document.DocumentUrl = dto.DocumentUrl ?? document.DocumentUrl;
        document.SubmissionDate = dto.SubmissionDate;
        document.ExpiryDate = dto.ExpiryDate;
        document.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return (await GetDocumentByIdAsync(id))!;
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        var document = await _context.ApplicationDocuments.FindAsync(id);
        if (document == null || document.IsDeleted)
            return false;

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<DocumentStatisticsDto> GetStatisticsAsync()
    {
        var documents = await _context.ApplicationDocuments
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var stats = new DocumentStatisticsDto
        {
            TotalDocuments = documents.Count,
            CompletedDocuments = documents.Count(d => d.Status == DocumentStatus.Approved),
            PendingDocuments = documents.Count(d => d.Status == DocumentStatus.NotStarted || d.Status == DocumentStatus.InProgress),
            ExpiringDocuments = documents.Count(d => d.ExpiryDate.HasValue &&
                                                     d.ExpiryDate.Value > now &&
                                                     d.ExpiryDate.Value <= now.AddDays(30)),
            ExpiredDocuments = documents.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now)
        };

        stats.DocumentsByType = documents
            .GroupBy(d => d.DocumentType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.DocumentsByStatus = documents
            .GroupBy(d => d.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    public async Task<IEnumerable<ApplicationDocumentDto>> GetExpiringDocumentsAsync(int days = 30)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddDays(days);

        var documents = await _context.ApplicationDocuments
            .Include(d => d.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(d => !d.IsDeleted &&
                       d.ExpiryDate.HasValue &&
                       d.ExpiryDate.Value > now &&
                       d.ExpiryDate.Value <= futureDate)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    private ApplicationDocumentDto MapToDto(ApplicationDocument document)
    {
        var now = DateTime.UtcNow;
        var isExpired = document.ExpiryDate.HasValue && document.ExpiryDate.Value < now;
        var daysUntilExpiry = document.ExpiryDate.HasValue
            ? (int)(document.ExpiryDate.Value - now).TotalDays
            : 0;

        return new ApplicationDocumentDto
        {
            Id = document.Id,
            ProgramId = document.ProgramId,
            StudentName = $"{document.Program.Student.User.FirstName} {document.Program.Student.User.LastName}",
            TargetUniversity = document.Program.TargetUniversity,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType.ToString(),
            DocumentUrl = document.DocumentUrl,
            Status = document.Status.ToString(),
            SubmissionDate = document.SubmissionDate,
            ExpiryDate = document.ExpiryDate,
            Notes = document.Notes,
            IsExpired = isExpired,
            DaysUntilExpiry = daysUntilExpiry,
            CreatedDate = document.CreatedAt
        };
    }
}
