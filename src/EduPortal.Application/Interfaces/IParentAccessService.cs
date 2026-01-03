namespace EduPortal.Application.Interfaces;

/// <summary>
/// Veli erişim kontrolü servisi.
/// Velilerin sadece kendilerine bağlı öğrencilerin verilerine erişmesini sağlar.
/// </summary>
public interface IParentAccessService
{
    /// <summary>
    /// Mevcut kullanıcının veli olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsParentAsync(string userId);

    /// <summary>
    /// Velinin ParentId'sini döndürür (veli değilse null)
    /// </summary>
    Task<int?> GetParentIdAsync(string userId);

    /// <summary>
    /// Veliye bağlı öğrenci ID'lerini döndürür
    /// </summary>
    Task<List<int>> GetLinkedStudentIdsAsync(int parentId);

    /// <summary>
    /// Velinin belirli bir öğrenciye erişim yetkisi olup olmadığını kontrol eder
    /// </summary>
    Task<bool> CanAccessStudentAsync(string userId, int studentId);

    /// <summary>
    /// Velinin belirli öğrencilere erişim yetkisi olup olmadığını toplu kontrol eder
    /// </summary>
    Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds);
}
