namespace EduPortal.Domain.Enums;

/// <summary>
/// Konuşma türü
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Birebir mesajlaşma
    /// </summary>
    Direct = 1,

    /// <summary>
    /// Ders bazlı grup sohbeti
    /// </summary>
    CourseGroup = 2,

    /// <summary>
    /// Öğrenci grubu sohbeti
    /// </summary>
    StudentGroup = 3,

    /// <summary>
    /// Rol bazli duyuru konusmasi (Admin -> Ogrenciler, Ogretmenler, vs.)
    /// </summary>
    Broadcast = 4
}
