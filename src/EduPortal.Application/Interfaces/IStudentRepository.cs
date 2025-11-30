using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task<Student?> GetByStudentNoAsync(string studentNo, CancellationToken cancellationToken = default);
    Task<Student?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentWithDetailsAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetStudentsByGradeAsync(int grade, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> StudentNoExistsAsync(string studentNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen yıl için son öğrenci sıra numarasını getirir
    /// </summary>
    Task<int> GetLastStudentSequenceForYearAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kimlik numarasının sistemde kayıtlı olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IdentityNumberExistsAsync(string identityNumber, CancellationToken cancellationToken = default);
}
