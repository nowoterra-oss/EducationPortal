using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class InternationalExamService : IInternationalExamService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InternationalExamService> _logger;

    public InternationalExamService(ApplicationDbContext context, ILogger<InternationalExamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<InternationalExamDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var exams = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<InternationalExamDto>(examDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<InternationalExamDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all international exams");
            return ApiResponse<PagedResponse<InternationalExamDto>>.ErrorResponse($"Sınavlar getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternationalExamDto>> GetByIdAsync(int id)
    {
        try
        {
            var exam = await _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<InternationalExamDto>.ErrorResponse("Sınav kaydı bulunamadı");
            }

            return ApiResponse<InternationalExamDto>.SuccessResponse(MapToDto(exam));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting international exam by ID: {ExamId}", id);
            return ApiResponse<InternationalExamDto>.ErrorResponse($"Sınav getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<InternationalExamDto>>> GetByStudentAsync(int studentId)
    {
        try
        {
            var exams = await _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            return ApiResponse<List<InternationalExamDto>>.SuccessResponse(examDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting international exams for student: {StudentId}", studentId);
            return ApiResponse<List<InternationalExamDto>>.ErrorResponse($"Öğrenci sınavları getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<InternationalExamDto>>> GetByExamTypeAsync(ExamType examType, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.ExamType == examType && !e.IsDeleted)
                .OrderByDescending(e => e.ExamDate)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var exams = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<InternationalExamDto>(examDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<InternationalExamDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting international exams by type: {ExamType}", examType);
            return ApiResponse<PagedResponse<InternationalExamDto>>.ErrorResponse($"Sınavlar getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternationalExamDto>> CreateAsync(InternationalExamCreateDto dto)
    {
        try
        {
            // Verify student exists
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId && !s.IsDeleted);

            if (student == null)
            {
                return ApiResponse<InternationalExamDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            var exam = new InternationalExam
            {
                StudentId = dto.StudentId,
                ExamType = dto.ExamType,
                ExamName = dto.ExamName,
                Grade = dto.Grade,
                AcademicYear = dto.AcademicYear,
                Score = dto.Score,
                MaxScore = dto.MaxScore,
                ApplicationStartDate = dto.ApplicationStartDate,
                ApplicationEndDate = dto.ApplicationEndDate,
                ExamDate = dto.ExamDate,
                ResultDate = dto.ResultDate,
                CertificateUrl = dto.CertificateUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.InternationalExams.AddAsync(exam);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdExam = await _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            return ApiResponse<InternationalExamDto>.SuccessResponse(MapToDto(createdExam!), "Sınav kaydı başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating international exam");
            return ApiResponse<InternationalExamDto>.ErrorResponse($"Sınav oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InternationalExamDto>> UpdateAsync(int id, InternationalExamUpdateDto dto)
    {
        try
        {
            var exam = await _context.InternationalExams
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<InternationalExamDto>.ErrorResponse("Sınav kaydı bulunamadı");
            }

            exam.ExamType = dto.ExamType;
            exam.ExamName = dto.ExamName;
            exam.Grade = dto.Grade;
            exam.AcademicYear = dto.AcademicYear;
            exam.Score = dto.Score;
            exam.MaxScore = dto.MaxScore;
            exam.ApplicationStartDate = dto.ApplicationStartDate;
            exam.ApplicationEndDate = dto.ApplicationEndDate;
            exam.ExamDate = dto.ExamDate;
            exam.ResultDate = dto.ResultDate;
            exam.CertificateUrl = dto.CertificateUrl;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<InternationalExamDto>.SuccessResponse(MapToDto(exam), "Sınav kaydı başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating international exam: {ExamId}", id);
            return ApiResponse<InternationalExamDto>.ErrorResponse($"Sınav güncellenirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var exam = await _context.InternationalExams
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return ApiResponse<bool>.ErrorResponse("Sınav kaydı bulunamadı");
            }

            // Soft delete
            exam.IsDeleted = true;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Sınav kaydı başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting international exam: {ExamId}", id);
            return ApiResponse<bool>.ErrorResponse($"Sınav silinirken bir hata oluştu: {ex.Message}");
        }
    }

    private static InternationalExamDto MapToDto(InternationalExam exam)
    {
        return new InternationalExamDto
        {
            Id = exam.Id,
            StudentId = exam.StudentId,
            StudentName = exam.Student?.User != null
                ? $"{exam.Student.User.FirstName} {exam.Student.User.LastName}"
                : string.Empty,
            ExamType = exam.ExamType,
            ExamName = exam.ExamName,
            Grade = exam.Grade,
            AcademicYear = exam.AcademicYear,
            Score = exam.Score,
            MaxScore = exam.MaxScore,
            ApplicationStartDate = exam.ApplicationStartDate,
            ApplicationEndDate = exam.ApplicationEndDate,
            ExamDate = exam.ExamDate,
            ResultDate = exam.ResultDate,
            CertificateUrl = exam.CertificateUrl,
            CreatedAt = exam.CreatedAt
        };
    }
}
