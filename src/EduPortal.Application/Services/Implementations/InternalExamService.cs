using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Application.Services.Implementations;

public class InternalExamService : IInternalExamService
{
    private readonly IInternalExamRepository _examRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InternalExamService> _logger;

    public InternalExamService(
        IInternalExamRepository examRepository,
        IStudentRepository studentRepository,
        ApplicationDbContext context,
        ILogger<InternalExamService> logger)
    {
        _examRepository = examRepository;
        _studentRepository = studentRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<InternalExamDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var exams = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<InternalExamDto>(examDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<InternalExamDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all exams");
            return ApiResponse<PagedResponse<InternalExamDto>>.ErrorResponse($"S1navlar getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternalExamDto>> GetByIdAsync(int id)
    {
        try
        {
            var exam = await _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<InternalExamDto>.ErrorResponse("S1nav bulunamad1");
            }

            var dto = MapToDto(exam);
            return ApiResponse<InternalExamDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exam by ID: {ExamId}", id);
            return ApiResponse<InternalExamDto>.ErrorResponse($"S1nav getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<InternalExamDto>>> GetByCourseAsync(int courseId)
    {
        try
        {
            var exams = await _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .Where(e => e.CourseId == courseId && !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            return ApiResponse<List<InternalExamDto>>.SuccessResponse(examDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exams for course: {CourseId}", courseId);
            return ApiResponse<List<InternalExamDto>>.ErrorResponse($"Ders s1navlar1 getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<InternalExamDto>>> GetByStudentAsync(int studentId)
    {
        try
        {
            // Get all exams for courses the student is enrolled in
            var studentCourseIds = await _context.CourseEnrollments
                .Where(ce => ce.StudentId == studentId)
                .Select(ce => ce.CourseId)
                .ToListAsync();

            var exams = await _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .Where(e => studentCourseIds.Contains(e.CourseId) && !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            return ApiResponse<List<InternalExamDto>>.SuccessResponse(examDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exams for student: {StudentId}", studentId);
            return ApiResponse<List<InternalExamDto>>.ErrorResponse($"Örenci s1navlar1 getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternalExamDto>> CreateAsync(InternalExamCreateDto dto, int teacherId)
    {
        try
        {
            var exam = new InternalExam
            {
                CourseId = dto.CourseId,
                TeacherId = teacherId,
                ExamType = dto.ExamType,
                Title = dto.Title,
                ExamDate = dto.ExamDate,
                Duration = dto.Duration,
                MaxScore = dto.MaxScore,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.InternalExams.AddAsync(exam);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdExam = await _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            var examDto = MapToDto(createdExam!);
            return ApiResponse<InternalExamDto>.SuccessResponse(examDto, "S1nav ba_ar1yla olu_turuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating exam");
            return ApiResponse<InternalExamDto>.ErrorResponse($"S1nav olu_turulurken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternalExamDto>> UpdateAsync(InternalExamUpdateDto dto)
    {
        try
        {
            var exam = await _context.InternalExams
                .Include(e => e.Course)
                .Include(e => e.Teacher)
                    .ThenInclude(t => t.User)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<InternalExamDto>.ErrorResponse("S1nav bulunamad1");
            }

            exam.Title = dto.Title;
            exam.ExamDate = dto.ExamDate;
            exam.Duration = dto.Duration;
            exam.MaxScore = dto.MaxScore;
            exam.Description = dto.Description;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var examDto = MapToDto(exam);
            return ApiResponse<InternalExamDto>.SuccessResponse(examDto, "S1nav ba_ar1yla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating exam: {ExamId}", dto.Id);
            return ApiResponse<InternalExamDto>.ErrorResponse($"S1nav güncellenirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var exam = await _context.InternalExams
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<bool>.ErrorResponse("S1nav bulunamad1");
            }

            // Soft delete
            exam.IsDeleted = true;
            exam.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "S1nav ba_ar1yla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting exam: {ExamId}", id);
            return ApiResponse<bool>.ErrorResponse($"S1nav silinirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<ExamResultDto>>> GetResultsAsync(int examId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.ExamResults
                .Include(r => r.Exam)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Where(r => r.ExamId == examId)
                .OrderByDescending(r => r.Score)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var results = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultDtos = results.Select(MapResultToDto).ToList();
            var pagedResponse = new PagedResponse<ExamResultDto>(resultDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<ExamResultDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting results for exam: {ExamId}", examId);
            return ApiResponse<PagedResponse<ExamResultDto>>.ErrorResponse($"Sonuçlar getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ExamResultDto>> AddResultAsync(ExamResultCreateDto dto)
    {
        try
        {
            // Check if exam exists
            var exam = await _context.InternalExams
                .FirstOrDefaultAsync(e => e.Id == dto.ExamId && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<ExamResultDto>.ErrorResponse("S1nav bulunamad1");
            }

            // Check if student exists
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                return ApiResponse<ExamResultDto>.ErrorResponse("Örenci bulunamad1");
            }

            // Check if result already exists
            var existingResult = await _context.ExamResults
                .FirstOrDefaultAsync(r => r.ExamId == dto.ExamId && r.StudentId == dto.StudentId);

            if (existingResult != null)
            {
                return ApiResponse<ExamResultDto>.ErrorResponse("Bu örenci için s1nav sonucu zaten mevcut");
            }

            // Calculate percentage
            var percentage = (dto.Score / exam.MaxScore) * 100;

            var result = new ExamResult
            {
                ExamId = dto.ExamId,
                StudentId = dto.StudentId,
                Score = dto.Score,
                Percentage = percentage,
                Notes = dto.Notes
            };

            await _context.ExamResults.AddAsync(result);
            await _context.SaveChangesAsync();

            // Calculate rank
            var allResults = await _context.ExamResults
                .Where(r => r.ExamId == dto.ExamId)
                .OrderByDescending(r => r.Score)
                .ToListAsync();

            for (int i = 0; i < allResults.Count; i++)
            {
                allResults[i].Rank = i + 1;
            }

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            result = await _context.ExamResults
                .Include(r => r.Exam)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.Id == result.Id);

            var resultDto = MapResultToDto(result!);
            return ApiResponse<ExamResultDto>.SuccessResponse(resultDto, "S1nav sonucu ba_ar1yla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding exam result");
            return ApiResponse<ExamResultDto>.ErrorResponse($"S1nav sonucu eklenirken bir hata olu_tu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ExamStatisticsDto>> GetStatisticsAsync(int examId)
    {
        try
        {
            var exam = await _context.InternalExams
                .FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<ExamStatisticsDto>.ErrorResponse("S1nav bulunamad1");
            }

            var results = await _context.ExamResults
                .Where(r => r.ExamId == examId)
                .ToListAsync();

            if (!results.Any())
            {
                var emptyStats = new ExamStatisticsDto
                {
                    ExamId = examId,
                    ExamTitle = exam.Title,
                    TotalStudents = 0,
                    AverageScore = 0,
                    HighestScore = 0,
                    LowestScore = 0,
                    PassRate = 0,
                    MedianScore = 0
                };

                return ApiResponse<ExamStatisticsDto>.SuccessResponse(emptyStats);
            }

            var scores = results.Select(r => r.Score).OrderBy(s => s).ToList();
            var passThreshold = exam.MaxScore * 0.5m; // 50% pass threshold

            var statistics = new ExamStatisticsDto
            {
                ExamId = examId,
                ExamTitle = exam.Title,
                TotalStudents = results.Count,
                AverageScore = results.Average(r => r.Score),
                HighestScore = results.Max(r => r.Score),
                LowestScore = results.Min(r => r.Score),
                PassRate = (decimal)results.Count(r => r.Score >= passThreshold) / results.Count * 100,
                MedianScore = scores.Count % 2 == 0
                    ? (scores[scores.Count / 2 - 1] + scores[scores.Count / 2]) / 2
                    : scores[scores.Count / 2]
            };

            return ApiResponse<ExamStatisticsDto>.SuccessResponse(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting statistics for exam: {ExamId}", examId);
            return ApiResponse<ExamStatisticsDto>.ErrorResponse($"0statistikler getirilirken bir hata olu_tu: {ex.Message}");
        }
    }

    // Private helper methods for manual mapping
    private static InternalExamDto MapToDto(InternalExam exam)
    {
        return new InternalExamDto
        {
            Id = exam.Id,
            CourseId = exam.CourseId,
            CourseName = exam.Course?.CourseName ?? string.Empty,
            TeacherId = exam.TeacherId,
            TeacherName = exam.Teacher?.User != null
                ? $"{exam.Teacher.User.FirstName} {exam.Teacher.User.LastName}"
                : string.Empty,
            ExamType = exam.ExamType,
            Title = exam.Title,
            ExamDate = exam.ExamDate,
            Duration = exam.Duration,
            MaxScore = exam.MaxScore,
            Description = exam.Description,
            TotalResults = exam.Results?.Count ?? 0,
            CreatedAt = exam.CreatedAt
        };
    }

    private static ExamResultDto MapResultToDto(ExamResult result)
    {
        return new ExamResultDto
        {
            Id = result.Id,
            ExamId = result.ExamId,
            ExamTitle = result.Exam?.Title ?? string.Empty,
            StudentId = result.StudentId,
            StudentName = result.Student?.User != null
                ? $"{result.Student.User.FirstName} {result.Student.User.LastName}"
                : string.Empty,
            Score = result.Score,
            Percentage = result.Percentage,
            Rank = result.Rank,
            Notes = result.Notes
        };
    }
}
