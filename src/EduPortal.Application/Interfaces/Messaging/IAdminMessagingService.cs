using EduPortal.Application.DTOs.Messaging;

namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Admin mesajlasma servisi - konusmalari okuma ve loglama
/// </summary>
public interface IAdminMessagingService
{
    /// <summary>
    /// Konusmayi okur ve log kaydeder
    /// </summary>
    /// <param name="dto">Okuma istegi</param>
    /// <param name="adminUserId">Admin kullanici ID</param>
    /// <param name="ipAddress">Admin IP adresi</param>
    /// <param name="userAgent">Admin tarayici bilgisi</param>
    Task<AdminConversationDetailDto> ReadConversationAsync(
        AdminReadConversationDto dto,
        string adminUserId,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Admin erisim loglarini listeler
    /// </summary>
    Task<List<AdminMessageAccessLogDto>> GetAccessLogsAsync(
        int page = 1,
        int pageSize = 50,
        string? adminUserId = null,
        int? conversationId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Tum konusmalari listeler (admin icin)
    /// </summary>
    Task<List<ConversationListDto>> GetAllConversationsAsync(
        int page = 1,
        int pageSize = 50,
        string? searchTerm = null);

    /// <summary>
    /// Belirli bir kullanicinin konusmalarini listeler (admin icin)
    /// </summary>
    Task<List<ConversationListDto>> GetUserConversationsAsync(string userId, int page = 1, int pageSize = 50);

    /// <summary>
    /// Eski mesajlari arsivler (1 yildan eski)
    /// </summary>
    Task<int> ArchiveOldMessagesAsync(DateTime olderThan);

    /// <summary>
    /// Eski erisim loglarini temizler (1 yildan eski)
    /// </summary>
    Task<int> CleanupOldAccessLogsAsync(DateTime olderThan);
}
