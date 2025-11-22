using EduPortal.Application.DTOs.Accommodation;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class AccommodationArrangementService : IAccommodationArrangementService
{
    private readonly ApplicationDbContext _context;

    public AccommodationArrangementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AccommodationArrangementDto>> GetAllArrangementsAsync()
    {
        var arrangements = await _context.AccommodationArrangements
            .Include(a => a.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return arrangements.Select(MapToDto);
    }

    public async Task<IEnumerable<AccommodationArrangementDto>> GetActiveArrangementsAsync()
    {
        var now = DateTime.UtcNow;

        var arrangements = await _context.AccommodationArrangements
            .Include(a => a.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(a => !a.IsDeleted &&
                       a.Status == AccommodationStatus.Confirmed &&
                       a.StartDate.HasValue &&
                       a.EndDate.HasValue &&
                       a.StartDate.Value <= now &&
                       a.EndDate.Value >= now)
            .ToListAsync();

        return arrangements.Select(MapToDto);
    }

    public async Task<IEnumerable<AccommodationArrangementDto>> GetArrangementsByProgramAsync(int programId)
    {
        var arrangements = await _context.AccommodationArrangements
            .Include(a => a.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(a => a.ProgramId == programId && !a.IsDeleted)
            .ToListAsync();

        return arrangements.Select(MapToDto);
    }

    public async Task<IEnumerable<AccommodationSummaryDto>> GetArrangementSummariesAsync()
    {
        var arrangements = await _context.AccommodationArrangements
            .Include(a => a.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        return arrangements.Select(a => new AccommodationSummaryDto
        {
            Id = a.Id,
            StudentName = $"{a.Program.Student.User.FirstName} {a.Program.Student.User.LastName}",
            Type = a.Type.ToString(),
            Name = a.Name,
            Status = a.Status.ToString(),
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            MonthlyCost = a.MonthlyRent
        });
    }

    public async Task<AccommodationArrangementDto?> GetArrangementByIdAsync(int id)
    {
        var arrangement = await _context.AccommodationArrangements
            .Include(a => a.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return arrangement != null ? MapToDto(arrangement) : null;
    }

    public async Task<AccommodationArrangementDto> CreateArrangementAsync(CreateAccommodationArrangementDto dto)
    {
        var arrangement = new AccommodationArrangement
        {
            ProgramId = dto.ProgramId,
            Type = (AccommodationType)dto.Type,
            Name = dto.Name,
            Address = dto.Address,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = AccommodationStatus.Searching,
            MonthlyRent = dto.MonthlyCost,
            Deposit = dto.SecurityDeposit,
            ContactPerson = dto.ContactPerson,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            Notes = dto.Notes
        };

        _context.AccommodationArrangements.Add(arrangement);
        await _context.SaveChangesAsync();

        return (await GetArrangementByIdAsync(arrangement.Id))!;
    }

    public async Task<AccommodationArrangementDto> UpdateArrangementAsync(int id, UpdateAccommodationArrangementDto dto)
    {
        var arrangement = await _context.AccommodationArrangements.FindAsync(id);
        if (arrangement == null || arrangement.IsDeleted)
            throw new Exception("Accommodation arrangement not found");

        arrangement.Type = (AccommodationType)dto.Type;
        arrangement.Name = dto.Name;
        arrangement.Address = dto.Address;
        arrangement.StartDate = dto.StartDate;
        arrangement.EndDate = dto.EndDate;
        arrangement.Status = (AccommodationStatus)dto.Status;
        arrangement.MonthlyRent = dto.MonthlyCost;
        arrangement.Deposit = dto.SecurityDeposit;
        arrangement.ContactPerson = dto.ContactPerson;
        arrangement.ContactPhone = dto.ContactPhone;
        arrangement.ContactEmail = dto.ContactEmail;
        arrangement.Notes = dto.Notes;
        arrangement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetArrangementByIdAsync(id))!;
    }

    public async Task<bool> DeleteArrangementAsync(int id)
    {
        var arrangement = await _context.AccommodationArrangements.FindAsync(id);
        if (arrangement == null || arrangement.IsDeleted)
            return false;

        arrangement.IsDeleted = true;
        arrangement.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<AccommodationStatisticsDto> GetStatisticsAsync()
    {
        var arrangements = await _context.AccommodationArrangements
            .Include(a => a.Program)
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var stats = new AccommodationStatisticsDto
        {
            TotalArrangements = arrangements.Count,
            ActiveArrangements = arrangements.Count(a =>
                a.StartDate.HasValue && a.EndDate.HasValue &&
                a.StartDate.Value <= now && a.EndDate.Value >= now &&
                a.Status == AccommodationStatus.Confirmed),
            ConfirmedArrangements = arrangements.Count(a => a.Status == AccommodationStatus.Confirmed),
            PendingArrangements = arrangements.Count(a =>
                a.Status == AccommodationStatus.Searching ||
                a.Status == AccommodationStatus.Applied)
        };

        stats.ArrangementsByType = arrangements
            .GroupBy(a => a.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.ArrangementsByStatus = arrangements
            .GroupBy(a => a.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.ArrangementsByCountry = arrangements
            .GroupBy(a => a.Program.TargetCountry)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.TotalMonthlyCosts = arrangements.Sum(a => a.MonthlyRent ?? 0);
        stats.TotalSecurityDeposits = arrangements.Sum(a => a.Deposit ?? 0);
        stats.AverageMonthlyCost = arrangements.Any()
            ? arrangements.Average(a => a.MonthlyRent ?? 0)
            : 0;

        return stats;
    }

    private AccommodationArrangementDto MapToDto(AccommodationArrangement arrangement)
    {
        var now = DateTime.UtcNow;
        var durationMonths = arrangement.StartDate.HasValue && arrangement.EndDate.HasValue
            ? (int)((arrangement.EndDate.Value - arrangement.StartDate.Value).TotalDays / 30)
            : 0;
        var totalCost = arrangement.MonthlyRent.HasValue && durationMonths > 0
            ? arrangement.MonthlyRent.Value * durationMonths
            : (decimal?)null;
        var isActive = arrangement.StartDate.HasValue && arrangement.EndDate.HasValue &&
                      arrangement.StartDate.Value <= now && arrangement.EndDate.Value >= now;
        var daysUntilStart = arrangement.StartDate.HasValue
            ? (int)(arrangement.StartDate.Value - now).TotalDays
            : 0;
        var daysUntilEnd = arrangement.EndDate.HasValue
            ? (int)(arrangement.EndDate.Value - now).TotalDays
            : 0;

        return new AccommodationArrangementDto
        {
            Id = arrangement.Id,
            ProgramId = arrangement.ProgramId,
            StudentName = $"{arrangement.Program.Student.User.FirstName} {arrangement.Program.Student.User.LastName}",
            StudentNo = arrangement.Program.Student.StudentNo,
            TargetCountry = arrangement.Program.TargetCountry,
            TargetUniversity = arrangement.Program.TargetUniversity,
            Type = arrangement.Type.ToString(),
            Name = arrangement.Name,
            Address = arrangement.Address,
            StartDate = arrangement.StartDate,
            EndDate = arrangement.EndDate,
            Status = arrangement.Status.ToString(),
            MonthlyCost = arrangement.MonthlyRent,
            SecurityDeposit = arrangement.Deposit,
            TotalCost = totalCost,
            ContactPerson = arrangement.ContactPerson,
            ContactPhone = arrangement.ContactPhone,
            ContactEmail = arrangement.ContactEmail,
            Notes = arrangement.Notes,
            DurationMonths = durationMonths,
            IsActive = isActive,
            DaysUntilStart = daysUntilStart,
            DaysUntilEnd = daysUntilEnd,
            CreatedDate = arrangement.CreatedAt,
            LastModifiedDate = arrangement.UpdatedAt
        };
    }
}
