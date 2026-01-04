using EduPortal.API.Hubs;
using EduPortal.Application.Interfaces.Messaging;

namespace EduPortal.API.Services;

/// <summary>
/// ChatHub uzerinden online kullanici durumunu sorgulayan servis
/// </summary>
public class OnlineStatusService : IOnlineStatusService
{
    public bool IsUserOnline(string userId)
    {
        return ChatHub.IsUserOnline(userId);
    }

    public IEnumerable<string> GetOnlineUserIds()
    {
        return ChatHub.GetOnlineUserIds();
    }
}
