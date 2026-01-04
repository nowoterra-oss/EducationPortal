namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Online kullanici durumu servisi
/// </summary>
public interface IOnlineStatusService
{
    /// <summary>
    /// Kullanici online mi?
    /// </summary>
    bool IsUserOnline(string userId);

    /// <summary>
    /// Online kullanici ID'lerini getirir
    /// </summary>
    IEnumerable<string> GetOnlineUserIds();
}
