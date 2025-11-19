using EduPortal.Domain.Entities;

namespace EduPortal.Infrastructure.Repositories;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
    Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Attendance>> GetAttendanceByDateRangeAsync(int studentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Attendance?> GetAttendanceByStudentAndDateAsync(int studentId, int courseId, DateTime date, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetAttendanceStatsByStudentAsync(int studentId, CancellationToken cancellationToken = default);
}
