using EduPortal.Application.DTOs.Competition;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CompetitionService : ICompetitionService
{
    private readonly ApplicationDbContext _context;

    public CompetitionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CompetitionDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.CompetitionsAndAwards
            .Include(c => c.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CompetitionDto?> GetByIdAsync(int id)
    {
        var competition = await _context.CompetitionsAndAwards
            .Include(c => c.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return competition == null ? null : MapToDto(competition);
    }

    public async Task<CompetitionDto> CreateAsync(CreateCompetitionDto dto)
    {
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            throw new KeyNotFoundException("Öğrenci bulunamadı");

        var competition = new CompetitionAndAward
        {
            StudentId = dto.StudentId,
            Name = dto.Name,
            Category = dto.Category,
            Level = dto.Level,
            Achievement = dto.Achievement,
            Date = dto.Date,
            DocumentUrl = dto.DocumentUrl,
            Description = dto.Description
        };

        _context.CompetitionsAndAwards.Add(competition);
        await _context.SaveChangesAsync();

        var created = await _context.CompetitionsAndAwards
            .Include(c => c.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == competition.Id);

        return MapToDto(created!);
    }

    public async Task<CompetitionDto> UpdateAsync(int id, UpdateCompetitionDto dto)
    {
        var competition = await _context.CompetitionsAndAwards
            .Include(c => c.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competition == null)
            throw new KeyNotFoundException("Yarışma/Ödül bulunamadı");

        competition.Name = dto.Name;
        competition.Category = dto.Category;
        competition.Level = dto.Level;
        competition.Achievement = dto.Achievement;
        competition.Date = dto.Date;
        competition.DocumentUrl = dto.DocumentUrl;
        competition.Description = dto.Description;

        await _context.SaveChangesAsync();

        return MapToDto(competition);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var competition = await _context.CompetitionsAndAwards.FindAsync(id);
        if (competition == null)
            return false;

        _context.CompetitionsAndAwards.Remove(competition);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CompetitionDto>> GetByStudentAsync(int studentId)
    {
        return await _context.CompetitionsAndAwards
            .Include(c => c.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.Date)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    private static CompetitionDto MapToDto(CompetitionAndAward c)
    {
        return new CompetitionDto
        {
            Id = c.Id,
            StudentId = c.StudentId,
            StudentName = c.Student?.User != null ? $"{c.Student.User.FirstName} {c.Student.User.LastName}" : string.Empty,
            StudentNo = c.Student?.StudentNo,
            Name = c.Name,
            Category = c.Category,
            Level = c.Level,
            Achievement = c.Achievement,
            Date = c.Date,
            DocumentUrl = c.DocumentUrl,
            Description = c.Description
        };
    }
}
