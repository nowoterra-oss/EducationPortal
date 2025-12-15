using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Calendar;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class AdminCalendarService : IAdminCalendarService
{
    private readonly ApplicationDbContext _context;

    public AdminCalendarService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<AdminCalendarEventDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _context.AdminCalendarEvents
            .Where(e => !e.IsDeleted && e.EventDate >= startDate.Date && e.EventDate <= endDate.Date)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime)
            .Select(e => new AdminCalendarEventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EventType = e.EventType,
                EventDate = e.EventDate,
                StartTime = e.StartTime.ToString(@"hh\:mm"),
                EndTime = e.EndTime.ToString(@"hh\:mm"),
                Location = e.Location,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AdminCalendarEventDto>>.SuccessResponse(events);
    }

    public async Task<ApiResponse<AdminCalendarEventDto>> GetByIdAsync(int id)
    {
        var entity = await _context.AdminCalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Etkinlik bulunamadı.");

        var dto = new AdminCalendarEventDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            EventType = entity.EventType,
            EventDate = entity.EventDate,
            StartTime = entity.StartTime.ToString(@"hh\:mm"),
            EndTime = entity.EndTime.ToString(@"hh\:mm"),
            Location = entity.Location,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        return ApiResponse<AdminCalendarEventDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<AdminCalendarEventDto>> CreateAsync(AdminCalendarEventCreateDto dto)
    {
        if (!TimeSpan.TryParse(dto.StartTime, out var startTime))
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Geçersiz başlangıç saati formatı. HH:mm formatı bekleniyor.");

        if (!TimeSpan.TryParse(dto.EndTime, out var endTime))
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Geçersiz bitiş saati formatı. HH:mm formatı bekleniyor.");

        if (endTime <= startTime)
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Bitiş saati başlangıç saatinden sonra olmalıdır.");

        var entity = new AdminCalendarEvent
        {
            Title = dto.Title,
            Description = dto.Description,
            EventType = dto.EventType,
            EventDate = dto.EventDate.Date,
            StartTime = startTime,
            EndTime = endTime,
            Location = dto.Location
        };

        _context.AdminCalendarEvents.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = new AdminCalendarEventDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            EventType = entity.EventType,
            EventDate = entity.EventDate,
            StartTime = entity.StartTime.ToString(@"hh\:mm"),
            EndTime = entity.EndTime.ToString(@"hh\:mm"),
            Location = entity.Location,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        return ApiResponse<AdminCalendarEventDto>.SuccessResponse(resultDto, "Etkinlik başarıyla oluşturuldu.");
    }

    public async Task<ApiResponse<AdminCalendarEventDto>> UpdateAsync(int id, AdminCalendarEventUpdateDto dto)
    {
        var entity = await _context.AdminCalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Etkinlik bulunamadı.");

        if (!TimeSpan.TryParse(dto.StartTime, out var startTime))
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Geçersiz başlangıç saati formatı. HH:mm formatı bekleniyor.");

        if (!TimeSpan.TryParse(dto.EndTime, out var endTime))
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Geçersiz bitiş saati formatı. HH:mm formatı bekleniyor.");

        if (endTime <= startTime)
            return ApiResponse<AdminCalendarEventDto>.ErrorResponse("Bitiş saati başlangıç saatinden sonra olmalıdır.");

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.EventType = dto.EventType;
        entity.EventDate = dto.EventDate.Date;
        entity.StartTime = startTime;
        entity.EndTime = endTime;
        entity.Location = dto.Location;

        await _context.SaveChangesAsync();

        var resultDto = new AdminCalendarEventDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            EventType = entity.EventType,
            EventDate = entity.EventDate,
            StartTime = entity.StartTime.ToString(@"hh\:mm"),
            EndTime = entity.EndTime.ToString(@"hh\:mm"),
            Location = entity.Location,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        return ApiResponse<AdminCalendarEventDto>.SuccessResponse(resultDto, "Etkinlik başarıyla güncellendi.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _context.AdminCalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
            return ApiResponse<bool>.ErrorResponse("Etkinlik bulunamadı.");

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Etkinlik başarıyla silindi.");
    }
}
