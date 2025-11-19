using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Course)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Course)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.CourseId == courseId)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByDateRangeAsync(int studentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Course)
            .Where(a => a.StudentId == studentId && a.Date >= startDate && a.Date <= endDate)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attendance?> GetAttendanceByStudentAndDateAsync(int studentId, int courseId, DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _dbSet
            .Include(a => a.Course)
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a =>
                a.StudentId == studentId &&
                a.CourseId == courseId &&
                a.Date.Date == dateOnly, cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetAttendanceStatsByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var attendances = await _dbSet
            .Where(a => a.StudentId == studentId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return attendances.ToDictionary(x => (int)x.Status, x => x.Count);
    }
}
