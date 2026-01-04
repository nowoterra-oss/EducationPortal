namespace EduPortal.Application.DTOs.Messaging;

public class PushSubscriptionDto
{
    public int Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePushSubscriptionDto
{
    public string Endpoint { get; set; } = string.Empty;
    public PushSubscriptionKeysDto Keys { get; set; } = new();
}

public class PushSubscriptionKeysDto
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class PushNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Url { get; set; }
    public string? Tag { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}
