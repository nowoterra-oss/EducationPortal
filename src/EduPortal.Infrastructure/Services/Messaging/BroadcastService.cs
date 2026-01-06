using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Entities.Messaging;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Toplu mesaj servisi implementasyonu - Admin icin
/// </summary>
public class BroadcastService : IBroadcastService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMessageEncryptionService _encryptionService;
    private readonly IContentModerationService _moderationService;
    private readonly IMessagingAuthorizationService _authorizationService;

    // Broadcast mesajlar icin ozel conversation ID
    private const int BroadcastConversationId = -1;

    public BroadcastService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IMessageEncryptionService encryptionService,
        IContentModerationService moderationService,
        IMessagingAuthorizationService authorizationService)
    {
        _context = context;
        _userManager = userManager;
        _encryptionService = encryptionService;
        _moderationService = moderationService;
        _authorizationService = authorizationService;
    }

    public async Task<BroadcastMessageDto> SendBroadcastAsync(CreateBroadcastMessageDto dto, string senderUserId)
    {
        // Yetki kontrolu
        var (canBroadcast, reason) = await _authorizationService.CanSendBroadcastAsync(senderUserId);
        if (!canBroadcast)
        {
            throw new InvalidOperationException(reason ?? "Toplu mesaj gönderme yetkiniz yok.");
        }

        // Icerik moderasyonu
        var moderationResult = _moderationService.ValidateContent(dto.Content);
        if (!moderationResult.IsValid)
        {
            throw new InvalidOperationException(moderationResult.BlockedReason ?? "Mesajınız uygunsuz içerik barındırmaktadır.");
        }

        // Hedef kitleyi bul
        var recipientUserIds = await GetAudienceUserIdsAsync(dto.TargetAudience);

        if (!recipientUserIds.Any())
        {
            throw new InvalidOperationException("Hedef kitle boş.");
        }

        // Mesaji sifrele
        var (encryptedContent, contentHash) = _encryptionService.Encrypt(dto.Content, BroadcastConversationId);

        var broadcast = new BroadcastMessage
        {
            SenderId = senderUserId,
            TargetAudience = dto.TargetAudience,
            Title = dto.Title,
            ContentEncrypted = encryptedContent,
            ContentHash = contentHash,
            SentAt = DateTime.UtcNow,
            ExpiresAt = dto.ExpiresAt,
            Priority = dto.Priority,
            RecipientCount = recipientUserIds.Count,
            CreatedAt = DateTime.UtcNow
        };

        _context.BroadcastMessages.Add(broadcast);
        await _context.SaveChangesAsync();

        // Alicilari ekle
        foreach (var userId in recipientUserIds)
        {
            _context.BroadcastMessageRecipients.Add(new BroadcastMessageRecipient
            {
                BroadcastMessageId = broadcast.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return await GetBroadcastMessageAsync(broadcast.Id, senderUserId) ?? throw new InvalidOperationException("Mesaj oluşturulamadı.");
    }

    public async Task<BroadcastMessageDto> SendDirectBroadcastAsync(string recipientUserId, string title, string content, string senderUserId)
    {
        // Yetki kontrolu
        var (canBroadcast, reason) = await _authorizationService.CanSendBroadcastAsync(senderUserId);
        if (!canBroadcast)
        {
            throw new InvalidOperationException(reason ?? "Toplu mesaj gönderme yetkiniz yok.");
        }

        // Alici var mi?
        var recipient = await _userManager.FindByIdAsync(recipientUserId);
        if (recipient == null)
        {
            throw new InvalidOperationException("Alıcı bulunamadı.");
        }

        // Icerik moderasyonu
        var moderationResult = _moderationService.ValidateContent(content);
        if (!moderationResult.IsValid)
        {
            throw new InvalidOperationException(moderationResult.BlockedReason ?? "Mesajınız uygunsuz içerik barındırmaktadır.");
        }

        // Mesaji sifrele
        var (encryptedContent, contentHash) = _encryptionService.Encrypt(content, BroadcastConversationId);

        var broadcast = new BroadcastMessage
        {
            SenderId = senderUserId,
            TargetAudience = BroadcastTargetAudience.None, // Bireysel
            Title = title,
            ContentEncrypted = encryptedContent,
            ContentHash = contentHash,
            SentAt = DateTime.UtcNow,
            Priority = Priority.Normal,
            RecipientCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.BroadcastMessages.Add(broadcast);
        await _context.SaveChangesAsync();

        // Aliciyi ekle
        _context.BroadcastMessageRecipients.Add(new BroadcastMessageRecipient
        {
            BroadcastMessageId = broadcast.Id,
            UserId = recipientUserId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return await GetBroadcastMessageAsync(broadcast.Id, senderUserId) ?? throw new InvalidOperationException("Mesaj oluşturulamadı.");
    }

    public async Task<List<BroadcastMessageListDto>> GetBroadcastMessagesAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Admin kontrolu - Admin tum broadcast'leri gormeli
        var user = await _userManager.FindByIdAsync(userId);
        var userRoles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
        var isAdmin = userRoles.Contains("Admin");

        if (isAdmin)
        {
            // Admin icin: Tum broadcast'leri getir (gonderen veya alici olarak)
            var adminMessages = await _context.BroadcastMessages
                .Include(b => b.Sender)
                .Where(b => b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(b => b.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return adminMessages.Select(b =>
            {
                var decrypted = _encryptionService.Decrypt(b.ContentEncrypted, BroadcastConversationId);

                return new BroadcastMessageListDto
                {
                    Id = b.Id,
                    SenderName = $"{b.Sender?.FirstName} {b.Sender?.LastName}".Trim(),
                    TargetAudienceDisplay = GetAudienceDisplayName(b.TargetAudience),
                    Title = b.Title,
                    ContentPreview = decrypted.Length > 100 ? decrypted.Substring(0, 97) + "..." : decrypted,
                    SentAt = b.SentAt,
                    Priority = b.Priority,
                    IsRead = b.SenderId == userId, // Gonderen icin okunmus say
                    RecipientCount = b.RecipientCount,
                    ReadCount = b.ReadCount
                };
            }).ToList();
        }

        // Normal kullanicilar icin: Sadece alici olduklari broadcast'leri getir
        var messages = await _context.BroadcastMessageRecipients
            .Include(r => r.BroadcastMessage)
                .ThenInclude(b => b.Sender)
            .Where(r => r.UserId == userId && !r.IsDeletedByUser)
            .Where(r => r.BroadcastMessage.ExpiresAt == null || r.BroadcastMessage.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(r => r.BroadcastMessage.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(r =>
        {
            var decrypted = _encryptionService.Decrypt(r.BroadcastMessage.ContentEncrypted, BroadcastConversationId);

            return new BroadcastMessageListDto
            {
                Id = r.BroadcastMessage.Id,
                SenderName = $"{r.BroadcastMessage.Sender?.FirstName} {r.BroadcastMessage.Sender?.LastName}".Trim(),
                TargetAudienceDisplay = GetAudienceDisplayName(r.BroadcastMessage.TargetAudience),
                Title = r.BroadcastMessage.Title,
                ContentPreview = decrypted.Length > 100 ? decrypted.Substring(0, 97) + "..." : decrypted,
                SentAt = r.BroadcastMessage.SentAt,
                Priority = r.BroadcastMessage.Priority,
                IsRead = r.IsRead,
                RecipientCount = r.BroadcastMessage.RecipientCount,
                ReadCount = r.BroadcastMessage.ReadCount
            };
        }).ToList();
    }

    public async Task<BroadcastMessageDto?> GetBroadcastMessageAsync(int messageId, string userId)
    {
        var broadcast = await _context.BroadcastMessages
            .Include(b => b.Sender)
            .FirstOrDefaultAsync(b => b.Id == messageId);

        if (broadcast == null)
        {
            return null;
        }

        // Kullanici alici mi veya gonderen mi?
        var recipient = await _context.BroadcastMessageRecipients
            .FirstOrDefaultAsync(r => r.BroadcastMessageId == messageId && r.UserId == userId);

        var sender = await _userManager.FindByIdAsync(userId);
        var senderRoles = sender != null ? await _userManager.GetRolesAsync(sender) : new List<string>();
        var isAdmin = senderRoles.Contains("Admin");

        if (recipient == null && broadcast.SenderId != userId && !isAdmin)
        {
            return null;
        }

        var decrypted = _encryptionService.Decrypt(broadcast.ContentEncrypted, BroadcastConversationId);

        return new BroadcastMessageDto
        {
            Id = broadcast.Id,
            SenderId = broadcast.SenderId,
            SenderName = $"{broadcast.Sender?.FirstName} {broadcast.Sender?.LastName}".Trim(),
            SenderPhoto = broadcast.Sender?.ProfilePhotoUrl,
            TargetAudience = broadcast.TargetAudience,
            TargetAudienceDisplay = GetAudienceDisplayName(broadcast.TargetAudience),
            Title = broadcast.Title,
            Content = decrypted,
            SentAt = broadcast.SentAt,
            ExpiresAt = broadcast.ExpiresAt,
            Priority = broadcast.Priority,
            RecipientCount = broadcast.RecipientCount,
            ReadCount = broadcast.ReadCount,
            IsRead = recipient?.IsRead ?? false,
            ReadAt = recipient?.ReadAt
        };
    }

    public async Task MarkBroadcastAsReadAsync(int messageId, string userId)
    {
        var recipient = await _context.BroadcastMessageRecipients
            .FirstOrDefaultAsync(r => r.BroadcastMessageId == messageId && r.UserId == userId);

        if (recipient == null)
        {
            return;
        }

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow;
            recipient.UpdatedAt = DateTime.UtcNow;

            // ReadCount'u guncelle
            var broadcast = await _context.BroadcastMessages.FindAsync(messageId);
            if (broadcast != null)
            {
                broadcast.ReadCount++;
                broadcast.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<BroadcastMessageListDto>> GetSentBroadcastsAsync(string adminUserId, int page = 1, int pageSize = 20)
    {
        var messages = await _context.BroadcastMessages
            .Include(b => b.Sender)
            .Where(b => b.SenderId == adminUserId)
            .OrderByDescending(b => b.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(b =>
        {
            var decrypted = _encryptionService.Decrypt(b.ContentEncrypted, BroadcastConversationId);

            return new BroadcastMessageListDto
            {
                Id = b.Id,
                SenderName = $"{b.Sender?.FirstName} {b.Sender?.LastName}".Trim(),
                TargetAudienceDisplay = GetAudienceDisplayName(b.TargetAudience),
                Title = b.Title,
                ContentPreview = decrypted.Length > 100 ? decrypted.Substring(0, 97) + "..." : decrypted,
                SentAt = b.SentAt,
                Priority = b.Priority,
                IsRead = false, // Gonderici icin anlamli degil
                RecipientCount = b.RecipientCount,
                ReadCount = b.ReadCount
            };
        }).ToList();
    }

    public async Task<int> GetAudienceCountAsync(BroadcastTargetAudience audience)
    {
        var userIds = await GetAudienceUserIdsAsync(audience);
        return userIds.Count;
    }

    public async Task<int> GetUnreadBroadcastCountAsync(string userId)
    {
        return await _context.BroadcastMessageRecipients
            .Where(r => r.UserId == userId && !r.IsRead && !r.IsDeletedByUser)
            .Where(r => r.BroadcastMessage.ExpiresAt == null || r.BroadcastMessage.ExpiresAt > DateTime.UtcNow)
            .CountAsync();
    }

    #region Private Helpers

    private async Task<List<string>> GetAudienceUserIdsAsync(BroadcastTargetAudience audience)
    {
        var userIds = new HashSet<string>();

        // Tum kullanicilar
        if (audience.HasFlag(BroadcastTargetAudience.All))
        {
            var allUsers = await _userManager.Users.Select(u => u.Id).ToListAsync();
            return allUsers;
        }

        // Ogrenciler
        if (audience.HasFlag(BroadcastTargetAudience.Students))
        {
            var students = await _userManager.GetUsersInRoleAsync("Ogrenci");
            foreach (var s in students)
            {
                userIds.Add(s.Id);
            }
        }

        // Ogretmenler
        if (audience.HasFlag(BroadcastTargetAudience.Teachers))
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Ogretmen");
            foreach (var t in teachers)
            {
                userIds.Add(t.Id);
            }
        }

        // Veliler
        if (audience.HasFlag(BroadcastTargetAudience.Parents))
        {
            var parents = await _userManager.GetUsersInRoleAsync("Veli");
            foreach (var p in parents)
            {
                userIds.Add(p.Id);
            }
        }

        // Danismanlar
        if (audience.HasFlag(BroadcastTargetAudience.Counselors))
        {
            var counselors = await _userManager.GetUsersInRoleAsync("Danışman");
            foreach (var c in counselors)
            {
                userIds.Add(c.Id);
            }
        }

        // Kayitcilar
        if (audience.HasFlag(BroadcastTargetAudience.Registrars))
        {
            var registrars = await _userManager.GetUsersInRoleAsync("Kayitci");
            foreach (var r in registrars)
            {
                userIds.Add(r.Id);
            }
        }

        return userIds.ToList();
    }

    private static string GetAudienceDisplayName(BroadcastTargetAudience audience)
    {
        if (audience == BroadcastTargetAudience.None)
        {
            return "Bireysel";
        }

        if (audience == BroadcastTargetAudience.All)
        {
            return "Herkes";
        }

        var parts = new List<string>();

        if (audience.HasFlag(BroadcastTargetAudience.Students))
            parts.Add("Öğrenciler");

        if (audience.HasFlag(BroadcastTargetAudience.Teachers))
            parts.Add("Öğretmenler");

        if (audience.HasFlag(BroadcastTargetAudience.Parents))
            parts.Add("Veliler");

        if (audience.HasFlag(BroadcastTargetAudience.Counselors))
            parts.Add("Danışmanlar");

        if (audience.HasFlag(BroadcastTargetAudience.Registrars))
            parts.Add("Kayıtçılar");

        return string.Join(", ", parts);
    }

    #endregion
}
