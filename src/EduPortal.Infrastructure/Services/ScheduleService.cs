using EduPortal.Application.DTOs.Schedule;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _context;

    public ScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ScheduleDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ScheduleDto?> GetByIdAsync(int id)
    {
        var schedule = await _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return schedule == null ? null : MapToDto(schedule);
    }

    public async Task<ScheduleDto> CreateAsync(CreateScheduleDto dto)
    {
        // Validate student exists
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            throw new KeyNotFoundException("Öğrenci bulunamadı");

        // Validate teacher exists
        var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
        if (teacher == null)
            throw new KeyNotFoundException("Öğretmen bulunamadı");

        // Validate course exists
        var course = await _context.Courses.FindAsync(dto.CourseId);
        if (course == null)
            throw new KeyNotFoundException("Ders bulunamadı");

        // Check for schedule conflicts
        var hasConflict = await CheckScheduleConflictAsync(dto.StudentId, dto.TeacherId, dto.DayOfWeek, dto.StartTime, dto.EndTime, dto.EffectiveFrom, dto.EffectiveTo);
        if (hasConflict)
            throw new InvalidOperationException("Bu zaman diliminde çakışan bir program var");

        var schedule = new LessonSchedule
        {
            StudentId = dto.StudentId,
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsRecurring = dto.IsRecurring,
            Status = LessonStatus.Scheduled,
            ClassroomId = dto.ClassroomId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.LessonSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        var created = await _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == schedule.Id);

        return MapToDto(created!);
    }

    public async Task<ScheduleDto> UpdateAsync(int id, UpdateScheduleDto dto)
    {
        var schedule = await _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
            throw new KeyNotFoundException("Program bulunamadı");

        schedule.TeacherId = dto.TeacherId;
        schedule.CourseId = dto.CourseId;
        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.EffectiveTo = dto.EffectiveTo;
        schedule.IsRecurring = dto.IsRecurring;
        schedule.Status = dto.Status;
        schedule.ClassroomId = dto.ClassroomId;
        schedule.Notes = dto.Notes;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(schedule);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var schedule = await _context.LessonSchedules.FindAsync(id);
        if (schedule == null)
            return false;

        _context.LessonSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ScheduleDto>> GetByStudentAsync(int studentId)
    {
        return await _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduleDto>> GetByTeacherAsync(int teacherId)
    {
        return await _context.LessonSchedules
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    private async Task<bool> CheckScheduleConflictAsync(int studentId, int teacherId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, DateTime effectiveFrom, DateTime? effectiveTo, int? excludeId = null)
    {
        var query = _context.LessonSchedules
            .Where(s => !s.IsDeleted &&
                       s.Status == LessonStatus.Scheduled &&
                       s.DayOfWeek == dayOfWeek &&
                       (s.StudentId == studentId || s.TeacherId == teacherId) &&
                       // Time overlap check
                       ((s.StartTime <= startTime && s.EndTime > startTime) ||
                        (s.StartTime < endTime && s.EndTime >= endTime) ||
                        (s.StartTime >= startTime && s.EndTime <= endTime)) &&
                       // Date range overlap check
                       s.EffectiveFrom <= (effectiveTo ?? DateTime.MaxValue) &&
                       (s.EffectiveTo == null || s.EffectiveTo >= effectiveFrom));

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    private static ScheduleDto MapToDto(LessonSchedule s)
    {
        return new ScheduleDto
        {
            Id = s.Id,
            StudentId = s.StudentId,
            StudentName = s.Student?.User != null ? $"{s.Student.User.FirstName} {s.Student.User.LastName}" : string.Empty,
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.User != null ? $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}" : string.Empty,
            CourseId = s.CourseId,
            CourseName = s.Course?.CourseName ?? string.Empty,
            DayOfWeek = s.DayOfWeek,
            DayName = GetDayName(s.DayOfWeek),
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            EffectiveFrom = s.EffectiveFrom,
            EffectiveTo = s.EffectiveTo,
            IsRecurring = s.IsRecurring,
            Status = s.Status,
            StatusName = GetStatusName(s.Status),
            ClassroomId = s.ClassroomId,
            ClassroomName = s.Classroom?.RoomNumber,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt
        };
    }

    private static string GetDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Sunday => "Pazar",
            DayOfWeek.Monday => "Pazartesi",
            DayOfWeek.Tuesday => "Salı",
            DayOfWeek.Wednesday => "Çarşamba",
            DayOfWeek.Thursday => "Perşembe",
            DayOfWeek.Friday => "Cuma",
            DayOfWeek.Saturday => "Cumartesi",
            _ => "Bilinmiyor"
        };
    }

    private static string GetStatusName(LessonStatus status)
    {
        return status switch
        {
            LessonStatus.Scheduled => "Planlandı",
            LessonStatus.Completed => "Tamamlandı",
            LessonStatus.Cancelled => "İptal Edildi",
            LessonStatus.Rescheduled => "Yeniden Planlandı",
            _ => "Bilinmiyor"
        };
    }
}
