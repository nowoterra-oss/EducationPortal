using EduPortal.Application.DTOs.Hobby;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class HobbyService : IHobbyService
{
    private readonly ApplicationDbContext _context;

    public HobbyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HobbyDto>> GetAllAsync()
    {
        return await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .OrderBy(h => h.Category)
            .ThenBy(h => h.Name)
            .Select(h => MapToDto(h))
            .ToListAsync();
    }

    public async Task<HobbyDto?> GetByIdAsync(int id)
    {
        var hobby = await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);

        return hobby == null ? null : MapToDto(hobby);
    }

    public async Task<HobbyDto> CreateAsync(CreateHobbyDto dto)
    {
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            throw new KeyNotFoundException("Öğrenci bulunamadı");

        var hobby = new StudentHobby
        {
            StudentId = dto.StudentId,
            Category = dto.Category,
            Name = dto.Name,
            HasLicense = dto.HasLicense,
            LicenseLevel = dto.LicenseLevel,
            LicenseDocumentUrl = dto.LicenseDocumentUrl,
            Achievements = dto.Achievements,
            StartDate = dto.StartDate
        };

        _context.StudentHobbies.Add(hobby);
        await _context.SaveChangesAsync();

        var created = await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(h => h.Id == hobby.Id);

        return MapToDto(created!);
    }

    public async Task<HobbyDto> UpdateAsync(int id, UpdateHobbyDto dto)
    {
        var hobby = await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hobby == null)
            throw new KeyNotFoundException("Hobi bulunamadı");

        hobby.Category = dto.Category;
        hobby.Name = dto.Name;
        hobby.HasLicense = dto.HasLicense;
        hobby.LicenseLevel = dto.LicenseLevel;
        hobby.LicenseDocumentUrl = dto.LicenseDocumentUrl;
        hobby.Achievements = dto.Achievements;
        hobby.StartDate = dto.StartDate;

        await _context.SaveChangesAsync();

        return MapToDto(hobby);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hobby = await _context.StudentHobbies.FindAsync(id);
        if (hobby == null)
            return false;

        _context.StudentHobbies.Remove(hobby);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<HobbyDto>> GetByStudentAsync(int studentId)
    {
        return await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .Where(h => h.StudentId == studentId)
            .OrderBy(h => h.Category)
            .ThenBy(h => h.Name)
            .Select(h => MapToDto(h))
            .ToListAsync();
    }

    public async Task<IEnumerable<HobbyDto>> GetByCategoryAsync(string category)
    {
        return await _context.StudentHobbies
            .Include(h => h.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .Where(h => h.Category == category)
            .OrderBy(h => h.Name)
            .Select(h => MapToDto(h))
            .ToListAsync();
    }

    private static HobbyDto MapToDto(StudentHobby h)
    {
        return new HobbyDto
        {
            Id = h.Id,
            StudentId = h.StudentId,
            StudentName = h.Student?.User != null ? $"{h.Student.User.FirstName} {h.Student.User.LastName}" : string.Empty,
            Category = h.Category,
            Name = h.Name,
            HasLicense = h.HasLicense,
            LicenseLevel = h.LicenseLevel,
            LicenseDocumentUrl = h.LicenseDocumentUrl,
            Achievements = h.Achievements,
            StartDate = h.StartDate
        };
    }
}
