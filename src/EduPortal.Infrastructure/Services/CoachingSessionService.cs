using EduPortal.Application.DTOs.CoachingSession;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CoachingSessionService : ICoachingSessionService
{
    private readonly ApplicationDbContext _context;

    public CoachingSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CoachingSessionDto>> GetAllSessionsAsync()
    {
        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .Where(cs => !cs.IsDeleted)
            .OrderByDescending(cs => cs.SessionDate)
            .ToListAsync();

        return sessions.Select(MapToDto);
    }

    public async Task<IEnumerable<CoachingSessionDto>> GetUpcomingSessionsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(days);

        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .Where(cs => !cs.IsDeleted &&
                        cs.SessionDate >= startDate &&
                        cs.SessionDate <= endDate &&
                        cs.Status == SessionStatus.Scheduled)
            .OrderBy(cs => cs.SessionDate)
            .ToListAsync();

        return sessions.Select(MapToDto);
    }

    public async Task<IEnumerable<CoachingSessionDto>> GetSessionsByStudentAsync(int studentId)
    {
        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .Where(cs => cs.StudentId == studentId && !cs.IsDeleted)
            .OrderByDescending(cs => cs.SessionDate)
            .ToListAsync();

        return sessions.Select(MapToDto);
    }

    public async Task<IEnumerable<CoachingSessionDto>> GetSessionsByCoachAsync(int coachId)
    {
        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .Where(cs => cs.CoachId == coachId && !cs.IsDeleted)
            .OrderByDescending(cs => cs.SessionDate)
            .ToListAsync();

        return sessions.Select(MapToDto);
    }

    public async Task<IEnumerable<CoachingSessionDto>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .Where(cs => !cs.IsDeleted &&
                        cs.SessionDate >= startDate &&
                        cs.SessionDate <= endDate)
            .OrderBy(cs => cs.SessionDate)
            .ToListAsync();

        return sessions.Select(MapToDto);
    }

    public async Task<IEnumerable<SessionCalendarDto>> GetCalendarEventsAsync(DateTime startDate, DateTime endDate)
    {
        var sessions = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Where(cs => !cs.IsDeleted &&
                        cs.SessionDate >= startDate &&
                        cs.SessionDate <= endDate)
            .ToListAsync();

        return sessions.Select(s => new SessionCalendarDto
        {
            Id = s.Id,
            Title = s.Title,
            Start = s.SessionDate,
            End = s.SessionDate.AddMinutes(s.DurationMinutes),
            StudentName = $"{s.Student.User.FirstName} {s.Student.User.LastName}",
            CoachName = $"{s.Coach.User.FirstName} {s.Coach.User.LastName}",
            Status = s.Status.ToString(),
            Type = s.SessionType.ToString(),
            BackgroundColor = GetStatusColor(s.Status)
        });
    }

    public async Task<CoachingSessionDto?> GetSessionByIdAsync(int id)
    {
        var session = await _context.CoachingSessions
            .Include(cs => cs.Student).ThenInclude(s => s.User)
            .Include(cs => cs.Coach).ThenInclude(c => c.User)
            .Include(cs => cs.Branch)
            .FirstOrDefaultAsync(cs => cs.Id == id && !cs.IsDeleted);

        return session != null ? MapToDto(session) : null;
    }

    public async Task<CoachingSessionDto> CreateSessionAsync(CreateCoachingSessionDto dto)
    {
        // Check for conflicts
        var hasConflict = await _context.CoachingSessions
            .AnyAsync(cs => cs.CoachId == dto.CoachId &&
                           !cs.IsDeleted &&
                           cs.Status != SessionStatus.Cancelled &&
                           cs.SessionDate.Date == dto.SessionDate.Date &&
                           Math.Abs((cs.SessionDate - dto.SessionDate).TotalMinutes) < cs.DurationMinutes);

        if (hasConflict)
            throw new Exception("Coach has another session at this time");

        var session = new CoachingSession
        {
            StudentId = dto.StudentId,
            CoachId = dto.CoachId,
            BranchId = dto.BranchId,
            CoachingArea = (CoachingArea)dto.CoachingArea,
            Title = dto.Title,
            SessionDate = dto.SessionDate,
            DurationMinutes = dto.DurationMinutes,
            SessionType = (SessionType)dto.SessionType,
            Status = SessionStatus.Scheduled,
            SessionNotes = dto.SessionNotes
        };

        _context.CoachingSessions.Add(session);
        await _context.SaveChangesAsync();

        return (await GetSessionByIdAsync(session.Id))!;
    }

    public async Task<CoachingSessionDto> UpdateSessionAsync(int id, UpdateCoachingSessionDto dto)
    {
        var session = await _context.CoachingSessions.FindAsync(id);
        if (session == null || session.IsDeleted)
            throw new Exception("Session not found");

        session.Title = dto.Title;
        session.SessionDate = dto.SessionDate;
        session.DurationMinutes = dto.DurationMinutes;
        session.SessionType = (SessionType)dto.SessionType;
        session.Status = (SessionStatus)dto.Status;
        session.SessionNotes = dto.SessionNotes;
        session.ActionItems = dto.ActionItems;

        await _context.SaveChangesAsync();

        return (await GetSessionByIdAsync(id))!;
    }

    public async Task<CoachingSessionDto> CompleteSessionAsync(int id, CompleteSessionDto dto)
    {
        var session = await _context.CoachingSessions.FindAsync(id);
        if (session == null || session.IsDeleted)
            throw new Exception("Session not found");

        if (session.Status != SessionStatus.Scheduled)
            throw new Exception("Only scheduled sessions can be completed");

        session.Status = SessionStatus.Completed;
        session.SessionNotes = dto.SessionNotes;
        session.ActionItems = dto.ActionItems;
        session.StudentFeedback = dto.StudentFeedback;
        session.Rating = dto.Rating;
        session.NextSessionDate = dto.NextSessionDate;

        await _context.SaveChangesAsync();

        return (await GetSessionByIdAsync(id))!;
    }

    public async Task<bool> CancelSessionAsync(int id, string reason)
    {
        var session = await _context.CoachingSessions.FindAsync(id);
        if (session == null || session.IsDeleted)
            return false;

        session.Status = SessionStatus.Cancelled;
        session.SessionNotes = $"Cancelled: {reason}";

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        var session = await _context.CoachingSessions.FindAsync(id);
        if (session == null || session.IsDeleted)
            return false;

        session.IsDeleted = true;
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private CoachingSessionDto MapToDto(CoachingSession session)
    {
        return new CoachingSessionDto
        {
            Id = session.Id,
            StudentId = session.StudentId,
            StudentName = $"{session.Student.User.FirstName} {session.Student.User.LastName}",
            CoachId = session.CoachId,
            CoachName = $"{session.Coach.User.FirstName} {session.Coach.User.LastName}",
            BranchId = session.BranchId,
            BranchName = session.Branch?.BranchName,
            CoachingArea = session.CoachingArea.ToString(),
            Title = session.Title,
            SessionDate = session.SessionDate,
            DurationMinutes = session.DurationMinutes,
            SessionType = session.SessionType.ToString(),
            Status = session.Status.ToString(),
            SessionNotes = session.SessionNotes,
            ActionItems = session.ActionItems,
            StudentFeedback = session.StudentFeedback,
            Rating = session.Rating,
            NextSessionDate = session.NextSessionDate,
            AttachmentUrl = session.AttachmentUrl,
            CreatedDate = session.CreatedAt
        };
    }

    private string GetStatusColor(SessionStatus status)
    {
        return status switch
        {
            SessionStatus.Scheduled => "#3B82F6", // blue
            SessionStatus.Completed => "#10B981", // green
            SessionStatus.Cancelled => "#EF4444", // red
            SessionStatus.NoShow => "#F59E0B",    // orange
            _ => "#6B7280" // gray
        };
    }
}
