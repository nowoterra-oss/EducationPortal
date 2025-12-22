using EduPortal.Application.Common;
using EduPortal.Application.DTOs.SimpleInternship;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.API.Controllers;

/// <summary>
/// Basit staj yonetimi
/// </summary>
[ApiController]
[Route("api/students/{studentId}/simple-internships")]
[Authorize]
public class SimpleInternshipsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SimpleInternshipsController> _logger;
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public SimpleInternshipsController(
        ApplicationDbContext context,
        ILogger<SimpleInternshipsController> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Ogrencinin stajlarini getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SimpleInternshipDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SimpleInternshipDto>>>> GetInternships(int studentId)
    {
        var internships = await _context.SimpleInternships
            .Where(i => i.StudentId == studentId && !i.IsDeleted)
            .OrderByDescending(i => i.StartDate)
            .Select(i => new SimpleInternshipDto
            {
                Id = i.Id,
                StudentId = i.StudentId,
                CompanyName = i.CompanyName,
                Position = i.Position,
                Industry = i.Industry,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                IsOngoing = i.IsOngoing,
                Description = i.Description,
                CertificateUrl = i.CertificateUrl,
                CertificateFileName = i.CertificateFileName,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SimpleInternshipDto>>.SuccessResponse(internships));
    }

    /// <summary>
    /// Yeni staj ekler (JSON veya multipart/form-data)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [Consumes("multipart/form-data", "application/json")]
    [ProducesResponseType(typeof(ApiResponse<SimpleInternshipDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SimpleInternshipDto>>> CreateInternship(
        int studentId,
        [FromForm] CreateSimpleInternshipDto dto)
    {
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId && !s.IsDeleted);
        if (!studentExists)
        {
            return NotFound(ApiResponse<SimpleInternshipDto>.ErrorResponse("Öğrenci bulunamadı"));
        }

        string? certificateUrl = null;
        string? certificateFileName = null;

        // Dosya yükleme işlemi
        if (dto.CertificateFile != null && dto.CertificateFile.Length > 0)
        {
            // Dosya boyutu kontrolü
            if (dto.CertificateFile.Length > MaxFileSize)
            {
                return BadRequest(ApiResponse<SimpleInternshipDto>.ErrorResponse("Dosya boyutu 10 MB'ı aşamaz"));
            }

            // Dosya uzantısı kontrolü
            var extension = Path.GetExtension(dto.CertificateFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<SimpleInternshipDto>.ErrorResponse("Desteklenmeyen dosya formatı. Desteklenen formatlar: PDF, JPG, JPEG, PNG, DOC, DOCX"));
            }

            // Dosya kaydetme
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "files", "internships");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{studentId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.CertificateFile.CopyToAsync(stream);
            }

            certificateUrl = $"/files/internships/{uniqueFileName}";
            certificateFileName = dto.CertificateFile.FileName;
        }

        var internship = new SimpleInternship
        {
            StudentId = studentId,
            CompanyName = dto.CompanyName,
            Position = dto.Position,
            Industry = dto.Industry,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsOngoing = dto.IsOngoing,
            Description = dto.Description,
            CertificateUrl = certificateUrl,
            CertificateFileName = certificateFileName,
            CreatedAt = DateTime.UtcNow
        };

        _context.SimpleInternships.Add(internship);
        await _context.SaveChangesAsync();

        var result = new SimpleInternshipDto
        {
            Id = internship.Id,
            StudentId = internship.StudentId,
            CompanyName = internship.CompanyName,
            Position = internship.Position,
            Industry = internship.Industry,
            StartDate = internship.StartDate,
            EndDate = internship.EndDate,
            IsOngoing = internship.IsOngoing,
            Description = internship.Description,
            CertificateUrl = internship.CertificateUrl,
            CertificateFileName = internship.CertificateFileName,
            CreatedAt = internship.CreatedAt
        };

        return CreatedAtAction(nameof(GetInternships), new { studentId }, ApiResponse<SimpleInternshipDto>.SuccessResponse(result, "Staj başarıyla eklendi"));
    }

    /// <summary>
    /// Staji siler
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteInternship(int studentId, int id)
    {
        var internship = await _context.SimpleInternships
            .FirstOrDefaultAsync(i => i.Id == id && i.StudentId == studentId && !i.IsDeleted);

        if (internship == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Staj bulunamadı"));
        }

        // Dosyayı fiziksel olarak silme (opsiyonel - soft delete ile kalabilir)
        if (!string.IsNullOrEmpty(internship.CertificateUrl))
        {
            var filePath = Path.Combine(_environment.ContentRootPath, internship.CertificateUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        internship.IsDeleted = true;
        internship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Staj başarıyla silindi"));
    }
}
