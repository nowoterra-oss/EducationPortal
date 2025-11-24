using EduPortal.Application.DTOs.Calendar;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _context;

    public CalendarService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CalendarEventDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.StartDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto);

        return (dtos, totalCount);
    }

    public async Task<CalendarEventDto?> GetByIdAsync(int id)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        return calendarEvent != null ? MapToDto(calendarEvent) : null;
    }

    public async Task<CalendarEventDto> CreateAsync(CreateCalendarEventDto dto)
    {
        var calendarEvent = new CalendarEvent
        {
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
            Scope = dto.Scope,
            Title = dto.Title,
            Description = dto.Description,
            EventType = dto.EventType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AllDayEvent = dto.AllDayEvent,
            Location = dto.Location,
            Priority = dto.Priority,
            Reminder = dto.Reminder,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(calendarEvent.Id))!;
    }

    public async Task<CalendarEventDto> UpdateAsync(int id, UpdateCalendarEventDto dto)
    {
        var calendarEvent = await _context.CalendarEvents.FindAsync(id);
        if (calendarEvent == null || calendarEvent.IsDeleted)
            throw new KeyNotFoundException("Etkinlik bulunamadı");

        calendarEvent.StudentId = dto.StudentId;
        calendarEvent.ClassId = dto.ClassId;
        calendarEvent.Scope = dto.Scope;
        calendarEvent.Title = dto.Title;
        calendarEvent.Description = dto.Description;
        calendarEvent.EventType = dto.EventType;
        calendarEvent.StartDate = dto.StartDate;
        calendarEvent.EndDate = dto.EndDate;
        calendarEvent.AllDayEvent = dto.AllDayEvent;
        calendarEvent.Location = dto.Location;
        calendarEvent.IsCompleted = dto.IsCompleted;
        calendarEvent.Priority = dto.Priority;
        calendarEvent.Reminder = dto.Reminder;
        calendarEvent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var calendarEvent = await _context.CalendarEvents.FindAsync(id);
        if (calendarEvent == null || calendarEvent.IsDeleted)
            return false;

        calendarEvent.IsDeleted = true;
        calendarEvent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CalendarEventDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .Where(e => !e.IsDeleted &&
                        e.StartDate >= startDate &&
                        e.StartDate <= endDate)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return events.Select(MapToDto);
    }

    public async Task<IEnumerable<CalendarEventDto>> GetByStudentAsync(int studentId)
    {
        var events = await _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .Where(e => !e.IsDeleted && e.StudentId == studentId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        return events.Select(MapToDto);
    }

    public async Task<IEnumerable<CalendarEventDto>> GetUpcomingAsync(int days)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        var events = await _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .Where(e => !e.IsDeleted &&
                        !e.IsCompleted &&
                        e.StartDate >= now &&
                        e.StartDate <= endDate)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return events.Select(MapToDto);
    }

    public async Task<IEnumerable<CalendarEventDto>> GetByClassAsync(int classId)
    {
        var events = await _context.CalendarEvents
            .Include(e => e.Student)
                .ThenInclude(s => s!.User)
            .Include(e => e.Class)
            .Where(e => !e.IsDeleted && e.ClassId == classId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        return events.Select(MapToDto);
    }

    private CalendarEventDto MapToDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventDto
        {
            Id = calendarEvent.Id,
            StudentId = calendarEvent.StudentId,
            StudentName = calendarEvent.Student != null
                ? $"{calendarEvent.Student.User.FirstName} {calendarEvent.Student.User.LastName}"
                : null,
            ClassId = calendarEvent.ClassId,
            ClassName = calendarEvent.Class?.ClassName,
            Scope = calendarEvent.Scope,
            Title = calendarEvent.Title,
            Description = calendarEvent.Description,
            EventType = calendarEvent.EventType,
            StartDate = calendarEvent.StartDate,
            EndDate = calendarEvent.EndDate,
            AllDayEvent = calendarEvent.AllDayEvent,
            Location = calendarEvent.Location,
            IsCompleted = calendarEvent.IsCompleted,
            Priority = calendarEvent.Priority,
            Reminder = calendarEvent.Reminder,
            CreatedAt = calendarEvent.CreatedAt
        };
    }
}
