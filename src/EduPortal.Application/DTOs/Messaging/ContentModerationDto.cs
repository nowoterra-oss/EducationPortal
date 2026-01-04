namespace EduPortal.Application.DTOs.Messaging;

/// <summary>
/// Icerik moderasyon sonucu
/// </summary>
public class ContentModerationResult
{
    public bool IsValid { get; set; }
    public bool HasProfanity { get; set; }
    public bool HasPhoneNumber { get; set; }
    public bool HasEmail { get; set; }
    public string? BlockedReason { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
    public string? SanitizedContent { get; set; }
}

/// <summary>
/// Kufur filtresi ayarlari
/// </summary>
public class ProfanityFilterSettings
{
    public bool IsEnabled { get; set; } = true;
    public bool BlockMessage { get; set; } = true; // true: mesaji engelle, false: yildizla
    public List<string> CustomBlockedWords { get; set; } = new();
    public List<string> WhitelistedWords { get; set; } = new();
}

/// <summary>
/// Telefon numarasi filtresi ayarlari
/// </summary>
public class PhoneFilterSettings
{
    public bool IsEnabled { get; set; } = true;
    public bool BlockMessage { get; set; } = true;
    public List<string> AllowedCountryCodes { get; set; } = new(); // Empty = block all
}
