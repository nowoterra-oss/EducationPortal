using EduPortal.Application.DTOs.Notification;

namespace EduPortal.Application.Interfaces;

public interface INotificationService
{
    Task<(IEnumerable<NotificationDto> Items, int TotalCount)> GetAllPagedAsync(
        string userId, int pageNumber, int pageSize);
    Task<NotificationDto?> GetByIdAsync(int id, string userId);
    Task<NotificationDto?> MarkAsReadAsync(int id, string userId);
    Task<int> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<NotificationDto> SendAsync(CreateNotificationDto dto);
    Task<int> SendBulkAsync(BulkNotificationDto dto);
}
