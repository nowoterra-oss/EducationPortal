using EduPortal.Domain.Entities;

namespace EduPortal.Infrastructure.Repositories;

public interface IHomeworkRepository : IGenericRepository<Homework>
{
    Task<Homework?> GetHomeworkWithSubmissionsAsync(int homeworkId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Homework>> GetHomeworksByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Homework>> GetActiveHomeworksAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Homework>> GetHomeworksByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentHomeworkSubmission>> GetStudentSubmissionsAsync(int studentId, CancellationToken cancellationToken = default);
    Task<StudentHomeworkSubmission?> GetSubmissionAsync(int homeworkId, int studentId, CancellationToken cancellationToken = default);
}
