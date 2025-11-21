using EduPortal.Application.DTOs.CoachingSession;

namespace EduPortal.Application.Interfaces;

public interface ICoachingSessionService
{
    Task<IEnumerable<CoachingSessionDto>> GetAllSessionsAsync();
    Task<IEnumerable<CoachingSessionDto>> GetUpcomingSessionsAsync(int days = 7);
    Task<IEnumerable<CoachingSessionDto>> GetSessionsByStudentAsync(int studentId);
    Task<IEnumerable<CoachingSessionDto>> GetSessionsByCoachAsync(int coachId);
    Task<IEnumerable<CoachingSessionDto>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<SessionCalendarDto>> GetCalendarEventsAsync(DateTime startDate, DateTime endDate);
    Task<CoachingSessionDto?> GetSessionByIdAsync(int id);
    Task<CoachingSessionDto> CreateSessionAsync(CreateCoachingSessionDto dto);
    Task<CoachingSessionDto> UpdateSessionAsync(int id, UpdateCoachingSessionDto dto);
    Task<CoachingSessionDto> CompleteSessionAsync(int id, CompleteSessionDto dto);
    Task<bool> CancelSessionAsync(int id, string reason);
    Task<bool> DeleteSessionAsync(int id);
}
