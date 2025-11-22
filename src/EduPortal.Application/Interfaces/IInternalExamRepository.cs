using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IInternalExamRepository : IGenericRepository<InternalExam>
{
    Task<InternalExam?> GetExamWithResultsAsync(int examId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalExam>> GetExamsByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalExam>> GetExamsByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExamResult>> GetExamResultsAsync(int examId, CancellationToken cancellationToken = default);
    Task<ExamResult?> GetStudentExamResultAsync(int examId, int studentId, CancellationToken cancellationToken = default);
}
