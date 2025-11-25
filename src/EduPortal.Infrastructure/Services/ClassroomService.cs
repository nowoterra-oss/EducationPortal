using EduPortal.Application.DTOs.Classroom;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ClassroomService : IClassroomService
{
    private readonly ApplicationDbContext _context;

    public ClassroomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ClassroomDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? buildingName = null, bool? isLab = null)
    {
        var query = _context.Classrooms
            .Include(c => c.Branch)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(buildingName))
            query = query.Where(c => c.BuildingName.Contains(buildingName));

        if (isLab.HasValue)
            query = query.Where(c => c.IsLab == isLab.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.BuildingName)
            .ThenBy(c => c.RoomNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ClassroomDto?> GetByIdAsync(int id)
    {
        var classroom = await _context.Classrooms
            .Include(c => c.Branch)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return classroom == null ? null : MapToDto(classroom);
    }

    public async Task<ClassroomDto> CreateAsync(CreateClassroomDto dto)
    {
        var classroom = new Classroom
        {
            RoomNumber = dto.RoomNumber,
            BuildingName = dto.BuildingName,
            Capacity = dto.Capacity,
            Floor = dto.Floor,
            Equipment = dto.Equipment,
            HasProjector = dto.HasProjector,
            HasSmartBoard = dto.HasSmartBoard,
            HasComputer = dto.HasComputer,
            IsAvailable = dto.IsAvailable,
            IsLab = dto.IsLab,
            BranchId = dto.BranchId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Classrooms.Add(classroom);
        await _context.SaveChangesAsync();

        var created = await _context.Classrooms
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == classroom.Id);

        return MapToDto(created!);
    }

    public async Task<ClassroomDto> UpdateAsync(int id, UpdateClassroomDto dto)
    {
        var classroom = await _context.Classrooms
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
            throw new KeyNotFoundException("Derslik bulunamadı");

        classroom.RoomNumber = dto.RoomNumber;
        classroom.BuildingName = dto.BuildingName;
        classroom.Capacity = dto.Capacity;
        classroom.Floor = dto.Floor;
        classroom.Equipment = dto.Equipment;
        classroom.HasProjector = dto.HasProjector;
        classroom.HasSmartBoard = dto.HasSmartBoard;
        classroom.HasComputer = dto.HasComputer;
        classroom.IsAvailable = dto.IsAvailable;
        classroom.IsLab = dto.IsLab;
        classroom.BranchId = dto.BranchId;
        classroom.Notes = dto.Notes;
        classroom.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(classroom);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classroom = await _context.Classrooms.FindAsync(id);
        if (classroom == null)
            return false;

        _context.Classrooms.Remove(classroom);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ClassroomDto>> GetAvailableAsync(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        // Get all classrooms that are available
        var allClassrooms = await _context.Classrooms
            .Include(c => c.Branch)
            .AsNoTracking()
            .Where(c => c.IsAvailable)
            .ToListAsync();

        // Get classrooms that are busy at the specified time
        var busyClassroomIds = await _context.LessonSchedules
            .Where(s => s.DayOfWeek == dayOfWeek &&
                        s.ClassroomId != null &&
                        ((s.StartTime <= startTime && s.EndTime > startTime) ||
                         (s.StartTime < endTime && s.EndTime >= endTime) ||
                         (s.StartTime >= startTime && s.EndTime <= endTime)))
            .Select(s => s.ClassroomId!.Value)
            .Distinct()
            .ToListAsync();

        // Filter out busy classrooms
        var availableClassrooms = allClassrooms
            .Where(c => !busyClassroomIds.Contains(c.Id))
            .Select(c => MapToDto(c))
            .ToList();

        return availableClassrooms;
    }

    public async Task<IEnumerable<ClassroomScheduleDto>> GetScheduleAsync(int classroomId)
    {
        var schedules = await _context.LessonSchedules
            .Include(s => s.Course)
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Classroom)
            .AsNoTracking()
            .Where(s => s.ClassroomId == classroomId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return schedules.Select(s => new ClassroomScheduleDto
        {
            ClassroomId = s.ClassroomId ?? 0,
            RoomNumber = s.Classroom?.RoomNumber ?? string.Empty,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            CourseName = s.Course?.CourseName,
            TeacherName = s.Teacher?.User != null ? $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}" : null
        });
    }

    private static ClassroomDto MapToDto(Classroom c)
    {
        return new ClassroomDto
        {
            Id = c.Id,
            RoomNumber = c.RoomNumber,
            BuildingName = c.BuildingName,
            Capacity = c.Capacity,
            Floor = c.Floor,
            Equipment = c.Equipment,
            HasProjector = c.HasProjector,
            HasSmartBoard = c.HasSmartBoard,
            HasComputer = c.HasComputer,
            IsAvailable = c.IsAvailable,
            IsLab = c.IsLab,
            BranchId = c.BranchId,
            BranchName = c.Branch?.BranchName,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt
        };
    }
}
