using EduPortal.Application.DTOs.Message;

namespace EduPortal.Application.Interfaces;

public interface IMessageService
{
    // Core CRUD
    Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetAllPagedAsync(
        string userId, int pageNumber, int pageSize);
    Task<MessageDto?> GetByIdAsync(int id, string userId);
    Task<MessageDto> SendAsync(string senderId, SendMessageDto dto);
    Task<MessageDto> ReplyAsync(int parentMessageId, string senderId, SendMessageDto dto);
    Task<bool> DeleteAsync(int id, string userId);

    // Inbox/Sent/Unread
    Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetInboxPagedAsync(
        string userId, int pageNumber, int pageSize);
    Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetSentPagedAsync(
        string userId, int pageNumber, int pageSize);
    Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetUnreadPagedAsync(
        string userId, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(string userId);

    // Read status
    Task<MessageDto?> MarkAsReadAsync(int id, string userId);
    Task<MessageDto?> MarkAsUnreadAsync(int id, string userId);

    // Conversation
    Task<(IEnumerable<MessageDto> Items, int TotalCount)> GetConversationPagedAsync(
        string currentUserId, string otherUserId, int pageNumber, int pageSize);

    // Search
    Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> SearchAsync(
        string userId, string searchTerm, int pageNumber, int pageSize);

    // Bulk operations
    Task<int> SendBulkAsync(string senderId, BulkMessageDto dto);
}
