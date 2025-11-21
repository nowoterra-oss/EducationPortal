using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class HomeworkRepository : GenericRepository<Homework>, IHomeworkRepository
{
    public HomeworkRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Homework?> GetHomeworkWithSubmissionsAsync(int homeworkId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Course)
            .Include(h => h.Submissions)
                .ThenInclude(s => s.Student)
                    .ThenInclude(st => st.User)
            .FirstOrDefaultAsync(h => h.Id == homeworkId, cancellationToken);
    }

    public async Task<IEnumerable<Homework>> GetHomeworksByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Course)
            .Where(h => h.CourseId == courseId)
            .OrderByDescending(h => h.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Homework>> GetActiveHomeworksAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        return await _dbSet
            .Include(h => h.Course)
            .Where(h => h.DueDate >= today)
            .OrderBy(h => h.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Homework>> GetHomeworksByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Course)
            .Where(h => h.DueDate >= startDate && h.DueDate <= endDate)
            .OrderBy(h => h.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentHomeworkSubmission>> GetStudentSubmissionsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentHomeworkSubmissions
            .Include(s => s.Homework)
                .ThenInclude(h => h.Course)
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentHomeworkSubmission?> GetSubmissionAsync(int homeworkId, int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentHomeworkSubmissions
            .Include(s => s.Homework)
            .Include(s => s.Student)
                .ThenInclude(st => st.User)
            .FirstOrDefaultAsync(s => s.HomeworkId == homeworkId && s.StudentId == studentId, cancellationToken);
    }
}
