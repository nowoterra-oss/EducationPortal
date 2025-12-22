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
[Produces("application/json")]
[Authorize]
public class SimpleInternshipsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SimpleInternshipsController> _logger;

    public SimpleInternshipsController(
        ApplicationDbContext context,
        ILogger<SimpleInternshipsController> logger)
    {
        _context = context;
        _logger = logger;
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
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Description = i.Description,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SimpleInternshipDto>>.SuccessResponse(internships));
    }

    /// <summary>
    /// Yeni staj ekler
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<SimpleInternshipDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SimpleInternshipDto>>> CreateInternship(
        int studentId,
        [FromBody] CreateSimpleInternshipDto dto)
    {
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId && !s.IsDeleted);
        if (!studentExists)
        {
            return NotFound(ApiResponse<SimpleInternshipDto>.ErrorResponse("Öğrenci bulunamadı"));
        }

        var internship = new SimpleInternship
        {
            StudentId = studentId,
            CompanyName = dto.CompanyName,
            Position = dto.Position,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
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
            StartDate = internship.StartDate,
            EndDate = internship.EndDate,
            Description = internship.Description,
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

        internship.IsDeleted = true;
        internship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Staj başarıyla silindi"));
    }
}
