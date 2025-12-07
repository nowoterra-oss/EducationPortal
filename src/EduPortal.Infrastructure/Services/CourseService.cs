using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Courses
            .Include(c => c.Curriculum)
            .Include(c => c.Resources)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.CourseName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Curriculum)
            .Include(c => c.Resources)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return course == null ? null : MapToDto(course);
    }

    public async Task<CourseDto> CreateAsync(CreateCourseDto dto)
    {
        // Aynı kod ile ders var mı kontrol et
        var existingCourse = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseCode == dto.CourseCode);

        if (existingCourse != null)
        {
            throw new InvalidOperationException($"'{dto.CourseCode}' kodu ile bir ders zaten mevcut");
        }

        var course = new Course
        {
            CourseName = dto.CourseName,
            CourseCode = dto.CourseCode,
            Subject = null,              // Varsayılan: null
            Level = null,                // Varsayılan: null
            Credits = 3,                 // Varsayılan: 3 kredi
            Description = null,          // Varsayılan: null
            IsActive = true              // Varsayılan: aktif
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(course.Id) ?? throw new InvalidOperationException("Ders oluşturulamadı");
    }

    public async Task<CourseDto> UpdateAsync(int id, UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
            throw new KeyNotFoundException("Ders bulunamadı");

        course.CourseName = dto.CourseName;
        course.CourseCode = dto.CourseCode;
        course.Subject = dto.Subject;
        course.Level = dto.Level;
        course.Credits = dto.Credits;
        course.Description = dto.Description;
        course.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Ders güncellenemedi");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CurriculumDto>> GetCurriculumAsync(int courseId)
    {
        var curricula = await _context.Curricula
            .Include(c => c.Resources)
            .Where(c => c.CourseId == courseId)
            .OrderBy(c => c.TopicOrder)
            .AsNoTracking()
            .ToListAsync();

        return curricula.Select(MapToCurriculumDto);
    }

    public async Task<IEnumerable<CurriculumDto>> UpdateCurriculumAsync(int courseId, UpdateCurriculumDto dto)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            throw new KeyNotFoundException("Ders bulunamadı");

        var existingItems = await _context.Curricula
            .Where(c => c.CourseId == courseId)
            .ToListAsync();

        var existingIds = existingItems.Select(e => e.Id).ToHashSet();
        var incomingIds = dto.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Delete items not in incoming list
        var toDelete = existingItems.Where(e => !incomingIds.Contains(e.Id)).ToList();
        _context.Curricula.RemoveRange(toDelete);

        // Update existing and add new
        foreach (var item in dto.Items)
        {
            if (item.Id.HasValue && existingIds.Contains(item.Id.Value))
            {
                // Update existing
                var existing = existingItems.First(e => e.Id == item.Id.Value);
                existing.TopicName = item.TopicName;
                existing.TopicOrder = item.TopicOrder;
                existing.Description = item.Description;
                existing.EstimatedHours = item.EstimatedHours;
                existing.IsCompleted = item.IsCompleted;
            }
            else
            {
                // Add new
                var newItem = new Curriculum
                {
                    CourseId = courseId,
                    TopicName = item.TopicName,
                    TopicOrder = item.TopicOrder,
                    Description = item.Description,
                    EstimatedHours = item.EstimatedHours,
                    IsCompleted = item.IsCompleted
                };
                _context.Curricula.Add(newItem);
            }
        }

        await _context.SaveChangesAsync();

        return await GetCurriculumAsync(courseId);
    }

    public async Task<IEnumerable<CourseResourceDto>> GetResourcesAsync(int courseId)
    {
        var resources = await _context.CourseResources
            .Include(r => r.Curriculum)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return resources.Select(MapToResourceDto);
    }

    public async Task<CourseResourceDto> AddResourceAsync(int courseId, CreateCourseResourceDto dto)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            throw new KeyNotFoundException("Ders bulunamadı");

        if (dto.CurriculumId.HasValue)
        {
            var curriculumExists = await _context.Curricula
                .AnyAsync(c => c.Id == dto.CurriculumId && c.CourseId == courseId);
            if (!curriculumExists)
                throw new KeyNotFoundException("Müfredat konusu bulunamadı");
        }

        var resource = new CourseResource
        {
            CourseId = courseId,
            CurriculumId = dto.CurriculumId,
            Title = dto.Title,
            ResourceType = dto.ResourceType,
            ResourceUrl = dto.ResourceUrl,
            Description = dto.Description,
            IsVisible = dto.IsVisible
        };

        _context.CourseResources.Add(resource);
        await _context.SaveChangesAsync();

        var savedResource = await _context.CourseResources
            .Include(r => r.Curriculum)
            .AsNoTracking()
            .FirstAsync(r => r.Id == resource.Id);

        return MapToResourceDto(savedResource);
    }

    public async Task<bool> DeleteResourceAsync(int courseId, int resourceId)
    {
        var resource = await _context.CourseResources
            .FirstOrDefaultAsync(r => r.Id == resourceId && r.CourseId == courseId);

        if (resource == null)
            return false;

        _context.CourseResources.Remove(resource);
        await _context.SaveChangesAsync();

        return true;
    }

    private static CourseDto MapToDto(Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            CourseName = course.CourseName,
            CourseCode = course.CourseCode,
            Subject = course.Subject,
            Level = course.Level,
            Credits = course.Credits,
            Description = course.Description,
            IsActive = course.IsActive,
            CurriculumCount = course.Curriculum?.Count ?? 0,
            ResourceCount = course.Resources?.Count ?? 0,
            CreatedAt = course.CreatedAt
        };
    }

    private static CurriculumDto MapToCurriculumDto(Curriculum curriculum)
    {
        return new CurriculumDto
        {
            Id = curriculum.Id,
            CourseId = curriculum.CourseId,
            TopicName = curriculum.TopicName,
            TopicOrder = curriculum.TopicOrder,
            Description = curriculum.Description,
            EstimatedHours = curriculum.EstimatedHours,
            IsCompleted = curriculum.IsCompleted,
            Resources = curriculum.Resources?.Select(MapToResourceDto).ToList() ?? new List<CourseResourceDto>()
        };
    }

    private static CourseResourceDto MapToResourceDto(CourseResource resource)
    {
        return new CourseResourceDto
        {
            Id = resource.Id,
            CourseId = resource.CourseId,
            CurriculumId = resource.CurriculumId,
            CurriculumTopicName = resource.Curriculum?.TopicName,
            Title = resource.Title,
            ResourceType = resource.ResourceType,
            ResourceUrl = resource.ResourceUrl,
            Description = resource.Description,
            IsVisible = resource.IsVisible,
            CreatedAt = resource.CreatedAt
        };
    }
}
