using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Application.Services.Implementations;

public class HomeworkService : IHomeworkService
{
    private readonly IHomeworkRepository _homeworkRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeworkService> _logger;

    public HomeworkService(
        IHomeworkRepository homeworkRepository,
        IStudentRepository studentRepository,
        ApplicationDbContext context,
        ILogger<HomeworkService> logger)
    {
        _homeworkRepository = homeworkRepository;
        _studentRepository = studentRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<HomeworkDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .Where(h => !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var homeworks = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var homeworkDtos = homeworks.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<HomeworkDto>(homeworkDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all homeworks");
            return ApiResponse<PagedResponse<HomeworkDto>>.ErrorResponse($"Ödevler getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> GetByIdAsync(int id)
    {
        try
        {
            var homework = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

            if (homework == null)
            {
                return ApiResponse<HomeworkDto>.ErrorResponse("Ödev bulunamadı");
            }

            var dto = MapToDto(homework);
            return ApiResponse<HomeworkDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homework by ID: {HomeworkId}", id);
            return ApiResponse<HomeworkDto>.ErrorResponse($"Ödev getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HomeworkDto>>> GetByCourseAsync(int courseId)
    {
        try
        {
            var homeworks = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .Where(h => h.CourseId == courseId && !h.IsDeleted)
                .OrderByDescending(h => h.DueDate)
                .ToListAsync();

            var homeworkDtos = homeworks.Select(MapToDto).ToList();
            return ApiResponse<List<HomeworkDto>>.SuccessResponse(homeworkDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homeworks for course: {CourseId}", courseId);
            return ApiResponse<List<HomeworkDto>>.ErrorResponse($"Ders ödevleri getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HomeworkDto>>> GetByStudentAsync(int studentId)
    {
        try
        {
            // Get all homeworks for courses the student is enrolled in
            var studentCourseIds = await _context.CourseEnrollments
                .Where(ce => ce.StudentId == studentId)
                .Select(ce => ce.CourseId)
                .ToListAsync();

            var homeworks = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .Where(h => studentCourseIds.Contains(h.CourseId) && !h.IsDeleted)
                .OrderByDescending(h => h.DueDate)
                .ToListAsync();

            var homeworkDtos = homeworks.Select(MapToDto).ToList();
            return ApiResponse<List<HomeworkDto>>.SuccessResponse(homeworkDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homeworks for student: {StudentId}", studentId);
            return ApiResponse<List<HomeworkDto>>.ErrorResponse($"Öğrenci ödevleri getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> CreateAsync(HomeworkCreateDto dto, int teacherId)
    {
        try
        {
            // Validate dates
            if (dto.DueDate < dto.AssignedDate)
            {
                return ApiResponse<HomeworkDto>.ErrorResponse("Son teslim tarihi, atanma tarihinden önce olamaz");
            }

            var homework = new Homework
            {
                CourseId = dto.CourseId,
                TeacherId = teacherId,
                Title = dto.Title,
                Description = dto.Description,
                AssignedDate = dto.AssignedDate,
                DueDate = dto.DueDate,
                MaxScore = dto.MaxScore,
                ResourceUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.Homeworks.AddAsync(homework);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdHomework = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .FirstOrDefaultAsync(h => h.Id == homework.Id);

            var homeworkDto = MapToDto(createdHomework!);
            return ApiResponse<HomeworkDto>.SuccessResponse(homeworkDto, "Ödev başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating homework");
            return ApiResponse<HomeworkDto>.ErrorResponse($"Ödev oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> UpdateAsync(HomeworkUpdateDto dto)
    {
        try
        {
            var homework = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                    .ThenInclude(t => t.User)
                .Include(h => h.Submissions)
                .FirstOrDefaultAsync(h => h.Id == dto.Id && !h.IsDeleted);

            if (homework == null)
            {
                return ApiResponse<HomeworkDto>.ErrorResponse("Ödev bulunamadı");
            }

            homework.Title = dto.Title;
            homework.Description = dto.Description;
            homework.DueDate = dto.DueDate;
            homework.MaxScore = dto.MaxScore;
            homework.ResourceUrl = dto.AttachmentUrl;
            homework.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var homeworkDto = MapToDto(homework);
            return ApiResponse<HomeworkDto>.SuccessResponse(homeworkDto, "Ödev başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating homework: {HomeworkId}", dto.Id);
            return ApiResponse<HomeworkDto>.ErrorResponse($"Ödev güncellenirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var homework = await _context.Homeworks
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

            if (homework == null)
            {
                return ApiResponse<bool>.ErrorResponse("Ödev bulunamadı");
            }

            // Soft delete
            homework.IsDeleted = true;
            homework.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Ödev başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting homework: {HomeworkId}", id);
            return ApiResponse<bool>.ErrorResponse($"Ödev silinirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<HomeworkSubmissionDto>>> GetSubmissionsAsync(int homeworkId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.StudentHomeworkSubmissions
                .Include(s => s.Homework)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Where(s => s.HomeworkId == homeworkId && !s.IsDeleted)
                .OrderByDescending(s => s.SubmissionDate)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var submissions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var submissionDtos = submissions.Select(MapSubmissionToDto).ToList();
            var pagedResponse = new PagedResponse<HomeworkSubmissionDto>(submissionDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkSubmissionDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting submissions for homework: {HomeworkId}", homeworkId);
            return ApiResponse<PagedResponse<HomeworkSubmissionDto>>.ErrorResponse($"Teslimler getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkSubmissionDto>> SubmitHomeworkAsync(HomeworkSubmitDto dto)
    {
        try
        {
            // Check if homework exists
            var homework = await _context.Homeworks
                .FirstOrDefaultAsync(h => h.Id == dto.HomeworkId && !h.IsDeleted);

            if (homework == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Ödev bulunamadı");
            }

            // Check if student exists
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            // Check if already submitted
            var existingSubmission = await _context.StudentHomeworkSubmissions
                .Include(s => s.Homework)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(s => s.HomeworkId == dto.HomeworkId && s.StudentId == dto.StudentId && !s.IsDeleted);

            StudentHomeworkSubmission submission;

            if (existingSubmission != null)
            {
                // Update existing submission
                existingSubmission.SubmissionUrl = dto.SubmissionUrl;
                existingSubmission.SubmissionDate = dto.SubmissionDate ?? DateTime.UtcNow;
                existingSubmission.Status = dto.Status;
                existingSubmission.CompletionPercentage = 100;
                existingSubmission.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                submission = existingSubmission;
            }
            else
            {
                // Create new submission
                submission = new StudentHomeworkSubmission
                {
                    HomeworkId = dto.HomeworkId,
                    StudentId = dto.StudentId,
                    SubmissionUrl = dto.SubmissionUrl,
                    SubmissionDate = dto.SubmissionDate ?? DateTime.UtcNow,
                    Status = dto.Status,
                    CompletionPercentage = 100,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _context.StudentHomeworkSubmissions.AddAsync(submission);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                submission = await _context.StudentHomeworkSubmissions
                    .Include(s => s.Homework)
                    .Include(s => s.Student)
                        .ThenInclude(st => st.User)
                    .FirstOrDefaultAsync(s => s.Id == submission.Id);
            }

            var submissionDto = MapSubmissionToDto(submission!);
            return ApiResponse<HomeworkSubmissionDto>.SuccessResponse(submissionDto, "Ödev başarıyla teslim edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while submitting homework");
            return ApiResponse<HomeworkSubmissionDto>.ErrorResponse($"Ödev teslim edilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkSubmissionDto>> GradeSubmissionAsync(GradeSubmissionDto dto)
    {
        try
        {
            var submission = await _context.StudentHomeworkSubmissions
                .Include(s => s.Homework)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId && !s.IsDeleted);

            if (submission == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Teslim bulunamadı");
            }

            submission.Score = dto.Score;
            submission.TeacherFeedback = dto.TeacherFeedback;
            submission.Status = HomeworkStatus.Degerlendirildi;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var submissionDto = MapSubmissionToDto(submission);
            return ApiResponse<HomeworkSubmissionDto>.SuccessResponse(submissionDto, "Ödev başarıyla notlandırıldı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while grading submission: {SubmissionId}", dto.SubmissionId);
            return ApiResponse<HomeworkSubmissionDto>.ErrorResponse($"Ödev notlandırılırken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HomeworkSubmissionDto>>> GetStudentSubmissionsAsync(int studentId)
    {
        try
        {
            var submissions = await _context.StudentHomeworkSubmissions
                .Include(s => s.Homework)
                    .ThenInclude(h => h.Course)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Where(s => s.StudentId == studentId && !s.IsDeleted)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            var submissionDtos = submissions.Select(MapSubmissionToDto).ToList();
            return ApiResponse<List<HomeworkSubmissionDto>>.SuccessResponse(submissionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting student submissions: {StudentId}", studentId);
            return ApiResponse<List<HomeworkSubmissionDto>>.ErrorResponse($"Öğrenci teslimleri getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    // Private helper methods for manual mapping
    private static HomeworkDto MapToDto(Homework homework)
    {
        return new HomeworkDto
        {
            Id = homework.Id,
            CourseId = homework.CourseId,
            CourseName = homework.Course?.CourseName ?? string.Empty,
            Title = homework.Title,
            Description = homework.Description,
            AssignedDate = homework.AssignedDate,
            DueDate = homework.DueDate,
            MaxScore = homework.MaxScore,
            AttachmentUrl = homework.ResourceUrl,
            IsActive = !homework.IsDeleted,
            CreatedAt = homework.CreatedAt,
            TotalSubmissions = homework.Submissions?.Count ?? 0
        };
    }

    private static HomeworkSubmissionDto MapSubmissionToDto(StudentHomeworkSubmission submission)
    {
        return new HomeworkSubmissionDto
        {
            Id = submission.Id,
            HomeworkId = submission.HomeworkId,
            HomeworkTitle = submission.Homework?.Title ?? string.Empty,
            StudentId = submission.StudentId,
            StudentName = submission.Student?.User != null
                ? $"{submission.Student.User.FirstName} {submission.Student.User.LastName}"
                : string.Empty,
            SubmissionUrl = submission.SubmissionUrl,
            Comment = null, // Not in entity, using TeacherFeedback instead
            SubmissionDate = submission.SubmissionDate,
            Status = submission.Status,
            Score = (int?)(submission.Score ?? 0),
            TeacherFeedback = submission.TeacherFeedback,
            GradedAt = submission.UpdatedAt,
            CreatedAt = submission.CreatedAt
        };
    }
}
