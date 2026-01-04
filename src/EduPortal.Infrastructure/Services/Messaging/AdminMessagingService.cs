using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Entities.Messaging;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Admin mesajlasma servisi - konusmalari okuma ve loglama
/// </summary>
public class AdminMessagingService : IAdminMessagingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMessageEncryptionService _encryptionService;
    private readonly ILogger<AdminMessagingService> _logger;

    public AdminMessagingService(
        ApplicationDbContext context,
        IMessageEncryptionService encryptionService,
        ILogger<AdminMessagingService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<AdminConversationDetailDto> ReadConversationAsync(
        AdminReadConversationDto dto,
        string adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants.Where(p => p.LeftAt == null))
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);

        if (conversation == null)
        {
            throw new InvalidOperationException("Konuşma bulunamadı.");
        }

        // Erisim logu olustur
        var accessLog = new AdminMessageAccessLog
        {
            AdminUserId = adminUserId,
            ConversationId = dto.ConversationId,
            AccessedAt = DateTime.UtcNow,
            Reason = dto.Reason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DecryptedMessageCount = conversation.Messages.Count,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminMessageAccessLogs.Add(accessLog);
        await _context.SaveChangesAsync();

        _logger.LogWarning(
            "Admin {AdminUserId} accessed conversation {ConversationId}. Reason: {Reason}. IP: {IpAddress}",
            adminUserId, dto.ConversationId, dto.Reason, ipAddress);

        // Mesajlari decrypt et
        var messages = conversation.Messages.Select(m => new AdminChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = $"{m.Sender?.FirstName} {m.Sender?.LastName}".Trim(),
            Content = m.IsDeleted
                ? "[Silindi]"
                : _encryptionService.Decrypt(m.ContentEncrypted, conversation.Id),
            SentAt = m.SentAt,
            IsEdited = m.IsEdited,
            IsDeleted = m.IsDeleted,
            DeletedAt = m.DeletedAt,
            DeletedBy = m.DeletedBy
        }).ToList();

        return new AdminConversationDetailDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt,
            Participants = conversation.Participants.Select(p => new ConversationParticipantDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = $"{p.User?.FirstName} {p.User?.LastName}".Trim(),
                UserPhoto = p.User?.ProfilePhotoUrl,
                Role = p.Role,
                JoinedAt = p.JoinedAt,
                LastReadAt = p.LastReadAt
            }).ToList(),
            Messages = messages,
            TotalMessageCount = messages.Count
        };
    }

    public async Task<List<AdminMessageAccessLogDto>> GetAccessLogsAsync(
        int page = 1,
        int pageSize = 50,
        string? adminUserId = null,
        int? conversationId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.AdminMessageAccessLogs
            .Include(l => l.AdminUser)
            .Include(l => l.Conversation)
                .ThenInclude(c => c.Participants)
                    .ThenInclude(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(adminUserId))
        {
            query = query.Where(l => l.AdminUserId == adminUserId);
        }

        if (conversationId.HasValue)
        {
            query = query.Where(l => l.ConversationId == conversationId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.AccessedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.AccessedAt <= endDate.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.AccessedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return logs.Select(l => new AdminMessageAccessLogDto
        {
            Id = l.Id,
            AdminUserId = l.AdminUserId,
            AdminUserName = $"{l.AdminUser?.FirstName} {l.AdminUser?.LastName}".Trim(),
            ConversationId = l.ConversationId,
            ConversationTitle = l.Conversation?.Title ?? $"Konuşma #{l.ConversationId}",
            ConversationParticipants = l.Conversation?.Participants
                .Where(p => p.LeftAt == null)
                .Select(p => $"{p.User?.FirstName} {p.User?.LastName}".Trim())
                .ToList() ?? new List<string>(),
            MessageId = l.MessageId,
            AccessedAt = l.AccessedAt,
            Reason = l.Reason,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            DecryptedMessageCount = l.DecryptedMessageCount
        }).ToList();
    }

    public async Task<List<ConversationListDto>> GetAllConversationsAsync(
        int page = 1,
        int pageSize = 50,
        string? searchTerm = null)
    {
        var query = _context.Conversations
            .Include(c => c.Participants.Where(p => p.LeftAt == null))
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(c =>
                (c.Title != null && c.Title.ToLower().Contains(searchLower)) ||
                c.Participants.Any(p =>
                    (p.User != null && (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(searchLower)) ||
                    (p.User != null && p.User.Email != null && p.User.Email.ToLower().Contains(searchLower))));
        }

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return conversations.Select(c =>
        {
            var lastMessage = c.Messages.FirstOrDefault();
            var participantNames = c.Participants
                .Where(p => p.LeftAt == null && p.User != null)
                .Select(p => $"{p.User!.FirstName} {p.User.LastName}")
                .ToList();

            return new ConversationListDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                DisplayName = c.Title ?? string.Join(", ", participantNames.Take(3)),
                LastMessageAt = c.LastMessageAt,
                LastMessagePreview = lastMessage != null
                    ? "[Şifreli - Admin erişimi gerekli]"
                    : null,
                ParticipantCount = c.Participants.Count(p => p.LeftAt == null)
            };
        }).ToList();
    }

    public async Task<List<ConversationListDto>> GetUserConversationsAsync(string userId, int page = 1, int pageSize = 50)
    {
        var conversations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants.Where(p => p.LeftAt == null))
                    .ThenInclude(p => p.User)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .OrderByDescending(cp => cp.Conversation.LastMessageAt ?? cp.Conversation.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cp => cp.Conversation)
            .ToListAsync();

        return conversations.Select(c =>
        {
            var lastMessage = c.Messages.FirstOrDefault();
            var participantNames = c.Participants
                .Where(p => p.LeftAt == null && p.User != null)
                .Select(p => $"{p.User!.FirstName} {p.User.LastName}")
                .ToList();

            return new ConversationListDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                DisplayName = c.Title ?? string.Join(", ", participantNames.Take(3)),
                LastMessageAt = c.LastMessageAt,
                LastMessagePreview = lastMessage != null
                    ? "[Şifreli - Admin erişimi gerekli]"
                    : null,
                ParticipantCount = c.Participants.Count(p => p.LeftAt == null)
            };
        }).ToList();
    }

    public async Task<int> ArchiveOldMessagesAsync(DateTime olderThan)
    {
        var messagesToArchive = await _context.ChatMessages
            .Where(m => m.SentAt < olderThan)
            .ToListAsync();

        var archivedCount = 0;

        foreach (var message in messagesToArchive)
        {
            var archive = new MessageArchive
            {
                OriginalConversationId = message.ConversationId,
                OriginalMessageId = message.Id,
                SenderId = message.SenderId,
                ContentEncrypted = message.ContentEncrypted,
                ContentHash = message.ContentHash,
                OriginalSentAt = message.SentAt,
                ArchivedAt = DateTime.UtcNow,
                ArchiveReason = "Auto-archived: older than 1 year",
                CreatedAt = DateTime.UtcNow
            };

            _context.MessageArchives.Add(archive);
            _context.ChatMessages.Remove(message);
            archivedCount++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived {Count} messages older than {Date}", archivedCount, olderThan);

        return archivedCount;
    }

    public async Task<int> CleanupOldAccessLogsAsync(DateTime olderThan)
    {
        var logsToDelete = await _context.AdminMessageAccessLogs
            .Where(l => l.AccessedAt < olderThan)
            .ToListAsync();

        _context.AdminMessageAccessLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} access logs older than {Date}", logsToDelete.Count, olderThan);

        return logsToDelete.Count;
    }
}
