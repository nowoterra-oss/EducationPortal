using EduPortal.Application.DTOs.Coach;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EduPortal.Application.Services;

public class CoachService : ICoachService
{
    private readonly ApplicationDbContext _context;

    public CoachService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CoachDto>> GetAllCoachesAsync()
    {
        var coaches = await _context.Coaches
            .Include(c => c.User)
            .Include(c => c.Branch)
            .Include(c => c.TeacherProfile)
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        return coaches.Select(MapToDto);
    }

    public async Task<IEnumerable<CoachSummaryDto>> GetAvailableCoachesAsync()
    {
        var coaches = await _context.Coaches
            .Include(c => c.User)
            .Where(c => !c.IsDeleted && c.IsAvailable)
            .ToListAsync();

        return coaches.Select(c => new CoachSummaryDto
        {
            Id = c.Id,
            FullName = $"{c.User.FirstName} {c.User.LastName}",
            Specialization = c.Specialization,
            CoachingAreas = ParseCoachingAreas(c.AreasJson),
            IsAvailable = c.IsAvailable,
            ActiveStudentCount = _context.StudentCoachAssignments
                .Count(sca => sca.CoachId == c.Id && sca.IsActive && !sca.IsDeleted),
            AverageRating = CalculateAverageRating(c.Id)
        });
    }

    public async Task<CoachDto?> GetCoachByIdAsync(int id)
    {
        var coach = await _context.Coaches
            .Include(c => c.User)
            .Include(c => c.Branch)
            .Include(c => c.TeacherProfile)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return coach != null ? MapToDto(coach) : null;
    }

    public async Task<CoachDto> CreateCoachAsync(CreateCoachDto dto)
    {
        var coach = new Coach
        {
            UserId = dto.UserId,
            BranchId = dto.BranchId,
            Specialization = dto.Specialization,
            Qualifications = dto.Qualifications,
            ExperienceYears = dto.ExperienceYears,
            AreasJson = JsonSerializer.Serialize(dto.CoachingAreas),
            IsAvailable = true,
            HourlyRate = dto.HourlyRate,
            Bio = dto.Bio,
            IsAlsoTeacher = dto.IsAlsoTeacher,
            TeacherId = dto.TeacherId
        };

        _context.Coaches.Add(coach);
        await _context.SaveChangesAsync();

        return (await GetCoachByIdAsync(coach.Id))!;
    }

    public async Task<CoachDto> UpdateCoachAsync(int id, UpdateCoachDto dto)
    {
        var coach = await _context.Coaches.FindAsync(id);
        if (coach == null || coach.IsDeleted)
            throw new Exception("Coach not found");

        coach.BranchId = dto.BranchId;
        coach.Specialization = dto.Specialization;
        coach.Qualifications = dto.Qualifications;
        coach.ExperienceYears = dto.ExperienceYears;
        coach.AreasJson = JsonSerializer.Serialize(dto.CoachingAreas);
        coach.IsAvailable = dto.IsAvailable;
        coach.HourlyRate = dto.HourlyRate;
        coach.Bio = dto.Bio;

        await _context.SaveChangesAsync();

        return (await GetCoachByIdAsync(id))!;
    }

    public async Task<bool> DeleteCoachAsync(int id)
    {
        var coach = await _context.Coaches.FindAsync(id);
        if (coach == null || coach.IsDeleted)
            return false;

        coach.IsDeleted = true;
        coach.DeletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CoachPerformanceDto> GetCoachPerformanceAsync(int id, DateTime startDate, DateTime endDate)
    {
        var coach = await _context.Coaches
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (coach == null)
            throw new Exception("Coach not found");

        var sessions = await _context.CoachingSessions
            .Where(cs => cs.CoachId == id &&
                        cs.SessionDate >= startDate &&
                        cs.SessionDate <= endDate &&
                        !cs.IsDeleted)
            .ToListAsync();

        var performance = new CoachPerformanceDto
        {
            CoachId = id,
            CoachName = $"{coach.User.FirstName} {coach.User.LastName}",
            StartDate = startDate,
            EndDate = endDate
        };

        performance.TotalSessionsScheduled = sessions.Count;
        performance.TotalSessionsCompleted = sessions.Count(s => s.Status == SessionStatus.Completed);
        performance.TotalSessionsCancelled = sessions.Count(s => s.Status == SessionStatus.Cancelled);
        performance.TotalSessionsNoShow = sessions.Count(s => s.Status == SessionStatus.NoShow);
        performance.CompletionRate = performance.TotalSessionsScheduled > 0
            ? (decimal)performance.TotalSessionsCompleted / performance.TotalSessionsScheduled * 100
            : 0;

        performance.TotalStudents = await _context.StudentCoachAssignments
            .Where(sca => sca.CoachId == id && !sca.IsDeleted)
            .Select(sca => sca.StudentId)
            .Distinct()
            .CountAsync();

        performance.ActiveStudents = await _context.StudentCoachAssignments
            .CountAsync(sca => sca.CoachId == id && sca.IsActive && !sca.IsDeleted);

        var ratingsQuery = sessions
            .Where(s => s.Rating.HasValue)
            .Select(s => s.Rating!.Value);

        performance.AverageRating = ratingsQuery.Any() ? ratingsQuery.Average() : 0;
        performance.TotalRatings = ratingsQuery.Count();

        return performance;
    }

    public async Task<IEnumerable<CoachDto>> GetCoachesByBranchAsync(int branchId)
    {
        var coaches = await _context.Coaches
            .Include(c => c.User)
            .Include(c => c.Branch)
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .ToListAsync();

        return coaches.Select(MapToDto);
    }

    private CoachDto MapToDto(Coach coach)
    {
        return new CoachDto
        {
            Id = coach.Id,
            UserId = coach.UserId,
            UserName = coach.User.UserName ?? "",
            FullName = $"{coach.User.FirstName} {coach.User.LastName}",
            Email = coach.User.Email,
            PhoneNumber = coach.User.PhoneNumber,
            BranchId = coach.BranchId,
            BranchName = coach.Branch?.BranchName,
            Specialization = coach.Specialization,
            Qualifications = coach.Qualifications,
            ExperienceYears = coach.ExperienceYears,
            CoachingAreas = ParseCoachingAreas(coach.AreasJson),
            IsAvailable = coach.IsAvailable,
            HourlyRate = coach.HourlyRate,
            Bio = coach.Bio,
            IsAlsoTeacher = coach.IsAlsoTeacher,
            TeacherId = coach.TeacherId,
            TeacherName = coach.TeacherProfile != null
                ? $"{coach.TeacherProfile.User.FirstName} {coach.TeacherProfile.User.LastName}"
                : null,
            ActiveStudentCount = _context.StudentCoachAssignments
                .Count(sca => sca.CoachId == coach.Id && sca.IsActive && !sca.IsDeleted),
            TotalSessionsCompleted = _context.CoachingSessions
                .Count(cs => cs.CoachId == coach.Id && cs.Status == SessionStatus.Completed && !cs.IsDeleted),
            AverageRating = CalculateAverageRating(coach.Id),
            CreatedDate = coach.CreatedDate
        };
    }

    private List<string> ParseCoachingAreas(string? areasJson)
    {
        if (string.IsNullOrEmpty(areasJson))
            return new List<string>();

        try
        {
            var areaIds = JsonSerializer.Deserialize<List<int>>(areasJson);
            return areaIds?.Select(id => ((CoachingArea)id).ToString()).ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private decimal CalculateAverageRating(int coachId)
    {
        var ratings = _context.CoachingSessions
            .Where(cs => cs.CoachId == coachId && cs.Rating.HasValue && !cs.IsDeleted)
            .Select(cs => cs.Rating!.Value)
            .ToList();

        return ratings.Any() ? ratings.Average() : 0;
    }
}
