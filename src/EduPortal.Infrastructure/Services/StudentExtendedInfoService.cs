using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

/// <summary>
/// Ogrenci ek bilgileri servisi implementasyonu
/// </summary>
public class StudentExtendedInfoService : IStudentExtendedInfoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentExtendedInfoService> _logger;

    public StudentExtendedInfoService(ApplicationDbContext context, ILogger<StudentExtendedInfoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Foreign Languages

    public async Task<ApiResponse<List<ForeignLanguageDto>>> GetForeignLanguagesAsync(int studentId)
    {
        try
        {
            var languages = await _context.StudentForeignLanguages
                .Where(fl => fl.StudentId == studentId && !fl.IsDeleted)
                .Select(fl => new ForeignLanguageDto
                {
                    Id = fl.Id,
                    StudentId = fl.StudentId,
                    Language = fl.Language,
                    Level = fl.Level
                })
                .ToListAsync();

            return ApiResponse<List<ForeignLanguageDto>>.SuccessResponse(languages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting foreign languages for student {StudentId}", studentId);
            return ApiResponse<List<ForeignLanguageDto>>.ErrorResponse("Yabanci diller getirilirken hata olustu");
        }
    }

    public async Task<ApiResponse<ForeignLanguageDto>> AddForeignLanguageAsync(int studentId, ForeignLanguageCreateDto dto)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return ApiResponse<ForeignLanguageDto>.ErrorResponse("Ogrenci bulunamadi");

            var entity = new StudentForeignLanguage
            {
                StudentId = studentId,
                Language = dto.Language,
                Level = dto.Level
            };

            _context.StudentForeignLanguages.Add(entity);
            await _context.SaveChangesAsync();

            var result = new ForeignLanguageDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                Language = entity.Language,
                Level = entity.Level
            };

            return ApiResponse<ForeignLanguageDto>.SuccessResponse(result, "Yabanci dil eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding foreign language for student {StudentId}", studentId);
            return ApiResponse<ForeignLanguageDto>.ErrorResponse("Yabanci dil eklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteForeignLanguageAsync(int studentId, int id)
    {
        try
        {
            var entity = await _context.StudentForeignLanguages
                .FirstOrDefaultAsync(fl => fl.Id == id && fl.StudentId == studentId && !fl.IsDeleted);

            if (entity == null)
                return ApiResponse<bool>.ErrorResponse("Yabanci dil kaydı bulunamadi");

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Yabanci dil silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting foreign language {Id} for student {StudentId}", id, studentId);
            return ApiResponse<bool>.ErrorResponse("Yabanci dil silinirken hata olustu");
        }
    }

    #endregion

    #region Hobbies

    public async Task<ApiResponse<List<HobbyDto>>> GetHobbiesAsync(int studentId)
    {
        try
        {
            var hobbies = await _context.StudentHobbies
                .Where(h => h.StudentId == studentId && !h.IsDeleted)
                .Select(h => new HobbyDto
                {
                    Id = h.Id,
                    StudentId = h.StudentId,
                    Category = h.Category,
                    Name = h.Name,
                    HasLicense = h.HasLicense,
                    LicenseLevel = h.LicenseLevel,
                    LicenseDocumentUrl = h.LicenseDocumentUrl,
                    Achievements = h.Achievements,
                    StartDate = h.StartDate
                })
                .ToListAsync();

            return ApiResponse<List<HobbyDto>>.SuccessResponse(hobbies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hobbies for student {StudentId}", studentId);
            return ApiResponse<List<HobbyDto>>.ErrorResponse("Hobiler getirilirken hata olustu");
        }
    }

    public async Task<ApiResponse<HobbyDto>> AddHobbyAsync(int studentId, HobbyCreateDto dto)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return ApiResponse<HobbyDto>.ErrorResponse("Ogrenci bulunamadi");

            var entity = new StudentHobby
            {
                StudentId = studentId,
                Category = dto.Category,
                Name = dto.Name,
                HasLicense = dto.HasLicense,
                LicenseLevel = dto.LicenseLevel,
                LicenseDocumentUrl = dto.LicenseDocumentUrl,
                Achievements = dto.Achievements,
                StartDate = dto.StartDate
            };

            _context.StudentHobbies.Add(entity);
            await _context.SaveChangesAsync();

            var result = new HobbyDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                Category = entity.Category,
                Name = entity.Name,
                HasLicense = entity.HasLicense,
                LicenseLevel = entity.LicenseLevel,
                LicenseDocumentUrl = entity.LicenseDocumentUrl,
                Achievements = entity.Achievements,
                StartDate = entity.StartDate
            };

            return ApiResponse<HobbyDto>.SuccessResponse(result, "Hobi eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding hobby for student {StudentId}", studentId);
            return ApiResponse<HobbyDto>.ErrorResponse("Hobi eklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteHobbyAsync(int studentId, int id)
    {
        try
        {
            var entity = await _context.StudentHobbies
                .FirstOrDefaultAsync(h => h.Id == id && h.StudentId == studentId && !h.IsDeleted);

            if (entity == null)
                return ApiResponse<bool>.ErrorResponse("Hobi kaydi bulunamadi");

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Hobi silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hobby {Id} for student {StudentId}", id, studentId);
            return ApiResponse<bool>.ErrorResponse("Hobi silinirken hata olustu");
        }
    }

    #endregion

    #region Activities

    public async Task<ApiResponse<List<ActivityDto>>> GetActivitiesAsync(int studentId)
    {
        try
        {
            var activities = await _context.StudentActivities
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    Name = a.Name,
                    Type = a.Type,
                    Organization = a.Organization,
                    Description = a.Description,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsOngoing = a.IsOngoing,
                    Achievements = a.Achievements
                })
                .ToListAsync();

            return ApiResponse<List<ActivityDto>>.SuccessResponse(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for student {StudentId}", studentId);
            return ApiResponse<List<ActivityDto>>.ErrorResponse("Aktiviteler getirilirken hata olustu");
        }
    }

    public async Task<ApiResponse<ActivityDto>> AddActivityAsync(int studentId, ActivityCreateDto dto)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return ApiResponse<ActivityDto>.ErrorResponse("Ogrenci bulunamadi");

            var entity = new StudentActivity
            {
                StudentId = studentId,
                Name = dto.Name,
                Type = dto.Type,
                Organization = dto.Organization,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsOngoing = dto.IsOngoing,
                Achievements = dto.Achievements
            };

            _context.StudentActivities.Add(entity);
            await _context.SaveChangesAsync();

            var result = new ActivityDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                Name = entity.Name,
                Type = entity.Type,
                Organization = entity.Organization,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsOngoing = entity.IsOngoing,
                Achievements = entity.Achievements
            };

            return ApiResponse<ActivityDto>.SuccessResponse(result, "Aktivite eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding activity for student {StudentId}", studentId);
            return ApiResponse<ActivityDto>.ErrorResponse("Aktivite eklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteActivityAsync(int studentId, int id)
    {
        try
        {
            var entity = await _context.StudentActivities
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == studentId && !a.IsDeleted);

            if (entity == null)
                return ApiResponse<bool>.ErrorResponse("Aktivite kaydi bulunamadi");

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Aktivite silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {Id} for student {StudentId}", id, studentId);
            return ApiResponse<bool>.ErrorResponse("Aktivite silinirken hata olustu");
        }
    }

    #endregion

    #region Readiness Exams

    public async Task<ApiResponse<List<ReadinessExamDto>>> GetReadinessExamsAsync(int studentId)
    {
        try
        {
            var exams = await _context.StudentReadinessExams
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Select(e => new ReadinessExamDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    ExamName = e.ExamName,
                    ExamDate = e.ExamDate,
                    Score = e.Score,
                    Notes = e.Notes
                })
                .ToListAsync();

            return ApiResponse<List<ReadinessExamDto>>.SuccessResponse(exams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting readiness exams for student {StudentId}", studentId);
            return ApiResponse<List<ReadinessExamDto>>.ErrorResponse("Hazir bulunusluk sinavlari getirilirken hata olustu");
        }
    }

    public async Task<ApiResponse<ReadinessExamDto>> AddReadinessExamAsync(int studentId, ReadinessExamCreateDto dto)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return ApiResponse<ReadinessExamDto>.ErrorResponse("Ogrenci bulunamadi");

            var entity = new StudentReadinessExam
            {
                StudentId = studentId,
                ExamName = dto.ExamName,
                ExamDate = dto.ExamDate,
                Score = dto.Score,
                Notes = dto.Notes
            };

            _context.StudentReadinessExams.Add(entity);
            await _context.SaveChangesAsync();

            var result = new ReadinessExamDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                ExamName = entity.ExamName,
                ExamDate = entity.ExamDate,
                Score = entity.Score,
                Notes = entity.Notes
            };

            return ApiResponse<ReadinessExamDto>.SuccessResponse(result, "Hazir bulunusluk sinavi eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding readiness exam for student {StudentId}", studentId);
            return ApiResponse<ReadinessExamDto>.ErrorResponse("Hazir bulunusluk sinavi eklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteReadinessExamAsync(int studentId, int id)
    {
        try
        {
            var entity = await _context.StudentReadinessExams
                .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == studentId && !e.IsDeleted);

            if (entity == null)
                return ApiResponse<bool>.ErrorResponse("Hazir bulunusluk sinavi kaydi bulunamadi");

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Hazir bulunusluk sinavi silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting readiness exam {Id} for student {StudentId}", id, studentId);
            return ApiResponse<bool>.ErrorResponse("Hazir bulunusluk sinavi silinirken hata olustu");
        }
    }

    #endregion
}
