namespace EduPortal.Domain.Enums;

/// <summary>
/// Kimlik belgesi türleri (global öğrenciler için)
/// </summary>
public enum IdentityType
{
    /// <summary>
    /// TC Kimlik Numarası
    /// </summary>
    TCKimlik = 0,

    /// <summary>
    /// Pasaport Numarası
    /// </summary>
    Pasaport = 1,

    /// <summary>
    /// Yabancı Kimlik Numarası
    /// </summary>
    YabanciKimlik = 2,

    /// <summary>
    /// Diğer (Öğrenci Kimliği, vb.)
    /// </summary>
    Diger = 3
}
