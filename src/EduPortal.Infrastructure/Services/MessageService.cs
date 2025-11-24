using EduPortal.Application.DTOs.Message;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetAllPagedAsync(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToSummaryDto);

        return (dtos, totalCount);
    }

    public async Task<MessageDto?> GetByIdAsync(int id, string userId)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Include(m => m.Replies)
            .FirstOrDefaultAsync(m => m.Id == id &&
                (m.SenderId == userId || m.RecipientId == userId));

        return message != null ? MapToDto(message) : null;
    }

    public async Task<MessageDto> SendAsync(string senderId, SendMessageDto dto)
    {
        var message = new Message
        {
            SenderId = senderId,
            RecipientId = dto.RecipientId,
            Subject = dto.Subject,
            Body = dto.Body,
            AttachmentUrl = dto.AttachmentUrl,
            ParentMessageId = dto.ParentMessageId,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(message.Id, senderId))!;
    }

    public async Task<MessageDto> ReplyAsync(int parentMessageId, string senderId, SendMessageDto dto)
    {
        var parentMessage = await _context.Messages.FindAsync(parentMessageId);
        if (parentMessage == null)
            throw new KeyNotFoundException("Yanıtlanacak mesaj bulunamadı");

        // Reply goes to the sender of the parent message
        var recipientId = parentMessage.SenderId == senderId
            ? parentMessage.RecipientId
            : parentMessage.SenderId;

        var replyDto = new SendMessageDto
        {
            RecipientId = recipientId,
            Subject = dto.Subject ?? $"Re: {parentMessage.Subject}",
            Body = dto.Body,
            AttachmentUrl = dto.AttachmentUrl,
            ParentMessageId = parentMessageId
        };

        return await SendAsync(senderId, replyDto);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id &&
                (m.SenderId == userId || m.RecipientId == userId));

        if (message == null)
            return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetInboxPagedAsync(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.RecipientId == userId)
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToSummaryDto);

        return (dtos, totalCount);
    }

    public async Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetSentPagedAsync(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToSummaryDto);

        return (dtos, totalCount);
    }

    public async Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> GetUnreadPagedAsync(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.RecipientId == userId && !m.IsRead)
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToSummaryDto);

        return (dtos, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Messages
            .CountAsync(m => m.RecipientId == userId && !m.IsRead);
    }

    public async Task<MessageDto?> MarkAsReadAsync(int id, string userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.RecipientId == userId);

        if (message == null)
            return null;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }

    public async Task<MessageDto?> MarkAsUnreadAsync(int id, string userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.RecipientId == userId);

        if (message == null)
            return null;

        message.IsRead = false;
        message.ReadAt = null;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }

    public async Task<(IEnumerable<MessageDto> Items, int TotalCount)> GetConversationPagedAsync(
        string currentUserId, string otherUserId, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m =>
                (m.SenderId == currentUserId && m.RecipientId == otherUserId) ||
                (m.SenderId == otherUserId && m.RecipientId == currentUserId))
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(m => MapToDto(m));

        return (dtos, totalCount);
    }

    public async Task<(IEnumerable<MessageSummaryDto> Items, int TotalCount)> SearchAsync(
        string userId, string searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m =>
                (m.SenderId == userId || m.RecipientId == userId) &&
                (m.Subject != null && m.Subject.Contains(searchTerm) ||
                 m.Body.Contains(searchTerm)))
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToSummaryDto);

        return (dtos, totalCount);
    }

    public async Task<int> SendBulkAsync(string senderId, BulkMessageDto dto)
    {
        var messages = dto.RecipientIds.Select(recipientId => new Message
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Subject = dto.Subject,
            Body = dto.Body,
            AttachmentUrl = dto.AttachmentUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false
        }).ToList();

        _context.Messages.AddRange(messages);
        await _context.SaveChangesAsync();

        return messages.Count;
    }

    private MessageDto MapToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
            SenderEmail = message.Sender.Email,
            RecipientId = message.RecipientId,
            RecipientName = $"{message.Recipient.FirstName} {message.Recipient.LastName}",
            RecipientEmail = message.Recipient.Email,
            Subject = message.Subject,
            Body = message.Body,
            IsRead = message.IsRead,
            SentAt = message.SentAt,
            ReadAt = message.ReadAt,
            AttachmentUrl = message.AttachmentUrl,
            ParentMessageId = message.ParentMessageId,
            ReplyCount = message.Replies?.Count ?? 0
        };
    }

    private MessageSummaryDto MapToSummaryDto(Message message)
    {
        return new MessageSummaryDto
        {
            Id = message.Id,
            SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
            RecipientName = $"{message.Recipient.FirstName} {message.Recipient.LastName}",
            Subject = message.Subject,
            BodyPreview = message.Body.Length > 100
                ? message.Body.Substring(0, 100) + "..."
                : message.Body,
            IsRead = message.IsRead,
            SentAt = message.SentAt,
            HasAttachment = !string.IsNullOrEmpty(message.AttachmentUrl),
            ReplyCount = _context.Messages.Count(m => m.ParentMessageId == message.Id)
        };
    }
}
