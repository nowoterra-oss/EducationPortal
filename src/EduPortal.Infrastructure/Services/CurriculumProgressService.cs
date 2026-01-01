using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class CurriculumProgressService : ICurriculumProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CurriculumProgressService> _logger;

    public CurriculumProgressService(
        ApplicationDbContext context,
        ILogger<CurriculumProgressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<StudentCurriculumProgressDto>>> GetStudentProgressAsync(int studentId, int courseId)
    {
        try
        {
            var curriculumItems = await _context.Curriculums
                .Where(c => c.CourseId == courseId && !c.IsDeleted)
                .Include(c => c.ExamResource)
                .OrderBy(c => c.TopicOrder)
                .ToListAsync();

            var progressList = new List<StudentCurriculumProgressDto>();

            foreach (var item in curriculumItems)
            {
                var progress = await _context.StudentCurriculumProgresses
                    .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == item.Id && !p.IsDeleted);

                // Konu ile ilgili ödevleri say
                var homeworkStats = await _context.HomeworkAssignments
                    .Where(h => h.StudentId == studentId && h.CurriculumId == item.Id && !h.IsDeleted)
                    .GroupBy(h => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Completed = g.Count(x => x.Status == HomeworkAssignmentStatus.Degerlendirildi)
                    })
                    .FirstOrDefaultAsync();

                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                progressList.Add(new StudentCurriculumProgressDto
                {
                    Id = progress?.Id ?? 0,
                    StudentId = studentId,
                    StudentName = student != null ? $"{student.User.FirstName} {student.User.LastName}" : "",
                    CurriculumId = item.Id,
                    TopicName = item.TopicName,
                    TopicOrder = item.TopicOrder,
                    IsTopicCompleted = progress?.IsTopicCompleted ?? false,
                    AreHomeworksCompleted = progress?.AreHomeworksCompleted ?? false,
                    IsExamUnlocked = progress?.IsExamUnlocked ?? false,
                    IsExamCompleted = progress?.IsExamCompleted ?? false,
                    ExamScore = progress?.ExamScore,
                    IsApprovedByTeacher = progress?.IsApprovedByTeacher ?? false,
                    TotalHomeworks = homeworkStats?.Total ?? 0,
                    CompletedHomeworks = homeworkStats?.Completed ?? 0,
                    HasExam = item.HasExam,
                    ExamFileName = item.ExamResource?.FileName,
                    ExamDownloadUrl = item.ExamResource?.FilePath
                });
            }

            return ApiResponse<List<StudentCurriculumProgressDto>>.SuccessResponse(progressList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student curriculum progress for studentId: {StudentId}, courseId: {CourseId}", studentId, courseId);
            return ApiResponse<List<StudentCurriculumProgressDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentCurriculumProgressDto>> GetTopicProgressAsync(int studentId, int curriculumId)
    {
        try
        {
            var curriculum = await _context.Curriculums
                .Include(c => c.ExamResource)
                .FirstOrDefaultAsync(c => c.Id == curriculumId && !c.IsDeleted);

            if (curriculum == null)
                return ApiResponse<StudentCurriculumProgressDto>.ErrorResponse("Müfredat bulunamadı");

            var progress = await _context.StudentCurriculumProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == curriculumId && !p.IsDeleted);

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            var homeworkStats = await _context.HomeworkAssignments
                .Where(h => h.StudentId == studentId && h.CurriculumId == curriculumId && !h.IsDeleted)
                .GroupBy(h => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(x => x.Status == HomeworkAssignmentStatus.Degerlendirildi)
                })
                .FirstOrDefaultAsync();

            var dto = new StudentCurriculumProgressDto
            {
                Id = progress?.Id ?? 0,
                StudentId = studentId,
                StudentName = student != null ? $"{student.User.FirstName} {student.User.LastName}" : "",
                CurriculumId = curriculum.Id,
                TopicName = curriculum.TopicName,
                TopicOrder = curriculum.TopicOrder,
                IsTopicCompleted = progress?.IsTopicCompleted ?? false,
                AreHomeworksCompleted = progress?.AreHomeworksCompleted ?? false,
                IsExamUnlocked = progress?.IsExamUnlocked ?? false,
                IsExamCompleted = progress?.IsExamCompleted ?? false,
                ExamScore = progress?.ExamScore,
                IsApprovedByTeacher = progress?.IsApprovedByTeacher ?? false,
                TotalHomeworks = homeworkStats?.Total ?? 0,
                CompletedHomeworks = homeworkStats?.Completed ?? 0,
                HasExam = curriculum.HasExam,
                ExamFileName = curriculum.ExamResource?.FileName,
                ExamDownloadUrl = curriculum.ExamResource?.FilePath
            };

            return ApiResponse<StudentCurriculumProgressDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting topic progress for studentId: {StudentId}, curriculumId: {CurriculumId}", studentId, curriculumId);
            return ApiResponse<StudentCurriculumProgressDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CheckAndUpdateProgressAsync(int studentId, int curriculumId)
    {
        try
        {
            var curriculum = await _context.Curriculums
                .FirstOrDefaultAsync(c => c.Id == curriculumId && !c.IsDeleted);

            if (curriculum == null)
                return ApiResponse<bool>.ErrorResponse("Müfredat bulunamadı");

            // Progress kaydı al veya oluştur
            var progress = await _context.StudentCurriculumProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == curriculumId && !p.IsDeleted);

            if (progress == null)
            {
                progress = new StudentCurriculumProgress
                {
                    StudentId = studentId,
                    CurriculumId = curriculumId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.StudentCurriculumProgresses.AddAsync(progress);
            }

            // Ödev tamamlanma kontrolü
            var homeworkStats = await _context.HomeworkAssignments
                .Where(h => h.StudentId == studentId && h.CurriculumId == curriculumId && !h.IsDeleted)
                .ToListAsync();

            var allCompleted = homeworkStats.Count > 0 &&
                homeworkStats.All(h => h.Status == HomeworkAssignmentStatus.Degerlendirildi);

            if (allCompleted && !progress.AreHomeworksCompleted)
            {
                progress.AreHomeworksCompleted = true;
                progress.HomeworksCompletedAt = DateTime.UtcNow;
            }

            // Sınav kilidi açma kontrolü (konu ve ödevler tamamlanmışsa)
            if (progress.IsTopicCompleted && progress.AreHomeworksCompleted &&
                progress.IsApprovedByTeacher && curriculum.HasExam && !progress.IsExamUnlocked)
            {
                progress.IsExamUnlocked = true;
                progress.ExamUnlockedAt = DateTime.UtcNow;
            }

            progress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "İlerleme güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking curriculum progress for studentId: {StudentId}, curriculumId: {CurriculumId}", studentId, curriculumId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ApproveTopicCompletionAsync(int teacherId, int studentId, int curriculumId)
    {
        try
        {
            var progress = await _context.StudentCurriculumProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == curriculumId && !p.IsDeleted);

            if (progress == null)
            {
                progress = new StudentCurriculumProgress
                {
                    StudentId = studentId,
                    CurriculumId = curriculumId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.StudentCurriculumProgresses.AddAsync(progress);
            }

            progress.IsTopicCompleted = true;
            progress.TopicCompletedAt = DateTime.UtcNow;
            progress.IsApprovedByTeacher = true;
            progress.ApprovedByTeacherId = teacherId;
            progress.ApprovedAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Otomatik sınav kilidi kontrolü
            await CheckAndUpdateProgressAsync(studentId, curriculumId);

            return ApiResponse<bool>.SuccessResponse(true, "Konu tamamlama onaylandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving topic completion for teacherId: {TeacherId}, studentId: {StudentId}, curriculumId: {CurriculumId}", teacherId, studentId, curriculumId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> UnlockExamAsync(int teacherId, int studentId, int curriculumId)
    {
        try
        {
            var progress = await _context.StudentCurriculumProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == curriculumId && !p.IsDeleted);

            if (progress == null)
            {
                progress = new StudentCurriculumProgress
                {
                    StudentId = studentId,
                    CurriculumId = curriculumId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.StudentCurriculumProgresses.AddAsync(progress);
            }

            progress.IsExamUnlocked = true;
            progress.ExamUnlockedAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Sınav kilidi açıldı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking exam for teacherId: {TeacherId}, studentId: {StudentId}, curriculumId: {CurriculumId}", teacherId, studentId, curriculumId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CompleteExamAsync(int studentId, int curriculumId, int score)
    {
        try
        {
            var progress = await _context.StudentCurriculumProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.CurriculumId == curriculumId && !p.IsDeleted);

            if (progress == null)
                return ApiResponse<bool>.ErrorResponse("İlerleme kaydı bulunamadı");

            if (!progress.IsExamUnlocked)
                return ApiResponse<bool>.ErrorResponse("Sınav henüz açılmamış");

            progress.IsExamCompleted = true;
            progress.ExamCompletedAt = DateTime.UtcNow;
            progress.ExamScore = score;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Sınav tamamlandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing exam for studentId: {StudentId}, curriculumId: {CurriculumId}", studentId, curriculumId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }
}
