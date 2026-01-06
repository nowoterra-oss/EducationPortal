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

        // N+1 Query optimizasyonu: Tum conversation'lar icin okunmamis mesaj sayilarini tek sorguda al
        var conversationIds = conversations.Select(cp => cp.ConversationId).ToList();
        var participantData = conversations.ToDictionary(cp => cp.ConversationId, cp => new { cp.LastReadAt, cp.UserId });

        var unreadCounts = await _context.ChatMessages
            .Where(m => conversationIds.Contains(m.ConversationId) && m.SenderId != userId)
            .GroupBy(m => m.ConversationId)
            .Select(g => new
            {
                ConversationId = g.Key,
                // Her conversation icin LastReadAt'e gore filtreleme yapmak icin tum mesajlari say
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Messages);

        foreach (var cp in conversations)
        {
            var conv = cp.Conversation;
            var lastMessage = conv.Messages.FirstOrDefault();

            // Okunmamis mesaj sayisi (onceden hesaplandi)
            var unreadCount = 0;
            if (unreadCounts.TryGetValue(conv.Id, out var messages))
            {
                var lastReadAt = cp.LastReadAt ?? DateTime.MinValue;
                unreadCount = messages.Count(m => m.SentAt > lastReadAt);
            }

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
            else if (conv.Type == ConversationType.Broadcast)
            {
                // Broadcast/RoleGroup sohbetleri icin title'i kullan
                displayName = conv.Title ?? "Duyuru";
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
        // Yetki kontrolu - kullanici bu kisiye mesaj atabilir mi?
        var (canMessage, reason) = await _authorizationService.CanMessageUserAsync(userId1, userId2);
        if (!canMessage)
        {
            throw new InvalidOperationException(reason ?? "Bu kullanıcıya mesaj atamazsınız.");
        }

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
        // Rol bazli grup kontrolu (1001-1005 arasi)
        if (groupId >= RoleGroupStudents && groupId <= RoleGroupRegistrars)
        {
            return await GetOrCreateRoleGroupConversationAsync(groupId, userId);
        }

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
        // Mesaj icerigi kontrolu
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Mesaj içeriği boş olamaz.");
        }

        if (dto.Content.Length > 4000)
        {
            throw new InvalidOperationException("Mesaj en fazla 4000 karakter olabilir.");
        }

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
        // Mesaj icerigi kontrolu
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Mesaj içeriği boş olamaz.");
        }

        if (dto.Content.Length > 4000)
        {
            throw new InvalidOperationException("Mesaj en fazla 4000 karakter olabilir.");
        }

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

        // Admin icin rol bazli gruplar ekle
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            {
                var roleGroups = await GetRoleBasedGroupsAsync();
                result.Groups.AddRange(roleGroups);
            }
        }

        return result;
    }

    // Rol bazli grup sabitleri (100001-100005 arasi - StudentGroup ID'leriyle cakismamasi icin yuksek degerler)
    public const int RoleGroupStudents = 100001;
    public const int RoleGroupTeachers = 100002;
    public const int RoleGroupParents = 100003;
    public const int RoleGroupCounselors = 100004;
    public const int RoleGroupRegistrars = 100005;

    /// <summary>
    /// Rol bazli grup konusmasini getirir veya olusturur (Admin icin broadcast)
    /// </summary>
    private async Task<ConversationDto> GetOrCreateRoleGroupConversationAsync(int roleGroupId, string userId)
    {
        // Admin kontrolu
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("Kullanıcı bulunamadı.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains("Admin"))
        {
            throw new InvalidOperationException("Bu gruba mesaj gönderme yetkiniz yok. Sadece Admin rol bazlı gruplara mesaj gönderebilir.");
        }

        // Grup adini ve hedef rol adini belirle
        var (groupName, roleName) = roleGroupId switch
        {
            RoleGroupStudents => ("Öğrenciler", "Ogrenci"),
            RoleGroupTeachers => ("Öğretmenler", "Ogretmen"),
            RoleGroupParents => ("Veliler", "Veli"),
            RoleGroupCounselors => ("Danışmanlar", "Danışman"),
            RoleGroupRegistrars => ("Kayıtçılar", "Kayitci"),
            _ => throw new InvalidOperationException("Geçersiz rol grubu.")
        };

        // Mevcut rol grubu konusmasi var mi? (Title ile eslestir)
        var existingConversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Type == ConversationType.Broadcast && c.Title == groupName);

        if (existingConversation != null)
        {
            // Kullanici zaten katilimci mi kontrol et
            var existingParticipant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == existingConversation.Id && cp.UserId == userId);

            if (existingParticipant == null)
            {
                // Admin'i katilimci olarak ekle
                _context.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = existingConversation.Id,
                    UserId = userId,
                    Role = ConversationParticipantRole.Owner,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            else if (existingParticipant.LeftAt != null)
            {
                existingParticipant.LeftAt = null;
                existingParticipant.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Yeni eklenen kullanicilari da ekle (rol grubuna yeni katilan kullanicilar)
            await SyncRoleGroupParticipantsAsync(existingConversation.Id, roleName, userId);

            return (await GetConversationAsync(existingConversation.Id, userId))!;
        }

        // Yeni rol grubu konusmasi olustur
        var conversation = new Conversation
        {
            Type = ConversationType.Broadcast,
            Title = groupName,
            MaxParticipants = 10000, // Cok fazla uye olabilir
            CreatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Sifreleme anahtari olustur
        conversation.EncryptionKeyHash = _encryptionService.GenerateConversationKey(conversation.Id);

        // Admin'i owner olarak ekle
        _context.ConversationParticipants.Add(new ConversationParticipant
        {
            ConversationId = conversation.Id,
            UserId = userId,
            Role = ConversationParticipantRole.Owner,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Hedef roldeki tum kullanicilari katilimci olarak ekle
        await AddRoleGroupParticipantsAsync(conversation.Id, roleName, userId);

        await _context.SaveChangesAsync();

        return (await GetConversationAsync(conversation.Id, userId))!;
    }

    /// <summary>
    /// Rol grubundaki kullanicilari conversation'a ekler
    /// </summary>
    private async Task AddRoleGroupParticipantsAsync(int conversationId, string roleName, string adminUserId)
    {
        // Hedef roldeki kullanicilari al
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

        foreach (var roleUser in usersInRole)
        {
            // Admin'i tekrar ekleme
            if (roleUser.Id == adminUserId) continue;

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = roleUser.Id,
                Role = ConversationParticipantRole.Participant,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Rol grubundaki yeni kullanicilari senkronize eder (mevcut conversation'a ekler)
    /// </summary>
    private async Task SyncRoleGroupParticipantsAsync(int conversationId, string roleName, string adminUserId)
    {
        // Hedef roldeki kullanicilari al
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

        // Mevcut katilimcilari al
        var existingParticipantIds = await _context.ConversationParticipants
            .Where(cp => cp.ConversationId == conversationId)
            .Select(cp => cp.UserId)
            .ToListAsync();

        foreach (var roleUser in usersInRole)
        {
            // Admin'i veya zaten katilimci olani ekleme
            if (roleUser.Id == adminUserId) continue;
            if (existingParticipantIds.Contains(roleUser.Id)) continue;

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = roleUser.Id,
                Role = ConversationParticipantRole.Participant,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Admin icin rol bazli gruplari getirir
    /// </summary>
    private async Task<List<ContactGroupDto>> GetRoleBasedGroupsAsync()
    {
        var groups = new List<ContactGroupDto>();

        // 1. Ogrenciler Grubu
        var studentCount = await _context.Students.CountAsync(s => !s.IsDeleted);
        groups.Add(new ContactGroupDto
        {
            GroupId = RoleGroupStudents, // Pozitif ID'ler rol bazli gruplar icin
            GroupName = "Öğrenciler",
            GroupType = "RoleGroup",
            MemberCount = studentCount,
            ExistingConversationId = null
        });

        // 2. Ogretmenler Grubu
        var teacherCount = await _context.Teachers.CountAsync(t => !t.IsDeleted);
        groups.Add(new ContactGroupDto
        {
            GroupId = RoleGroupTeachers,
            GroupName = "Öğretmenler",
            GroupType = "RoleGroup",
            MemberCount = teacherCount,
            ExistingConversationId = null
        });

        // 3. Veliler Grubu
        var parentCount = await _context.Parents.CountAsync(p => !p.IsDeleted);
        groups.Add(new ContactGroupDto
        {
            GroupId = RoleGroupParents,
            GroupName = "Veliler",
            GroupType = "RoleGroup",
            MemberCount = parentCount,
            ExistingConversationId = null
        });

        // 4. Danismanlar Grubu (Ogrenciye atanmis ogretmenler)
        var counselorCount = await _context.StudentTeacherAssignments
            .Where(sta => sta.IsActive && !sta.IsDeleted && sta.AssignmentType == AssignmentType.Advisor)
            .Select(sta => sta.TeacherId)
            .Distinct()
            .CountAsync();
        groups.Add(new ContactGroupDto
        {
            GroupId = RoleGroupCounselors,
            GroupName = "Danışmanlar",
            GroupType = "RoleGroup",
            MemberCount = counselorCount,
            ExistingConversationId = null
        });

        // 5. Kayitcilar Grubu (Kayitci rolu olan kullanicilar)
        var registrarUsers = await _userManager.GetUsersInRoleAsync("Kayitci");
        groups.Add(new ContactGroupDto
        {
            GroupId = RoleGroupRegistrars,
            GroupName = "Kayıtçılar",
            GroupType = "RoleGroup",
            MemberCount = registrarUsers.Count,
            ExistingConversationId = null
        });

        return groups;
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
        // N+1 Query optimizasyonu: Tek sorguda tum okunmamis mesajlari say
        var participations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId && cp.LeftAt == null)
            .Select(cp => new { cp.ConversationId, cp.LastReadAt })
            .ToListAsync();

        if (!participations.Any())
        {
            return 0;
        }

        var conversationIds = participations.Select(p => p.ConversationId).ToList();

        // Tek sorguda tum mesajlari al ve memory'de filtrele
        var allMessages = await _context.ChatMessages
            .Where(m => conversationIds.Contains(m.ConversationId) && m.SenderId != userId)
            .Select(m => new { m.ConversationId, m.SentAt })
            .ToListAsync();

        var participationDict = participations.ToDictionary(p => p.ConversationId, p => p.LastReadAt ?? DateTime.MinValue);

        var totalUnread = allMessages.Count(m =>
            participationDict.TryGetValue(m.ConversationId, out var lastReadAt) && m.SentAt > lastReadAt);

        return totalUnread;
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsAsync(string userId)
    {
        // N+1 Query optimizasyonu: Tek sorguda tum okunmamis mesaj sayilarini hesapla
        var participations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId && cp.LeftAt == null)
            .Select(cp => new { cp.ConversationId, cp.LastReadAt })
            .ToListAsync();

        if (!participations.Any())
        {
            return new Dictionary<int, int>();
        }

        var conversationIds = participations.Select(p => p.ConversationId).ToList();
        var participationDict = participations.ToDictionary(p => p.ConversationId, p => p.LastReadAt ?? DateTime.MinValue);

        // Tek sorguda tum mesajlari al
        var allMessages = await _context.ChatMessages
            .Where(m => conversationIds.Contains(m.ConversationId) && m.SenderId != userId)
            .Select(m => new { m.ConversationId, m.SentAt })
            .ToListAsync();

        // Memory'de grupla ve say
        var result = allMessages
            .Where(m => participationDict.TryGetValue(m.ConversationId, out var lastReadAt) && m.SentAt > lastReadAt)
            .GroupBy(m => m.ConversationId)
            .Where(g => g.Any())
            .ToDictionary(g => g.Key, g => g.Count());

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
