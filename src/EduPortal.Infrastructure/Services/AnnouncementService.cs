using EduPortal.Application.DTOs.Announcement;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;

    public AnnouncementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<AnnouncementDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, AnnouncementType? type = null)
    {
        var query = _context.Announcements
            .Include(a => a.Publisher)
            .AsNoTracking();

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<AnnouncementDto?> GetByIdAsync(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Publisher)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return announcement == null ? null : MapToDto(announcement);
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto dto, string userId)
    {
        var announcement = new Announcement
        {
            Title = dto.Title,
            Content = dto.Content,
            Type = dto.Type,
            PublishedBy = userId,
            PublishedDate = DateTime.UtcNow,
            ExpiryDate = dto.ExpiryDate,
            IsActive = dto.IsActive,
            IsPinned = dto.IsPinned,
            AttachmentUrl = dto.AttachmentUrl,
            TargetAudience = dto.TargetAudience,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        var created = await _context.Announcements
            .Include(a => a.Publisher)
            .FirstOrDefaultAsync(a => a.Id == announcement.Id);

        return MapToDto(created!);
    }

    public async Task<AnnouncementDto> UpdateAsync(int id, UpdateAnnouncementDto dto)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Publisher)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
            throw new KeyNotFoundException("Duyuru bulunamadı");

        announcement.Title = dto.Title;
        announcement.Content = dto.Content;
        announcement.Type = dto.Type;
        announcement.ExpiryDate = dto.ExpiryDate;
        announcement.IsActive = dto.IsActive;
        announcement.IsPinned = dto.IsPinned;
        announcement.AttachmentUrl = dto.AttachmentUrl;
        announcement.TargetAudience = dto.TargetAudience;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(announcement);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
            return false;

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(IEnumerable<AnnouncementDto> Items, int TotalCount)> GetActiveAsync(int pageNumber, int pageSize)
    {
        var now = DateTime.UtcNow;
        var query = _context.Announcements
            .Include(a => a.Publisher)
            .AsNoTracking()
            .Where(a => a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > now));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.PublishedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<AnnouncementDto>> GetPinnedAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Include(a => a.Publisher)
            .AsNoTracking()
            .Where(a => a.IsPinned && a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > now))
            .OrderByDescending(a => a.PublishedDate)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<AnnouncementDto> PinAsync(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Publisher)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
            throw new KeyNotFoundException("Duyuru bulunamadı");

        announcement.IsPinned = true;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(announcement);
    }

    public async Task<AnnouncementDto> UnpinAsync(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Publisher)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
            throw new KeyNotFoundException("Duyuru bulunamadı");

        announcement.IsPinned = false;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(announcement);
    }

    private static AnnouncementDto MapToDto(Announcement a)
    {
        return new AnnouncementDto
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            Type = a.Type,
            TypeName = GetTypeName(a.Type),
            PublishedBy = a.PublishedBy,
            PublisherName = a.Publisher != null ? $"{a.Publisher.FirstName} {a.Publisher.LastName}" : string.Empty,
            PublishedDate = a.PublishedDate,
            ExpiryDate = a.ExpiryDate,
            IsActive = a.IsActive,
            IsPinned = a.IsPinned,
            AttachmentUrl = a.AttachmentUrl,
            TargetAudience = a.TargetAudience,
            ViewCount = a.ViewCount,
            CreatedAt = a.CreatedAt
        };
    }

    private static string GetTypeName(AnnouncementType type)
    {
        return type switch
        {
            AnnouncementType.General => "Genel",
            AnnouncementType.Urgent => "Acil",
            AnnouncementType.Academic => "Akademik",
            AnnouncementType.Event => "Etkinlik",
            AnnouncementType.Holiday => "Tatil",
            _ => "Bilinmiyor"
        };
    }
}
