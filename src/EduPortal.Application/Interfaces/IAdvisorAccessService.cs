namespace EduPortal.Application.Interfaces;

/// <summary>
/// Danışman (Danisman rolündeki öğretmen) erişim kontrolü servisi.
/// Danışmanların sadece kendilerine atanmış öğrencilerin verilerine erişmesini sağlar.
/// </summary>
public interface IAdvisorAccessService
{
    /// <summary>
    /// Mevcut kullanıcının danışman olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsAdvisorAsync(string userId);

    /// <summary>
    /// Danışmanın TeacherId'sini döndürür (danışman değilse null)
    /// </summary>
    Task<int?> GetAdvisorTeacherIdAsync(string userId);

    /// <summary>
    /// Danışmana atanmış öğrenci ID'lerini döndürür
    /// </summary>
    Task<List<int>> GetAssignedStudentIdsAsync(int teacherId);

    /// <summary>
    /// Danışmanın belirli bir öğrenciye erişim yetkisi olup olmadığını kontrol eder
    /// </summary>
    Task<bool> CanAccessStudentAsync(string userId, int studentId);

    /// <summary>
    /// Danışmanın belirli öğrencilere erişim yetkisi olup olmadığını toplu kontrol eder
    /// </summary>
    Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds);
}
