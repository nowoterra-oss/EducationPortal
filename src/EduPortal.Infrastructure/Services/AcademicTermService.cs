using EduPortal.Application.DTOs.AcademicTerm;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class AcademicTermService : IAcademicTermService
{
    private readonly ApplicationDbContext _context;

    public AcademicTermService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<AcademicTermDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.AcademicTerms.AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.StartDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<AcademicTermDto?> GetByIdAsync(int id)
    {
        var term = await _context.AcademicTerms
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return term == null ? null : MapToDto(term);
    }

    public async Task<AcademicTermDto> CreateAsync(CreateAcademicTermDto dto)
    {
        var term = new AcademicTerm
        {
            TermName = dto.TermName,
            AcademicYear = dto.AcademicYear,
            TermNumber = dto.TermNumber,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MidtermStartDate = dto.MidtermStartDate,
            MidtermEndDate = dto.MidtermEndDate,
            FinalStartDate = dto.FinalStartDate,
            FinalEndDate = dto.FinalEndDate,
            IsActive = dto.IsActive,
            IsCurrent = dto.IsCurrent,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        // If this is set as current, deactivate other current terms
        if (term.IsCurrent)
        {
            await DeactivateOtherCurrentTermsAsync();
        }

        _context.AcademicTerms.Add(term);
        await _context.SaveChangesAsync();

        return MapToDto(term);
    }

    public async Task<AcademicTermDto> UpdateAsync(int id, UpdateAcademicTermDto dto)
    {
        var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);

        if (term == null)
            throw new KeyNotFoundException("Akademik dönem bulunamadı");

        term.TermName = dto.TermName;
        term.AcademicYear = dto.AcademicYear;
        term.TermNumber = dto.TermNumber;
        term.StartDate = dto.StartDate;
        term.EndDate = dto.EndDate;
        term.MidtermStartDate = dto.MidtermStartDate;
        term.MidtermEndDate = dto.MidtermEndDate;
        term.FinalStartDate = dto.FinalStartDate;
        term.FinalEndDate = dto.FinalEndDate;
        term.IsActive = dto.IsActive;
        term.Description = dto.Description;
        term.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(term);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var term = await _context.AcademicTerms.FindAsync(id);
        if (term == null)
            return false;

        // Check if term has related data
        var hasRelatedData = await _context.StudentClassAssignments.AnyAsync(s => s.AcademicTermId == id)
            || await _context.WeeklySchedules.AnyAsync(w => w.AcademicTermId == id);

        if (hasRelatedData)
            throw new InvalidOperationException("Bu dönem kullanımda olduğu için silinemez");

        _context.AcademicTerms.Remove(term);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AcademicTermDto?> GetCurrentAsync()
    {
        var term = await _context.AcademicTerms
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsCurrent && t.IsActive);

        return term == null ? null : MapToDto(term);
    }

    public async Task<AcademicTermDto> ActivateAsync(int id)
    {
        var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == id);

        if (term == null)
            throw new KeyNotFoundException("Akademik dönem bulunamadı");

        // Deactivate other current terms
        await DeactivateOtherCurrentTermsAsync();

        term.IsCurrent = true;
        term.IsActive = true;
        term.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(term);
    }

    private async Task DeactivateOtherCurrentTermsAsync()
    {
        var currentTerms = await _context.AcademicTerms
            .Where(t => t.IsCurrent)
            .ToListAsync();

        foreach (var t in currentTerms)
        {
            t.IsCurrent = false;
            t.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static AcademicTermDto MapToDto(AcademicTerm t)
    {
        return new AcademicTermDto
        {
            Id = t.Id,
            TermName = t.TermName,
            AcademicYear = t.AcademicYear,
            TermNumber = t.TermNumber,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            MidtermStartDate = t.MidtermStartDate,
            MidtermEndDate = t.MidtermEndDate,
            FinalStartDate = t.FinalStartDate,
            FinalEndDate = t.FinalEndDate,
            IsActive = t.IsActive,
            IsCurrent = t.IsCurrent,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
    }
}
