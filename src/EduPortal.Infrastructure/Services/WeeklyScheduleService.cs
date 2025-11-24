using EduPortal.Application.DTOs.WeeklySchedule;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class WeeklyScheduleService : IWeeklyScheduleService
{
    private readonly ApplicationDbContext _context;

    public WeeklyScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<WeeklyScheduleDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => !ws.IsDeleted)
            .OrderBy(ws => ws.DayOfWeek)
            .ThenBy(ws => ws.StartTime);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto);

        return (dtos, totalCount);
    }

    public async Task<WeeklyScheduleDto?> GetByIdAsync(int id)
    {
        var schedule = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .FirstOrDefaultAsync(ws => ws.Id == id && !ws.IsDeleted);

        return schedule != null ? MapToDto(schedule) : null;
    }

    public async Task<WeeklyScheduleDto> CreateAsync(CreateWeeklyScheduleDto dto)
    {
        var schedule = new WeeklySchedule
        {
            ClassId = dto.ClassId,
            CourseId = dto.CourseId,
            TeacherId = dto.TeacherId,
            ClassroomId = dto.ClassroomId,
            AcademicTermId = dto.AcademicTermId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            AcademicYear = dto.AcademicYear,
            IsActive = dto.IsActive,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.WeeklySchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(schedule.Id))!;
    }

    public async Task<IEnumerable<WeeklyScheduleDto>> CreateBulkAsync(IEnumerable<CreateWeeklyScheduleDto> dtos)
    {
        var now = DateTime.UtcNow;
        var schedules = dtos.Select(dto => new WeeklySchedule
        {
            ClassId = dto.ClassId,
            CourseId = dto.CourseId,
            TeacherId = dto.TeacherId,
            ClassroomId = dto.ClassroomId,
            AcademicTermId = dto.AcademicTermId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            AcademicYear = dto.AcademicYear,
            IsActive = dto.IsActive,
            Notes = dto.Notes,
            CreatedAt = now
        }).ToList();

        _context.WeeklySchedules.AddRange(schedules);
        await _context.SaveChangesAsync();

        var ids = schedules.Select(s => s.Id).ToList();
        var createdSchedules = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => ids.Contains(ws.Id))
            .ToListAsync();

        return createdSchedules.Select(MapToDto);
    }

    public async Task<WeeklyScheduleDto> UpdateAsync(int id, UpdateWeeklyScheduleDto dto)
    {
        var schedule = await _context.WeeklySchedules.FindAsync(id);
        if (schedule == null || schedule.IsDeleted)
            throw new KeyNotFoundException("Ders programı bulunamadı");

        schedule.ClassId = dto.ClassId;
        schedule.CourseId = dto.CourseId;
        schedule.TeacherId = dto.TeacherId;
        schedule.ClassroomId = dto.ClassroomId;
        schedule.AcademicTermId = dto.AcademicTermId;
        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.AcademicYear = dto.AcademicYear;
        schedule.IsActive = dto.IsActive;
        schedule.Notes = dto.Notes;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var schedule = await _context.WeeklySchedules.FindAsync(id);
        if (schedule == null || schedule.IsDeleted)
            return false;

        schedule.IsDeleted = true;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<WeeklyScheduleDto>> GetByClassAsync(int classId)
    {
        var schedules = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => ws.ClassId == classId && !ws.IsDeleted && ws.IsActive)
            .OrderBy(ws => ws.DayOfWeek)
            .ThenBy(ws => ws.StartTime)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

    public async Task<IEnumerable<WeeklyScheduleDto>> GetByTeacherAsync(int teacherId)
    {
        var schedules = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => ws.TeacherId == teacherId && !ws.IsDeleted && ws.IsActive)
            .OrderBy(ws => ws.DayOfWeek)
            .ThenBy(ws => ws.StartTime)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

    public async Task<IEnumerable<WeeklyScheduleDto>> GetByClassroomAsync(int classroomId)
    {
        var schedules = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => ws.ClassroomId == classroomId && !ws.IsDeleted && ws.IsActive)
            .OrderBy(ws => ws.DayOfWeek)
            .ThenBy(ws => ws.StartTime)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

    public async Task<IEnumerable<WeeklyScheduleDto>> GetTodayAsync()
    {
        var today = DateTime.Today.DayOfWeek;

        var schedules = await _context.WeeklySchedules
            .Include(ws => ws.Class)
            .Include(ws => ws.Course)
            .Include(ws => ws.Teacher)
                .ThenInclude(t => t.User)
            .Include(ws => ws.Classroom)
            .Include(ws => ws.AcademicTerm)
            .Where(ws => ws.DayOfWeek == today && !ws.IsDeleted && ws.IsActive)
            .OrderBy(ws => ws.StartTime)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

    private WeeklyScheduleDto MapToDto(WeeklySchedule schedule)
    {
        return new WeeklyScheduleDto
        {
            Id = schedule.Id,
            ClassId = schedule.ClassId,
            ClassName = schedule.Class.ClassName,
            CourseId = schedule.CourseId,
            CourseName = schedule.Course.CourseName,
            TeacherId = schedule.TeacherId,
            TeacherName = $"{schedule.Teacher.User.FirstName} {schedule.Teacher.User.LastName}",
            ClassroomId = schedule.ClassroomId,
            ClassroomName = schedule.Classroom?.RoomName,
            AcademicTermId = schedule.AcademicTermId,
            AcademicTermName = schedule.AcademicTerm.TermName,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            AcademicYear = schedule.AcademicYear,
            IsActive = schedule.IsActive,
            Notes = schedule.Notes
        };
    }
}
