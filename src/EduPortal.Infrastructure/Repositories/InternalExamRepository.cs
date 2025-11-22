using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class InternalExamRepository : GenericRepository<InternalExam>, IInternalExamRepository
{
    public InternalExamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<InternalExam?> GetExamWithResultsAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Course)
            .Include(e => e.Teacher)
                .ThenInclude(t => t.User)
            .Include(e => e.Results)
                .ThenInclude(r => r.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(e => e.Id == examId, cancellationToken);
    }

    public async Task<IEnumerable<InternalExam>> GetExamsByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Course)
            .Include(e => e.Teacher)
                .ThenInclude(t => t.User)
            .Include(e => e.Results)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InternalExam>> GetExamsByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        // Get all exams where the student has a result or is in the exam's class
        var studentClassIds = await _context.StudentClassAssignments
            .Where(sca => sca.StudentId == studentId && sca.IsActive && !sca.IsDeleted)
            .Select(sca => sca.ClassId)
            .ToListAsync(cancellationToken);

        return await _dbSet
            .Include(e => e.Course)
            .Include(e => e.Teacher)
                .ThenInclude(t => t.User)
            .Include(e => e.Results)
            .Where(e => e.Results.Any(r => r.StudentId == studentId) ||
                        (e.ClassId.HasValue && studentClassIds.Contains(e.ClassId.Value)))
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExamResult>> GetExamResultsAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _context.ExamResults
            .Include(r => r.Exam)
            .Include(r => r.Student)
                .ThenInclude(s => s.User)
            .Where(r => r.ExamId == examId)
            .OrderByDescending(r => r.Score)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExamResult?> GetStudentExamResultAsync(int examId, int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExamResults
            .Include(r => r.Exam)
            .Include(r => r.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentId == studentId, cancellationToken);
    }
}
