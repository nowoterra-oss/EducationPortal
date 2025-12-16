using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Certificate;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduPortal.Infrastructure.Services;

public class StudentCertificateService : IStudentCertificateService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _certificatesPath;
    private readonly int _maxFileSizeMB;
    private readonly string[] _allowedExtensions;

    public StudentCertificateService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _certificatesPath = _configuration["FileStorage:CertificatesPath"] ?? "uploads/certificates";
        _maxFileSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 5);
        _allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
            ?? new[] { ".pdf", ".jpg", ".jpeg", ".png" };
    }

    public async Task<ApiResponse<List<StudentCertificateDto>>> GetByStudentIdAsync(int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student == null)
            return ApiResponse<List<StudentCertificateDto>>.ErrorResponse("Öğrenci bulunamadı.");

        var certificates = await _context.StudentCertificates
            .Where(c => c.StudentId == studentId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new StudentCertificateDto
            {
                Id = c.Id,
                StudentId = c.StudentId,
                Name = c.Name,
                Description = c.Description,
                FileName = c.FileName,
                FileType = c.FileType,
                FileSize = c.FileSize,
                UploadDate = c.CreatedAt,
                IssueDate = c.IssueDate,
                IssuingOrganization = c.IssuingOrganization,
                IsAddedByAdmin = c.IsAddedByAdmin
            })
            .ToListAsync();

        return ApiResponse<List<StudentCertificateDto>>.SuccessResponse(certificates);
    }

    public async Task<ApiResponse<StudentCertificateDto>> GetByIdAsync(int studentId, int certificateId)
    {
        var certificate = await _context.StudentCertificates
            .FirstOrDefaultAsync(c => c.Id == certificateId && c.StudentId == studentId && !c.IsDeleted);

        if (certificate == null)
            return ApiResponse<StudentCertificateDto>.ErrorResponse("Sertifika bulunamadı.");

        var resultDto = new StudentCertificateDto
        {
            Id = certificate.Id,
            StudentId = certificate.StudentId,
            Name = certificate.Name,
            Description = certificate.Description,
            FileName = certificate.FileName,
            FileType = certificate.FileType,
            FileSize = certificate.FileSize,
            UploadDate = certificate.CreatedAt,
            IssueDate = certificate.IssueDate,
            IssuingOrganization = certificate.IssuingOrganization,
            IsAddedByAdmin = certificate.IsAddedByAdmin
        };

        return ApiResponse<StudentCertificateDto>.SuccessResponse(resultDto);
    }

    public async Task<ApiResponse<StudentCertificateUploadResultDto>> UploadAsync(int studentId, IFormFile file, StudentCertificateCreateDto dto, bool isAddedByAdmin)
    {
        // Validate student exists
        var student = await _context.Students.FindAsync(studentId);
        if (student == null)
            return ApiResponse<StudentCertificateUploadResultDto>.ErrorResponse("Öğrenci bulunamadı.");

        // Validate file
        if (file == null || file.Length == 0)
            return ApiResponse<StudentCertificateUploadResultDto>.ErrorResponse("Dosya seçilmedi.");

        // Check file size
        var maxBytes = _maxFileSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
            return ApiResponse<StudentCertificateUploadResultDto>.ErrorResponse($"Dosya boyutu {_maxFileSizeMB}MB'dan büyük olamaz.");

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return ApiResponse<StudentCertificateUploadResultDto>.ErrorResponse($"İzin verilen dosya türleri: {string.Join(", ", _allowedExtensions)}");

        // Create directory if not exists
        var studentCertificatesPath = Path.Combine(_certificatesPath, studentId.ToString());
        if (!Directory.Exists(studentCertificatesPath))
            Directory.CreateDirectory(studentCertificatesPath);

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(studentCertificatesPath, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save to database
        var certificate = new StudentCertificate
        {
            StudentId = studentId,
            Name = dto.Name,
            Description = dto.Description,
            FilePath = filePath,
            FileName = file.FileName,
            FileType = extension.TrimStart('.'),
            FileSize = file.Length,
            IssueDate = dto.IssueDate,
            IssuingOrganization = dto.IssuingOrganization,
            IsAddedByAdmin = isAddedByAdmin
        };

        _context.StudentCertificates.Add(certificate);
        await _context.SaveChangesAsync();

        var resultDto = new StudentCertificateUploadResultDto
        {
            Id = certificate.Id,
            Name = certificate.Name,
            FileName = certificate.FileName
        };

        return ApiResponse<StudentCertificateUploadResultDto>.SuccessResponse(resultDto, "Sertifika başarıyla yüklendi.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int studentId, int certificateId)
    {
        var certificate = await _context.StudentCertificates
            .FirstOrDefaultAsync(c => c.Id == certificateId && c.StudentId == studentId && !c.IsDeleted);

        if (certificate == null)
            return ApiResponse<bool>.ErrorResponse("Sertifika bulunamadı.");

        // Delete file from disk
        if (File.Exists(certificate.FilePath))
        {
            try
            {
                File.Delete(certificate.FilePath);
            }
            catch
            {
                // Log error but continue with database deletion
            }
        }

        // Soft delete from database
        certificate.IsDeleted = true;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Sertifika başarıyla silindi.");
    }

    public async Task<(byte[]? fileBytes, string? contentType, string? fileName)> DownloadAsync(int studentId, int certificateId)
    {
        var certificate = await _context.StudentCertificates
            .FirstOrDefaultAsync(c => c.Id == certificateId && c.StudentId == studentId && !c.IsDeleted);

        if (certificate == null)
            return (null, null, null);

        if (!File.Exists(certificate.FilePath))
            return (null, null, null);

        var fileBytes = await File.ReadAllBytesAsync(certificate.FilePath);
        var contentType = GetContentType(certificate.FileType);

        return (fileBytes, contentType, certificate.FileName);
    }

    private string GetContentType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
