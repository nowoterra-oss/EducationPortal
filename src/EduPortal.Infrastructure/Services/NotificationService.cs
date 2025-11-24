using EduPortal.Application.DTOs.Notification;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<NotificationDto> Items, int TotalCount)> GetAllPagedAsync(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto);

        return (dtos, totalCount);
    }

    public async Task<NotificationDto?> GetByIdAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

        return notification != null ? MapToDto(notification) : null;
    }

    public async Task<NotificationDto?> MarkAsReadAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

        if (notification == null)
            return null;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<int> MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        return unreadNotifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

        if (notification == null)
            return false;

        notification.IsDeleted = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<NotificationDto> SendAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            ActionUrl = dto.ActionUrl,
            ActionText = dto.ActionText,
            RelatedEntityId = dto.RelatedEntityId,
            RelatedEntityType = dto.RelatedEntityType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<int> SendBulkAsync(BulkNotificationDto dto)
    {
        var now = DateTime.UtcNow;
        var notifications = dto.UserIds.Select(userId => new Notification
        {
            UserId = userId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            ActionUrl = dto.ActionUrl,
            ActionText = dto.ActionText,
            IsRead = false,
            CreatedAt = now
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        return notifications.Count;
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            ActionUrl = notification.ActionUrl,
            ActionText = notification.ActionText,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            CreatedAt = notification.CreatedAt
        };
    }
}
