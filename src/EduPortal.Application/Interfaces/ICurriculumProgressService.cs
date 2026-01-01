using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;

namespace EduPortal.Application.Interfaces;

public interface ICurriculumProgressService
{
    // Öğrenci müfredat ilerlemesi
    Task<ApiResponse<List<StudentCurriculumProgressDto>>> GetStudentProgressAsync(int studentId, int courseId);
    Task<ApiResponse<StudentCurriculumProgressDto>> GetTopicProgressAsync(int studentId, int curriculumId);

    // Öğretmen onayı
    Task<ApiResponse<bool>> ApproveTopicCompletionAsync(int teacherId, int studentId, int curriculumId);
    Task<ApiResponse<bool>> UnlockExamAsync(int teacherId, int studentId, int curriculumId);

    // Otomatik kontrol
    Task<ApiResponse<bool>> CheckAndUpdateProgressAsync(int studentId, int curriculumId);

    // Sınav tamamlama
    Task<ApiResponse<bool>> CompleteExamAsync(int studentId, int curriculumId, int score);
}
