using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EduPortal.Infrastructure.Services;

public class HomeworkDraftService : IHomeworkDraftService
{
    private readonly ApplicationDbContext _context;
    private readonly IHomeworkAssignmentService _homeworkService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HomeworkDraftService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public HomeworkDraftService(
        ApplicationDbContext context,
        IHomeworkAssignmentService homeworkService,
        IWebHostEnvironment env,
        ILogger<HomeworkDraftService> logger)
    {
        _context = context;
        _homeworkService = homeworkService;
        _env = env;
        _logger = logger;
    }

    public async Task<ApiResponse<List<HomeworkDraftDto>>> GetDraftsByTeacherAsync(int teacherId, bool? isSent = null)
    {
        try
        {
            var query = _context.HomeworkDrafts
                .Include(d => d.Teacher)
                    .ThenInclude(t => t.User)
                .Include(d => d.Course)
                .Where(d => d.TeacherId == teacherId && !d.IsDeleted);

            if (isSent.HasValue)
            {
                query = query.Where(d => d.IsSent == isSent.Value);
            }

            var drafts = await query
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .ToListAsync();

            var dtos = drafts.Select(MapToDto).ToList();
            return ApiResponse<List<HomeworkDraftDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drafts for teacher {TeacherId}", teacherId);
            return ApiResponse<List<HomeworkDraftDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDraftDto>> GetDraftByIdAsync(int draftId)
    {
        try
        {
            var draft = await _context.HomeworkDrafts
                .Include(d => d.Teacher)
                    .ThenInclude(t => t.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == draftId && !d.IsDeleted);

            if (draft == null)
                return ApiResponse<HomeworkDraftDto>.ErrorResponse("Taslak bulunamadı");

            return ApiResponse<HomeworkDraftDto>.SuccessResponse(MapToDto(draft));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting draft {DraftId}", draftId);
            return ApiResponse<HomeworkDraftDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDraftDto>> GetDraftByLessonIdAsync(int teacherId, string lessonId)
    {
        try
        {
            var draft = await _context.HomeworkDrafts
                .Include(d => d.Teacher)
                    .ThenInclude(t => t.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.TeacherId == teacherId && d.LessonId == lessonId && !d.IsDeleted);

            if (draft == null)
                return ApiResponse<HomeworkDraftDto>.ErrorResponse("Taslak bulunamadı");

            return ApiResponse<HomeworkDraftDto>.SuccessResponse(MapToDto(draft));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting draft by lessonId {LessonId}", lessonId);
            return ApiResponse<HomeworkDraftDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDraftDto>> CreateOrUpdateDraftAsync(int teacherId, CreateHomeworkDraftDto dto)
    {
        try
        {
            // Mevcut taslak var mı kontrol et (lessonId + teacherId unique)
            var existing = await _context.HomeworkDrafts
                .Include(d => d.Teacher)
                    .ThenInclude(t => t.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.TeacherId == teacherId && d.LessonId == dto.LessonId && !d.IsDeleted);

            if (existing != null)
            {
                // Güncelle
                existing.Title = dto.Title;
                existing.Description = dto.Description;
                existing.DueDate = dto.DueDate;
                existing.TestDueDate = dto.TestDueDate;
                existing.HasTest = dto.HasTest ?? false;
                existing.CourseId = dto.CourseId;
                existing.StudentsJson = JsonSerializer.Serialize(dto.Students, _jsonOptions);
                existing.ContentUrlsJson = dto.ContentUrls != null ? JsonSerializer.Serialize(dto.ContentUrls, _jsonOptions) : null;
                existing.CourseResourceIdsJson = dto.CourseResourceIds != null ? JsonSerializer.Serialize(dto.CourseResourceIds, _jsonOptions) : null;
                existing.TestUrlsJson = dto.TestUrls != null ? JsonSerializer.Serialize(dto.TestUrls, _jsonOptions) : null;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Draft {DraftId} updated for teacher {TeacherId}", existing.Id, teacherId);
                return ApiResponse<HomeworkDraftDto>.SuccessResponse(MapToDto(existing), "Taslak güncellendi");
            }
            else
            {
                // Yeni oluştur
                var newDraft = new HomeworkDraft
                {
                    TeacherId = teacherId,
                    LessonId = dto.LessonId,
                    CourseId = dto.CourseId,
                    Title = dto.Title,
                    Description = dto.Description,
                    DueDate = dto.DueDate,
                    TestDueDate = dto.TestDueDate,
                    HasTest = dto.HasTest ?? false,
                    StudentsJson = JsonSerializer.Serialize(dto.Students, _jsonOptions),
                    ContentUrlsJson = dto.ContentUrls != null ? JsonSerializer.Serialize(dto.ContentUrls, _jsonOptions) : null,
                    CourseResourceIdsJson = dto.CourseResourceIds != null ? JsonSerializer.Serialize(dto.CourseResourceIds, _jsonOptions) : null,
                    TestUrlsJson = dto.TestUrls != null ? JsonSerializer.Serialize(dto.TestUrls, _jsonOptions) : null,
                    IsSent = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.HomeworkDrafts.Add(newDraft);
                await _context.SaveChangesAsync();

                // Navigation property'leri yükle
                await _context.Entry(newDraft)
                    .Reference(d => d.Teacher)
                    .Query()
                    .Include(t => t.User)
                    .LoadAsync();

                if (newDraft.CourseId.HasValue)
                {
                    await _context.Entry(newDraft).Reference(d => d.Course).LoadAsync();
                }

                _logger.LogInformation("Draft {DraftId} created for teacher {TeacherId}", newDraft.Id, teacherId);
                return ApiResponse<HomeworkDraftDto>.SuccessResponse(MapToDto(newDraft), "Taslak oluşturuldu");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating draft for teacher {TeacherId}", teacherId);
            return ApiResponse<HomeworkDraftDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDraftDto>> UpdateDraftAsync(int draftId, UpdateHomeworkDraftDto dto)
    {
        try
        {
            var draft = await _context.HomeworkDrafts
                .Include(d => d.Teacher)
                    .ThenInclude(t => t.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == draftId && !d.IsDeleted);

            if (draft == null)
                return ApiResponse<HomeworkDraftDto>.ErrorResponse("Taslak bulunamadı");

            if (draft.IsSent)
                return ApiResponse<HomeworkDraftDto>.ErrorResponse("Gönderilmiş taslak güncellenemez");

            // Partial update
            if (dto.Title != null) draft.Title = dto.Title;
            if (dto.Description != null) draft.Description = dto.Description;
            if (dto.DueDate.HasValue) draft.DueDate = dto.DueDate.Value;
            if (dto.TestDueDate.HasValue) draft.TestDueDate = dto.TestDueDate;
            if (dto.HasTest.HasValue) draft.HasTest = dto.HasTest.Value;
            if (dto.Students != null) draft.StudentsJson = JsonSerializer.Serialize(dto.Students, _jsonOptions);
            if (dto.ContentUrls != null) draft.ContentUrlsJson = JsonSerializer.Serialize(dto.ContentUrls, _jsonOptions);
            if (dto.CourseResourceIds != null) draft.CourseResourceIdsJson = JsonSerializer.Serialize(dto.CourseResourceIds, _jsonOptions);
            if (dto.TestUrls != null) draft.TestUrlsJson = JsonSerializer.Serialize(dto.TestUrls, _jsonOptions);

            draft.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<HomeworkDraftDto>.SuccessResponse(MapToDto(draft), "Taslak güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating draft {DraftId}", draftId);
            return ApiResponse<HomeworkDraftDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<DraftFileDto>> UploadContentFileAsync(int draftId, Stream fileStream, string fileName, string contentType)
    {
        return await UploadFileAsync(draftId, fileStream, fileName, "content");
    }

    public async Task<ApiResponse<DraftFileDto>> UploadTestFileAsync(int draftId, Stream fileStream, string fileName, string contentType)
    {
        return await UploadFileAsync(draftId, fileStream, fileName, "test");
    }

    private async Task<ApiResponse<DraftFileDto>> UploadFileAsync(int draftId, Stream fileStream, string fileName, string fileType)
    {
        try
        {
            var draft = await _context.HomeworkDrafts.FindAsync(draftId);
            if (draft == null || draft.IsDeleted)
                return ApiResponse<DraftFileDto>.ErrorResponse("Taslak bulunamadı");

            if (draft.IsSent)
                return ApiResponse<DraftFileDto>.ErrorResponse("Gönderilmiş taslağa dosya yüklenemez");

            // Dosyayı kaydet
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var folder = fileType == "content" ? "draft-content" : "draft-test";
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            var fileDto = new DraftFileDto
            {
                Name = fileName,
                DownloadUrl = $"/uploads/{folder}/{uniqueFileName}",
                FileSize = new FileInfo(filePath).Length
            };

            // JSON'a ekle
            if (fileType == "content")
            {
                var files = string.IsNullOrEmpty(draft.ContentFilesJson)
                    ? new List<DraftFileDto>()
                    : JsonSerializer.Deserialize<List<DraftFileDto>>(draft.ContentFilesJson, _jsonOptions) ?? new List<DraftFileDto>();
                files.Add(fileDto);
                draft.ContentFilesJson = JsonSerializer.Serialize(files, _jsonOptions);
            }
            else
            {
                var files = string.IsNullOrEmpty(draft.TestFilesJson)
                    ? new List<DraftFileDto>()
                    : JsonSerializer.Deserialize<List<DraftFileDto>>(draft.TestFilesJson, _jsonOptions) ?? new List<DraftFileDto>();
                files.Add(fileDto);
                draft.TestFilesJson = JsonSerializer.Serialize(files, _jsonOptions);
            }

            draft.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("File {FileName} uploaded to draft {DraftId} ({FileType})", fileName, draftId, fileType);
            return ApiResponse<DraftFileDto>.SuccessResponse(fileDto, "Dosya yüklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to draft {DraftId}", draftId);
            return ApiResponse<DraftFileDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveFileAsync(int draftId, string downloadUrl, string fileType)
    {
        try
        {
            var draft = await _context.HomeworkDrafts.FindAsync(draftId);
            if (draft == null || draft.IsDeleted)
                return ApiResponse<bool>.ErrorResponse("Taslak bulunamadı");

            if (draft.IsSent)
                return ApiResponse<bool>.ErrorResponse("Gönderilmiş taslaktan dosya silinemez");

            bool removed = false;

            if (fileType == "content" && !string.IsNullOrEmpty(draft.ContentFilesJson))
            {
                var files = JsonSerializer.Deserialize<List<DraftFileDto>>(draft.ContentFilesJson, _jsonOptions) ?? new List<DraftFileDto>();
                var toRemove = files.FirstOrDefault(f => f.DownloadUrl == downloadUrl);
                if (toRemove != null)
                {
                    files.Remove(toRemove);
                    draft.ContentFilesJson = files.Any() ? JsonSerializer.Serialize(files, _jsonOptions) : null;
                    removed = true;
                }
            }
            else if (fileType == "test" && !string.IsNullOrEmpty(draft.TestFilesJson))
            {
                var files = JsonSerializer.Deserialize<List<DraftFileDto>>(draft.TestFilesJson, _jsonOptions) ?? new List<DraftFileDto>();
                var toRemove = files.FirstOrDefault(f => f.DownloadUrl == downloadUrl);
                if (toRemove != null)
                {
                    files.Remove(toRemove);
                    draft.TestFilesJson = files.Any() ? JsonSerializer.Serialize(files, _jsonOptions) : null;
                    removed = true;
                }
            }

            if (removed)
            {
                // Fiziksel dosyayı da sil
                var physicalPath = Path.Combine(_env.WebRootPath ?? "wwwroot", downloadUrl.TrimStart('/'));
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                draft.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Dosya silindi");
            }

            return ApiResponse<bool>.ErrorResponse("Dosya bulunamadı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing file from draft {DraftId}", draftId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SendDraftResultDto>> SendDraftAsync(int draftId)
    {
        try
        {
            var draft = await _context.HomeworkDrafts
                .Include(d => d.Teacher)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == draftId && !d.IsDeleted);

            if (draft == null)
                return ApiResponse<SendDraftResultDto>.ErrorResponse("Taslak bulunamadı");

            if (draft.IsSent)
                return ApiResponse<SendDraftResultDto>.ErrorResponse("Bu taslak zaten gönderildi");

            var students = JsonSerializer.Deserialize<List<DraftStudentDto>>(draft.StudentsJson, _jsonOptions) ?? new List<DraftStudentDto>();

            if (!students.Any())
                return ApiResponse<SendDraftResultDto>.ErrorResponse("Taslakta öğrenci yok");

            var contentUrls = !string.IsNullOrEmpty(draft.ContentUrlsJson)
                ? JsonSerializer.Deserialize<List<string>>(draft.ContentUrlsJson, _jsonOptions) : null;
            var contentFiles = !string.IsNullOrEmpty(draft.ContentFilesJson)
                ? JsonSerializer.Deserialize<List<DraftFileDto>>(draft.ContentFilesJson, _jsonOptions) : null;
            var resourceIds = !string.IsNullOrEmpty(draft.CourseResourceIdsJson)
                ? JsonSerializer.Deserialize<List<int>>(draft.CourseResourceIdsJson, _jsonOptions) : null;
            var testUrls = !string.IsNullOrEmpty(draft.TestUrlsJson)
                ? JsonSerializer.Deserialize<List<string>>(draft.TestUrlsJson, _jsonOptions) : null;
            var testFiles = !string.IsNullOrEmpty(draft.TestFilesJson)
                ? JsonSerializer.Deserialize<List<DraftFileDto>>(draft.TestFilesJson, _jsonOptions) : null;

            var result = new SendDraftResultDto
            {
                TotalStudents = students.Count,
                Errors = new List<string>(),
                CreatedAssignmentIds = new List<int>()
            };

            // Content URL'lerine dosya URL'lerini ekle
            var allContentUrls = new List<string>();
            if (contentUrls != null) allContentUrls.AddRange(contentUrls);
            if (contentFiles != null) allContentUrls.AddRange(contentFiles.Select(f => f.DownloadUrl));

            var allTestUrls = new List<string>();
            if (testUrls != null) allTestUrls.AddRange(testUrls);
            if (testFiles != null) allTestUrls.AddRange(testFiles.Select(f => f.DownloadUrl));

            foreach (var student in students)
            {
                try
                {
                    var assignmentDto = new CreateHomeworkAssignmentDto
                    {
                        StudentId = student.StudentId,
                        TeacherId = draft.TeacherId,
                        Title = draft.Title,
                        Description = draft.Description,
                        StartDate = DateTime.UtcNow,
                        DueDate = draft.DueDate,
                        CourseId = draft.CourseId,
                        SkipAttendanceCheck = true,
                        ContentUrls = allContentUrls.Any() ? allContentUrls : null,
                        CourseResourceIds = resourceIds,
                        TestDueDate = draft.HasTest ? draft.TestDueDate : null,
                        TestUrls = draft.HasTest && allTestUrls.Any() ? allTestUrls : null,
                        HasTest = draft.HasTest
                    };

                    var assignmentResult = await _homeworkService.CreateAssignmentAsync(draft.TeacherId, assignmentDto);

                    if (assignmentResult.Success && assignmentResult.Data != null)
                    {
                        result.SuccessCount++;
                        result.CreatedAssignmentIds.Add(assignmentResult.Data.Id);
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Errors.Add($"{student.StudentName}: {assignmentResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"{student.StudentName}: {ex.Message}");
                    _logger.LogWarning(ex, "Failed to create assignment for student {StudentId} from draft {DraftId}", student.StudentId, draftId);
                }
            }

            // Taslağı gönderildi olarak işaretle
            draft.IsSent = true;
            draft.SentAt = DateTime.UtcNow;
            draft.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Draft {DraftId} sent: {SuccessCount}/{TotalStudents} successful",
                draftId, result.SuccessCount, result.TotalStudents);

            return ApiResponse<SendDraftResultDto>.SuccessResponse(result,
                $"{result.SuccessCount}/{result.TotalStudents} öğrenciye ödev gönderildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending draft {DraftId}", draftId);
            return ApiResponse<SendDraftResultDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteDraftAsync(int draftId)
    {
        try
        {
            var draft = await _context.HomeworkDrafts.FindAsync(draftId);
            if (draft == null || draft.IsDeleted)
                return ApiResponse<bool>.ErrorResponse("Taslak bulunamadı");

            // Soft delete
            draft.IsDeleted = true;
            draft.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Draft {DraftId} deleted", draftId);
            return ApiResponse<bool>.SuccessResponse(true, "Taslak silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting draft {DraftId}", draftId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    private HomeworkDraftDto MapToDto(HomeworkDraft draft)
    {
        return new HomeworkDraftDto
        {
            Id = draft.Id,
            TeacherId = draft.TeacherId,
            TeacherName = draft.Teacher?.User != null
                ? $"{draft.Teacher.User.FirstName} {draft.Teacher.User.LastName}"
                : string.Empty,
            LessonId = draft.LessonId,
            CourseId = draft.CourseId,
            CourseName = draft.Course?.CourseName,
            Title = draft.Title,
            Description = draft.Description,
            DueDate = draft.DueDate,
            TestDueDate = draft.TestDueDate,
            Students = !string.IsNullOrEmpty(draft.StudentsJson)
                ? JsonSerializer.Deserialize<List<DraftStudentDto>>(draft.StudentsJson, _jsonOptions) ?? new List<DraftStudentDto>()
                : new List<DraftStudentDto>(),
            ContentUrls = !string.IsNullOrEmpty(draft.ContentUrlsJson)
                ? JsonSerializer.Deserialize<List<string>>(draft.ContentUrlsJson, _jsonOptions)
                : null,
            ContentFiles = !string.IsNullOrEmpty(draft.ContentFilesJson)
                ? JsonSerializer.Deserialize<List<DraftFileDto>>(draft.ContentFilesJson, _jsonOptions)
                : null,
            CourseResourceIds = !string.IsNullOrEmpty(draft.CourseResourceIdsJson)
                ? JsonSerializer.Deserialize<List<int>>(draft.CourseResourceIdsJson, _jsonOptions)
                : null,
            TestUrls = !string.IsNullOrEmpty(draft.TestUrlsJson)
                ? JsonSerializer.Deserialize<List<string>>(draft.TestUrlsJson, _jsonOptions)
                : null,
            TestFiles = !string.IsNullOrEmpty(draft.TestFilesJson)
                ? JsonSerializer.Deserialize<List<DraftFileDto>>(draft.TestFilesJson, _jsonOptions)
                : null,
            HasTest = draft.HasTest,
            IsSent = draft.IsSent,
            SentAt = draft.SentAt,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt
        };
    }
}
