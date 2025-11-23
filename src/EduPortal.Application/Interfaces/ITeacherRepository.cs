using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface ITeacherRepository : IGenericRepository<Teacher>
{
    Task<Teacher?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Teacher>> GetTeachersByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Teacher>> GetActiveTeachersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetTeacherCoursesAsync(int teacherId, CancellationToken cancellationToken = default);
}
