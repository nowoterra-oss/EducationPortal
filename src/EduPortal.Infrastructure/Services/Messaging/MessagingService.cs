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
/// Ana mesajlasma servisi implementasyonu
/// </summary>
public class MessagingService : IMessagingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMessageEncryptionService _encryptionService;
    private readonly IContentModerationService _moderationService;
    private readonly IMessagingAuthorizationService _authorizationService;
    private readonly IOnlineStatusService _onlineStatusService;

    public MessagingService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IMessageEncryptionService encryptionService,
        IContentModerationService moderationService,
        IMessagingAuthorizationService authorizationService,
        IOnlineStatusService onlineStatusService)
    {
        _context = context;
        _userManager = userManager;
        _encryptionService = encryptionService;
        _moderationService = moderationService;
        _authorizationService = authorizationService;
        _onlineStatusService = onlineStatusService;
    }

    #region Conversations

    public async Task<List<ConversationListDto>> GetConversationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var conversations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId && cp.LeftAt == null)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants.Where(p => p.LeftAt == null))
                    .ThenInclude(p => p.User)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.StudentGroup)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Course)
            .OrderByDescending(cp => cp.IsPinned)
            .ThenByDescending(cp => cp.Conversation.LastMessageAt ?? cp.Conversation.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<ConversationListDto>();

        foreach (var cp in conversations)
        {
            var conv = cp.Conversation;
            var lastMessage = conv.Messages.FirstOrDefault();

            // Okunmamis mesaj sayisi
            var unreadCount = await _context.ChatMessages
                .CountAsync(m => m.ConversationId == conv.Id &&
                                m.SentAt > (cp.LastReadAt ?? DateTime.MinValue) &&
                                m.SenderId != userId);

            // Direct konusmada karsi tarafin bilgilerini al
            var displayName = conv.Title;
            string? displayPhoto = null;

            if (conv.Type == ConversationType.Direct)
            {
                var otherParticipant = conv.Participants.FirstOrDefault(p => p.UserId != userId);
                if (otherParticipant?.User != null)
                {
                    var firstName = otherParticipant.User.FirstName ?? "";
                    var lastName = otherParticipant.User.LastName ?? "";
                    displayName = $"{firstName} {lastName}".Trim();
                    displayPhoto = otherParticipant.User.ProfilePhotoUrl;
                }
            }
            else if (conv.Type == ConversationType.StudentGroup)
            {
                displayName = conv.StudentGroup?.Name ?? conv.Title;
            }
            else if (conv.Type == ConversationType.CourseGroup)
            {
                displayName = conv.Course?.CourseName ?? conv.Title;
            }

            // Yaziyor gostergesi
            var typingUser = conv.Participants
                .FirstOrDefault(p => p.UserId != userId && p.IsTyping &&
                                    p.LastTypingAt > DateTime.UtcNow.AddSeconds(-5));

            // Katilimci fotograflarini al (kendisi haric, en fazla 4 tane)
            var participantPhotos = conv.Participants
                .Where(p => p.LeftAt == null && p.UserId != userId && !string.IsNullOrEmpty(p.User?.ProfilePhotoUrl))
                .Take(4)
                .Select(p => p.User!.ProfilePhotoUrl!)
                .ToList();

            result.Add(new ConversationListDto
            {
                Id = conv.Id,
                Type = conv.Type,
                Title = conv.Title,
                DisplayName = displayName ?? $"Konuşma #{conv.Id}",
                DisplayPhoto = displayPhoto,
                ParticipantPhotos = participantPhotos,
                LastMessageAt = conv.LastMessageAt,
                LastMessagePreview = lastMessage != null
                    ? TruncateText(_encryptionService.Decrypt(lastMessage.ContentEncrypted, conv.Id), 100)
                    : null,
                LastMessageSenderName = lastMessage?.Sender != null
                    ? $"{lastMessage.Sender.FirstName} {lastMessage.Sender.LastName}"
                    : null,
                UnreadCount = unreadCount,
                IsMuted = cp.IsMuted,
                IsPinned = cp.IsPinned,
                IsTyping = typingUser != null,
                TypingUserName = typingUser != null
                    ? $"{typingUser.User?.FirstName}"
                    : null,
                ParticipantCount = conv.Participants.Count(p => p.LeftAt == null)
            });
        }

        return result;
    }

    public async Task<ConversationDto?> GetConversationAsync(int conversationId, string userId)
    {
        var participant = await _context.ConversationParticipants
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants.Where(p => p.LeftAt == null))
                    .ThenInclude(p => p.User)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Course)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.StudentGroup)
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId &&
                                      cp.UserId == userId &&
                                      cp.LeftAt == null);

        if (participant == null)
        {
            return null;
        }

        var conv = participant.Conversation;

        // Son mesaji al
        var lastMessage = await _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();

        // Okunmamis mesaj sayisi
        var unreadCount = await _context.ChatMessages
            .CountAsync(m => m.ConversationId == conversationId &&
                            m.SentAt > (participant.LastReadAt ?? DateTime.MinValue) &&
                            m.SenderId != userId);

        // Direct konusmada karsi tarafin bilgilerini al
        var displayName = conv.Title;
        string? displayPhoto = null;

        if (conv.Type == ConversationType.Direct)
        {
            var otherParticipant = conv.Participants.FirstOrDefault(p => p.UserId != userId);
            if (otherParticipant?.User != null)
            {
                var firstName = otherParticipant.User.FirstName ?? "";
                var lastName = otherParticipant.User.LastName ?? "";
                displayName = $"{firstName} {lastName}".Trim();
                displayPhoto = otherParticipant.User.ProfilePhotoUrl;
            }
        }
        else if (conv.Type == ConversationType.StudentGroup)
        {
            displayName = conv.StudentGroup?.Name ?? conv.Title;
        }
        else if (conv.Type == ConversationType.CourseGroup)
        {
            displayName = conv.Course?.CourseName ?? conv.Title;
        }

        return new ConversationDto
        {
            Id = conv.Id,
            Type = conv.Type,
            Title = conv.Title,
            DisplayName = displayName ?? $"Konuşma #{conv.Id}",
            DisplayPhoto = displayPhoto,
            CourseId = conv.CourseId,
            CourseName = conv.Course?.CourseName,
            StudentGroupId = conv.StudentGroupId,
            StudentGroupName = conv.StudentGroup?.Name,
            LastMessageAt = conv.LastMessageAt,
            MaxParticipants = conv.MaxParticipants,
            CreatedAt = conv.CreatedAt,
            UnreadCount = unreadCount,
            IsMuted = participant.IsMuted,
            IsPinned = participant.IsPinned,
            LastMessage = lastMessage != null ? MapToMessageDto(lastMessage, userId) : null,
            Participants = conv.Participants
                .Where(p => p.LeftAt == null)
                .Select(p => new ConversationParticipantDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = $"{p.User?.FirstName} {p.User?.LastName}".Trim(),
                    UserPhoto = p.User?.ProfilePhotoUrl,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt,
                    LastReadAt = p.LastReadAt,
                    IsTyping = p.IsTyping && p.LastTypingAt > DateTime.UtcNow.AddSeconds(-5),
                    IsOnline = _onlineStatusService.IsUserOnline(p.UserId)
                }).ToList()
        };
    }

    public async Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto, string creatorUserId)
    {
        // Yetki kontrolu
        foreach (var participantId in dto.ParticipantUserIds)
        {
            var (canMessage, reason) = await _authorizationService.CanMessageUserAsync(creatorUserId, participantId);
            if (!canMessage)
            {
                throw new InvalidOperationException(reason ?? "Bu kullanıcıya mesaj atamazsınız.");
            }
        }

        // Katilimci sayisi kontrolu
        var totalParticipants = dto.ParticipantUserIds.Count + 1; // +1 for creator
        if (totalParticipants > 10)
        {
            throw new InvalidOperationException("Bir konuşmada en fazla 10 kişi olabilir.");
        }

        var conversation = new Conversation
        {
            Type = dto.Type,
            Title = dto.Title,
            CourseId = dto.CourseId,
            StudentGroupId = dto.StudentGroupId,
            MaxParticipants = 10,
            CreatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Sifreleme anahtari olustur
        conversation.EncryptionKeyHash = _encryptionService.GenerateConversationKey(conversation.Id);

        // Olusturucuyu ekle
        var creatorParticipant = new ConversationParticipant
        {
            ConversationId = conversation.Id,
            UserId = creatorUserId,
            Role = ConversationParticipantRole.Owner,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.ConversationParticipants.Add(creatorParticipant);

        // Diger katilimcilari ekle
        foreach (var participantId in dto.ParticipantUserIds)
        {
            var participant = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = participantId,
                Role = ConversationParticipantRole.Participant,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.ConversationParticipants.Add(participant);
        }

        await _context.SaveChangesAsync();

        return (await GetConversationAsync(conversation.Id, creatorUserId))!;
    }

    public async Task<ConversationDto> GetOrCreateDirectConversationAsync(string userId1, string userId2)
    {
        // Mevcut konusmayi bul
        var existingConversation = await _context.Conversations
            .Where(c => c.Type == ConversationType.Direct)
            .Where(c => c.Participants.Any(p => p.UserId == userId1 && p.LeftAt == null))
            .Where(c => c.Participants.Any(p => p.UserId == userId2 && p.LeftAt == null))
            .Where(c => c.Participants.Count(p => p.LeftAt == null) == 2)
            .FirstOrDefaultAsync();

        if (existingConversation != null)
        {
            return (await GetConversationAsync(existingConversation.Id, userId1))!;
        }

        // Yeni konusma olustur
        return await CreateConversationAsync(new CreateConversationDto
        {
            Type = ConversationType.Direct,
            ParticipantUserIds = new List<string> { userId2 }
        }, userId1);
    }

    public async Task<ConversationDto> GetOrCreateGroupConversationAsync(int groupId, string userId)
    {
        // Gruba erisim yetkisi kontrolu
        var (canMessage, reason) = await _authorizationService.CanMessageGroupAsync(userId, groupId);
        if (!canMessage)
        {
            throw new InvalidOperationException(reason ?? "Bu gruba mesaj atma yetkiniz yok.");
        }

        // Grup bilgisini al
        var group = await _context.StudentGroups
            .Include(g => g.Members.Where(m => m.IsActive && !m.IsDeleted))
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("Grup bulunamadı.");
        }

        // Mevcut grup konusmasi var mi?
        var existingConversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.StudentGroupId == groupId && c.Type == ConversationType.StudentGroup);

        if (existingConversation != null)
        {
            // Kullanici zaten katilimci mi kontrol et
            var existingParticipant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == existingConversation.Id && cp.UserId == userId);

            if (existingParticipant != null && existingParticipant.LeftAt != null)
            {
                // Tekrar katil
                existingParticipant.LeftAt = null;
                existingParticipant.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else if (existingParticipant == null)
            {
                // Yeni katilimci olarak ekle
                _context.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = existingConversation.Id,
                    UserId = userId,
                    Role = ConversationParticipantRole.Participant,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return (await GetConversationAsync(existingConversation.Id, userId))!;
        }

        // Yeni grup konusmasi olustur
        var conversation = new Conversation
        {
            Type = ConversationType.StudentGroup,
            Title = group.Name ?? $"Grup #{groupId}",
            StudentGroupId = groupId,
            MaxParticipants = 50,
            CreatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Sifreleme anahtari olustur
        conversation.EncryptionKeyHash = _encryptionService.GenerateConversationKey(conversation.Id);

        // Olusturucuyu ekle
        _context.ConversationParticipants.Add(new ConversationParticipant
        {
            ConversationId = conversation.Id,
            UserId = userId,
            Role = ConversationParticipantRole.Owner,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Grup uyelerini ekle
        foreach (var member in group.Members.Where(m => m.Student?.UserId != null && m.Student.UserId != userId))
        {
            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = member.Student!.UserId!,
                Role = ConversationParticipantRole.Participant,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Ogretmeni de ekle (GroupLessonSchedule uzerinden)
        var teacherUserIds = await _context.GroupLessonSchedules
            .Where(gls => gls.GroupId == groupId && !gls.IsDeleted)
            .Include(gls => gls.Teacher)
            .Where(gls => gls.Teacher != null && gls.Teacher.UserId != userId)
            .Select(gls => gls.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var teacherUserId in teacherUserIds)
        {
            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = teacherUserId,
                Role = ConversationParticipantRole.Admin,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return (await GetConversationAsync(conversation.Id, userId))!;
    }

    public async Task DeleteConversationAsync(int conversationId, string userId)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Bu konuşmada yer almıyorsunuz.");
        }

        // Soft delete - sadece kullanici icin gizle
        participant.LeftAt = DateTime.UtcNow;
        participant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task MuteConversationAsync(int conversationId, string userId, bool mute)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Bu konuşmada yer almıyorsunuz.");
        }

        participant.IsMuted = mute;
        participant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task PinConversationAsync(int conversationId, string userId, bool pin)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Bu konuşmada yer almıyorsunuz.");
        }

        participant.IsPinned = pin;
        participant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Messages

    public async Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 50)
    {
        // Yetki kontrolu
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Bu konuşmada yer almıyorsunuz.");
        }

        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
            .Include(m => m.ReadReceipts)
                .ThenInclude(r => r.User)
            .Include(m => m.DeliveryReceipts)
                .ThenInclude(d => d.User)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(m => MapToMessageDto(m, userId)).Reverse().ToList();
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, string senderUserId)
    {
        // Yetki kontrolu
        var (canMessage, reason) = await _authorizationService.CanMessageInConversationAsync(senderUserId, dto.ConversationId);
        if (!canMessage)
        {
            throw new InvalidOperationException(reason ?? "Bu konuşmaya mesaj atamazsınız.");
        }

        // Icerik moderasyonu
        var moderationResult = _moderationService.ValidateContent(dto.Content);
        if (!moderationResult.IsValid)
        {
            throw new InvalidOperationException(moderationResult.BlockedReason ?? "Mesajınız uygunsuz içerik barındırmaktadır.");
        }

        // Mesaji sifrele
        var (encryptedContent, contentHash) = _encryptionService.Encrypt(dto.Content, dto.ConversationId);

        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = senderUserId,
            ContentEncrypted = encryptedContent,
            ContentHash = contentHash,
            SentAt = DateTime.UtcNow,
            ReplyToMessageId = dto.ReplyToMessageId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);

        // Konusmanin son mesaj bilgisini guncelle
        var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
        if (conversation != null)
        {
            conversation.LastMessageAt = message.SentAt;
            conversation.LastMessageId = message.Id;
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        // Gondericinin yaziyor durumunu kapat
        var senderParticipant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == dto.ConversationId && cp.UserId == senderUserId);

        if (senderParticipant != null)
        {
            senderParticipant.IsTyping = false;
            senderParticipant.LastTypingAt = null;
        }

        await _context.SaveChangesAsync();

        // Mesaji yukle ve don
        var savedMessage = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
            .FirstAsync(m => m.Id == message.Id);

        return MapToMessageDto(savedMessage, senderUserId);
    }

    public async Task<ChatMessageDto> EditMessageAsync(EditMessageDto dto, string userId)
    {
        var message = await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == dto.MessageId);

        if (message == null)
        {
            throw new InvalidOperationException("Mesaj bulunamadı.");
        }

        if (message.SenderId != userId)
        {
            throw new InvalidOperationException("Sadece kendi mesajlarınızı düzenleyebilirsiniz.");
        }

        // 15 dakika icinde duzenlenebilir
        if (message.SentAt < DateTime.UtcNow.AddMinutes(-15))
        {
            throw new InvalidOperationException("Mesajlar sadece 15 dakika içinde düzenlenebilir.");
        }

        // Icerik moderasyonu
        var moderationResult = _moderationService.ValidateContent(dto.Content);
        if (!moderationResult.IsValid)
        {
            throw new InvalidOperationException(moderationResult.BlockedReason ?? "Mesajınız uygunsuz içerik barındırmaktadır.");
        }

        // Yeni icerigi sifrele
        var (encryptedContent, contentHash) = _encryptionService.Encrypt(dto.Content, message.ConversationId);

        message.ContentEncrypted = encryptedContent;
        message.ContentHash = contentHash;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToMessageDto(message, userId);
    }

    public async Task DeleteMessageAsync(int messageId, string userId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);

        if (message == null)
        {
            throw new InvalidOperationException("Mesaj bulunamadı.");
        }

        if (message.SenderId != userId)
        {
            throw new InvalidOperationException("Sadece kendi mesajlarınızı silebilirsiniz.");
        }

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.DeletedBy = userId;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetMessageConversationIdAsync(int messageId, string userId)
    {
        var message = await _context.ChatMessages
            .Where(m => m.Id == messageId)
            .Select(m => new { m.ConversationId, m.SenderId })
            .FirstOrDefaultAsync();

        if (message == null)
        {
            return null;
        }

        // Mesajin sahibi mi veya konusmada mi kontrol et
        if (message.SenderId == userId)
        {
            return message.ConversationId;
        }

        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == message.ConversationId &&
                           cp.UserId == userId &&
                           cp.LeftAt == null);

        return isParticipant ? message.ConversationId : null;
    }

    public async Task MarkAsReadAsync(MarkAsReadDto dto, string userId)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == dto.ConversationId && cp.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Bu konuşmada yer almıyorsunuz.");
        }

        if (dto.MessageId.HasValue)
        {
            // Belirli bir mesaja kadar okundu isaretlesi
            participant.LastReadMessageId = dto.MessageId;
            participant.LastReadAt = DateTime.UtcNow;

            // Read receipt ekle
            var existingReceipt = await _context.MessageReadReceipts
                .AnyAsync(r => r.MessageId == dto.MessageId && r.UserId == userId);

            if (!existingReceipt)
            {
                _context.MessageReadReceipts.Add(new MessageReadReceipt
                {
                    MessageId = dto.MessageId.Value,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            // Tum mesajlari okundu isaretlesi
            var lastMessage = await _context.ChatMessages
                .Where(m => m.ConversationId == dto.ConversationId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                participant.LastReadMessageId = lastMessage.Id;
                participant.LastReadAt = DateTime.UtcNow;
            }
        }

        participant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessageDto>> GetUndeliveredMessagesForUserAsync(int conversationId, string userId)
    {
        // Yetki kontrolu
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            return new List<ChatMessageDto>();
        }

        // Bu kullaniciya henuz iletilmemis mesajlari bul (kendisinin gonderdigi mesajlar haric)
        var undeliveredMessages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.DeliveryReceipts)
            .Where(m => m.ConversationId == conversationId)
            .Where(m => m.SenderId != userId) // Kendi mesajlarini alma
            .Where(m => !m.DeliveryReceipts.Any(dr => dr.UserId == userId)) // Henuz iletilmemis
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return undeliveredMessages.Select(m => MapToMessageDto(m, userId)).ToList();
    }

    public async Task MarkMessageAsDeliveredAsync(int messageId, string userId)
    {
        // Mesaji al
        var message = await _context.ChatMessages
            .Include(m => m.DeliveryReceipts)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            return;
        }

        // Zaten iletildi mi kontrol et
        if (message.DeliveryReceipts.Any(dr => dr.UserId == userId))
        {
            return;
        }

        // Kendi mesajina iletildi bilgisi eklenmez
        if (message.SenderId == userId)
        {
            return;
        }

        // Kullanicinin bu konusmada olup olmadigini kontrol et
        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == message.ConversationId && cp.UserId == userId && cp.LeftAt == null);

        if (!isParticipant)
        {
            return;
        }

        // Delivery receipt ekle
        _context.MessageDeliveryReceipts.Add(new MessageDeliveryReceipt
        {
            MessageId = messageId,
            UserId = userId,
            DeliveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessageDto>> GetUnreadMessagesForUserAsync(int conversationId, string userId)
    {
        // Yetki kontrolu
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            return new List<ChatMessageDto>();
        }

        // Bu kullanicinin henuz okumadigi mesajlari bul (kendisinin gonderdigi mesajlar haric)
        var unreadMessages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ReadReceipts)
            .Where(m => m.ConversationId == conversationId)
            .Where(m => m.SenderId != userId) // Kendi mesajlarini alma
            .Where(m => !m.ReadReceipts.Any(rr => rr.UserId == userId)) // Henuz okunmamis
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return unreadMessages.Select(m => MapToMessageDto(m, userId)).ToList();
    }

    #endregion

    #region Typing Indicator

    public async Task UpdateTypingStatusAsync(int conversationId, string userId, bool isTyping)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (participant == null)
        {
            return;
        }

        participant.IsTyping = isTyping;
        participant.LastTypingAt = isTyping ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
    }

    public async Task<List<TypingIndicatorDto>> GetTypingUsersAsync(int conversationId)
    {
        var typingParticipants = await _context.ConversationParticipants
            .Include(cp => cp.User)
            .Where(cp => cp.ConversationId == conversationId &&
                        cp.IsTyping &&
                        cp.LastTypingAt > DateTime.UtcNow.AddSeconds(-5))
            .ToListAsync();

        return typingParticipants.Select(p => new TypingIndicatorDto
        {
            ConversationId = conversationId,
            UserId = p.UserId,
            UserName = $"{p.User?.FirstName} {p.User?.LastName}".Trim(),
            IsTyping = true
        }).ToList();
    }

    #endregion

    #region Contacts

    public async Task<ContactListDto> GetContactsAsync(string userId)
    {
        var result = new ContactListDto();

        var allowedUserIds = await _authorizationService.GetAllowedRecipientsAsync(userId);
        var allowedGroupIds = await _authorizationService.GetAllowedGroupsAsync(userId);

        // Kullanicilari kategorilere ayir
        foreach (var allowedUserId in allowedUserIds)
        {
            var contact = await GetContactDtoAsync(allowedUserId, userId);
            if (contact == null) continue;

            // Turkce ve Ingilizce rolleri destekle
            switch (contact.Role)
            {
                case "Teacher":
                case "Ogretmen":
                    result.Teachers.Add(contact);
                    break;
                case "Student":
                case "Ogrenci":
                    result.Students.Add(contact);
                    break;
                case "Parent":
                case "Veli":
                    result.Parents.Add(contact);
                    break;
                case "Counselor":
                case "Danışman":
                    result.Counselors.Add(contact);
                    break;
                case "Admin":
                    result.Admins.Add(contact);
                    break;
            }
        }

        // Gruplari ekle
        foreach (var groupId in allowedGroupIds)
        {
            var group = await _context.StudentGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) continue;

            // Mevcut konusma var mi?
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.StudentGroupId == groupId && c.Type == ConversationType.StudentGroup);

            result.Groups.Add(new ContactGroupDto
            {
                GroupId = group.Id,
                GroupName = group.Name ?? $"Grup #{group.Id}",
                GroupType = "StudentGroup",
                MemberCount = group.Members.Count,
                ExistingConversationId = existingConversation?.Id
            });
        }

        return result;
    }

    public async Task<ContactListDto> SearchContactsAsync(string userId, string searchTerm)
    {
        var allContacts = await GetContactsAsync(userId);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return allContacts;
        }

        var searchLower = searchTerm.ToLowerInvariant();

        allContacts.Teachers = allContacts.Teachers
            .Where(c => c.Name.ToLowerInvariant().Contains(searchLower))
            .ToList();

        allContacts.Students = allContacts.Students
            .Where(c => c.Name.ToLowerInvariant().Contains(searchLower))
            .ToList();

        allContacts.Parents = allContacts.Parents
            .Where(c => c.Name.ToLowerInvariant().Contains(searchLower))
            .ToList();

        allContacts.Counselors = allContacts.Counselors
            .Where(c => c.Name.ToLowerInvariant().Contains(searchLower))
            .ToList();

        allContacts.Admins = allContacts.Admins
            .Where(c => c.Name.ToLowerInvariant().Contains(searchLower))
            .ToList();

        allContacts.Groups = allContacts.Groups
            .Where(g => g.GroupName.ToLowerInvariant().Contains(searchLower))
            .ToList();

        return allContacts;
    }

    #endregion

    #region Unread Count

    public async Task<int> GetTotalUnreadCountAsync(string userId)
    {
        var participations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId && cp.LeftAt == null)
            .ToListAsync();

        var totalUnread = 0;
        foreach (var cp in participations)
        {
            totalUnread += await _context.ChatMessages
                .CountAsync(m => m.ConversationId == cp.ConversationId &&
                                m.SentAt > (cp.LastReadAt ?? DateTime.MinValue) &&
                                m.SenderId != userId);
        }

        return totalUnread;
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsAsync(string userId)
    {
        var result = new Dictionary<int, int>();

        var participations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId && cp.LeftAt == null)
            .ToListAsync();

        foreach (var cp in participations)
        {
            var unread = await _context.ChatMessages
                .CountAsync(m => m.ConversationId == cp.ConversationId &&
                                m.SentAt > (cp.LastReadAt ?? DateTime.MinValue) &&
                                m.SenderId != userId);

            if (unread > 0)
            {
                result[cp.ConversationId] = unread;
            }
        }

        return result;
    }

    #endregion

    #region Private Helpers

    private ChatMessageDto MapToMessageDto(ChatMessage message, string currentUserId)
    {
        var decryptedContent = message.IsDeleted
            ? "[Bu mesaj silindi]"
            : _encryptionService.Decrypt(message.ContentEncrypted, message.ConversationId);

        var readReceipts = message.ReadReceipts?.Select(r => new MessageReadReceiptDto
        {
            UserId = r.UserId,
            UserName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
            UserPhoto = r.User?.ProfilePhotoUrl,
            ReadAt = r.ReadAt
        }).ToList() ?? new List<MessageReadReceiptDto>();

        var deliveryReceipts = message.DeliveryReceipts?.Select(d => new MessageDeliveryReceiptDto
        {
            UserId = d.UserId,
            UserName = $"{d.User?.FirstName} {d.User?.LastName}".Trim(),
            UserPhoto = d.User?.ProfilePhotoUrl,
            DeliveredAt = d.DeliveredAt
        }).ToList() ?? new List<MessageDeliveryReceiptDto>();

        return new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = $"{message.Sender?.FirstName} {message.Sender?.LastName}".Trim(),
            SenderPhoto = message.Sender?.ProfilePhotoUrl,
            Content = decryptedContent,
            SentAt = message.SentAt,
            EditedAt = message.EditedAt,
            IsEdited = message.IsEdited,
            IsDeleted = message.IsDeleted,
            IsSystemMessage = message.IsSystemMessage,
            ReplyToMessageId = message.ReplyToMessageId,
            ReplyToMessage = message.ReplyToMessage != null ? new ChatMessageReplyDto
            {
                Id = message.ReplyToMessage.Id,
                SenderName = $"{message.ReplyToMessage.Sender?.FirstName} {message.ReplyToMessage.Sender?.LastName}".Trim(),
                ContentPreview = message.ReplyToMessage.IsDeleted
                    ? "[Silindi]"
                    : TruncateText(_encryptionService.Decrypt(message.ReplyToMessage.ContentEncrypted, message.ConversationId), 100),
                IsDeleted = message.ReplyToMessage.IsDeleted
            } : null,
            ReadReceipts = readReceipts,
            ReadByUserIds = readReceipts.Select(r => r.UserId).ToList(),
            DeliveryReceipts = deliveryReceipts,
            DeliveredToUserIds = deliveryReceipts.Select(d => d.UserId).ToList(),
            IsMine = message.SenderId == currentUserId,
            IsRead = message.ReadReceipts?.Any() ?? false,
            IsDelivered = message.DeliveryReceipts?.Any() ?? false
        };
    }

    private async Task<ContactDto?> GetContactDtoAsync(string targetUserId, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "User";

        // Mevcut konusma var mi?
        var existingConversation = await _context.Conversations
            .Where(c => c.Type == ConversationType.Direct)
            .Where(c => c.Participants.Any(p => p.UserId == currentUserId && p.LeftAt == null))
            .Where(c => c.Participants.Any(p => p.UserId == targetUserId && p.LeftAt == null))
            .Where(c => c.Participants.Count(p => p.LeftAt == null) == 2)
            .FirstOrDefaultAsync();

        return new ContactDto
        {
            UserId = user.Id,
            Name = $"{user.FirstName} {user.LastName}".Trim(),
            Photo = user.ProfilePhotoUrl,
            Role = primaryRole,
            Email = user.Email,
            ExistingConversationId = existingConversation?.Id,
            IsOnline = _onlineStatusService.IsUserOnline(user.Id)
        };
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text.Substring(0, maxLength - 3) + "...";
    }

    #endregion
}
