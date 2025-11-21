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
}
