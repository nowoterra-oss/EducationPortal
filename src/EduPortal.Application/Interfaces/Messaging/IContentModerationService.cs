using EduPortal.Application.DTOs.Messaging;

namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Icerik moderasyon servisi - kufur ve telefon numarasi filtresi
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Mesaj icerigini kontrol eder
    /// </summary>
    ContentModerationResult ValidateContent(string content);

    /// <summary>
    /// Mesaj icerigini temizler (kufurleri yildizlar, telefon numaralarini gizler)
    /// </summary>
    string SanitizeContent(string content);

    /// <summary>
    /// Kufur listesini yukler
    /// </summary>
    Task LoadProfanityListAsync();

    /// <summary>
    /// Ozel engelli kelime ekler
    /// </summary>
    void AddBlockedWord(string word);

    /// <summary>
    /// Beyaz listeye kelime ekler
    /// </summary>
    void AddWhitelistedWord(string word);
}
