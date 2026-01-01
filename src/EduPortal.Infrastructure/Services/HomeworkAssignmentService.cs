using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Calendar;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.DTOs.Notification;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EduPortal.Infrastructure.Services;

public class HomeworkAssignmentService : IHomeworkAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ICalendarService _calendarService;
    private readonly ILogger<HomeworkAssignmentService> _logger;

    public HomeworkAssignmentService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ICalendarService calendarService,
        ILogger<HomeworkAssignmentService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<StudentSummaryDto>>> GetTeacherStudentsAsync(int teacherId)
    {
        try
        {
            var students = await _context.StudentTeacherAssignments
                .Where(sta => sta.TeacherId == teacherId && sta.IsActive && !sta.IsDeleted)
                .Include(sta => sta.Student)
                    .ThenInclude(s => s.User)
                .Include(sta => sta.Course)
                .Select(sta => new StudentSummaryDto
                {
                    Id = sta.Student.Id,
                    StudentNo = sta.Student.StudentNo,
                    FullName = $"{sta.Student.User.FirstName} {sta.Student.User.LastName}",
                    Email = sta.Student.User.Email,
                    CourseName = sta.Course != null ? sta.Course.CourseName : null
                })
                .Distinct()
                .ToListAsync();

            return ApiResponse<List<StudentSummaryDto>>.SuccessResponse(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher students for teacherId: {TeacherId}", teacherId);
            return ApiResponse<List<StudentSummaryDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkAssignmentDto>> CreateAssignmentAsync(int teacherId, CreateHomeworkAssignmentDto dto)
    {
        try
        {
            // Yoklama kontrolünü atla parametresi false ise kontrol yap
            if (!dto.SkipAttendanceCheck)
            {
                // Yoklama ve değerlendirme yapılmış mı kontrol et
                var today = DateTime.UtcNow.Date;
                var hasCompletedAttendance = await _context.Attendances
                    .AnyAsync(a =>
                        a.TeacherId == teacherId &&
                        a.StudentId == dto.StudentId &&
                        a.Date.Date == today &&
                        a.IsEvaluationCompleted &&
                        !a.IsDeleted);

                if (!hasCompletedAttendance)
                {
                    return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(
                        "Ödev vermeden önce bugünkü yoklama ve ders değerlendirmesi tamamlanmalıdır.");
                }
            }

            // Get a default course if not provided
            var courseId = dto.CourseId ?? await GetDefaultCourseIdAsync(teacherId);

            // 1. Önce Homework oluştur
            var homework = new Homework
            {
                Title = dto.Title,
                Description = dto.Description,
                TeacherId = teacherId,
                CourseId = courseId,
                AssignedDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                ResourceUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Homeworks.AddAsync(homework);
            await _context.SaveChangesAsync();

            // 2. HomeworkAssignment oluştur
            var assignment = new HomeworkAssignment
            {
                HomeworkId = homework.Id,
                StudentId = dto.StudentId,
                TeacherId = teacherId,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                Status = HomeworkAssignmentStatus.Atandi,
                CreatedAt = DateTime.UtcNow,
                TestDueDate = dto.TestDueDate,
                HasTest = dto.HasTest ?? false
            };

            // Ders İçeriği kaynaklarını JSON olarak kaydet
            if ((dto.ContentUrls?.Any() == true) || (dto.ContentFiles?.Any() == true))
            {
                var contentFiles = await ProcessContentFilesAsync(dto.ContentFiles, "homework-content");
                var contentResources = new ContentResourcesDto
                {
                    Urls = dto.ContentUrls?.Select(url => new ResourceUrlDto { Url = url }).ToList() ?? new(),
                    Files = contentFiles
                };
                assignment.ContentResourcesJson = JsonSerializer.Serialize(contentResources);
            }

            // Ders Sonu Testi kaynaklarını JSON olarak kaydet
            if ((dto.TestUrls?.Any() == true) || (dto.TestFiles?.Any() == true))
            {
                var testFiles = await ProcessContentFilesAsync(dto.TestFiles, "homework-tests");
                var testInfo = new TestInfoDto
                {
                    DueDate = dto.TestDueDate,
                    Urls = dto.TestUrls?.Select(url => new ResourceUrlDto { Url = url }).ToList() ?? new(),
                    Files = testFiles
                };
                assignment.TestInfoJson = JsonSerializer.Serialize(testInfo);
            }

            await _context.HomeworkAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            // 2.5 Seçilen ders kaynaklarını HomeworkAttachment olarak kaydet
            if (dto.CourseResourceIds?.Any() == true)
            {
                var courseResources = await _context.CourseResources
                    .Where(cr => dto.CourseResourceIds.Contains(cr.Id) && !cr.IsDeleted)
                    .ToListAsync();

                foreach (var resource in courseResources)
                {
                    var attachment = new HomeworkAttachment
                    {
                        HomeworkAssignmentId = assignment.Id,
                        FileName = resource.FileName ?? resource.Title,
                        FilePath = resource.FilePath ?? resource.ResourceUrl,
                        MimeType = resource.MimeType,
                        FileSize = resource.FileSize ?? 0,
                        IsFromCourseResource = true,
                        CourseResourceId = resource.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.HomeworkAttachments.AddAsync(attachment);
                }
                await _context.SaveChangesAsync();
            }

            // 3. Öğrenciye bildirim gönder
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            if (student != null)
            {
                await _notificationService.SendAsync(new CreateNotificationDto
                {
                    UserId = student.UserId,
                    Title = "Yeni Ödev Atandı",
                    Message = $"'{dto.Title}' başlıklı yeni bir ödeviniz var. Son teslim: {dto.DueDate:dd/MM/yyyy}",
                    Type = NotificationType.Homework,
                    RelatedEntityType = "HomeworkAssignment",
                    RelatedEntityId = assignment.Id
                });

                // 4. Takvime ekle
                await AddToStudentCalendarAsync(assignment.Id);
            }

            var result = await GetAssignmentDetailAsync(assignment.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating homework assignment");
            return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<HomeworkAssignmentDto>>> CreateBulkAssignmentsAsync(int teacherId, BulkCreateHomeworkAssignmentDto dto)
    {
        try
        {
            // Yoklama kontrolünü atla parametresi false ise kontrol yap
            if (!dto.SkipAttendanceCheck)
            {
                var today = DateTime.UtcNow.Date;
                var studentsWithoutAttendance = new List<int>();

                foreach (var studentId in dto.StudentIds.Distinct())
                {
                    var hasCompletedAttendance = await _context.Attendances
                        .AnyAsync(a =>
                            a.TeacherId == teacherId &&
                            a.StudentId == studentId &&
                            a.Date.Date == today &&
                            a.IsEvaluationCompleted &&
                            !a.IsDeleted);

                    if (!hasCompletedAttendance)
                    {
                        studentsWithoutAttendance.Add(studentId);
                    }
                }

                if (studentsWithoutAttendance.Any())
                {
                    return ApiResponse<List<HomeworkAssignmentDto>>.ErrorResponse(
                        $"Ödev vermeden önce bugünkü yoklama ve ders değerlendirmesi tamamlanmalıdır. Eksik öğrenci sayısı: {studentsWithoutAttendance.Count}");
                }
            }

            var courseId = dto.CourseId ?? await GetDefaultCourseIdAsync(teacherId);

            // 1. Homework oluştur (tüm öğrenciler için aynı)
            var homework = new Homework
            {
                Title = dto.Title,
                Description = dto.Description,
                TeacherId = teacherId,
                CourseId = courseId,
                AssignedDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                ResourceUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Homeworks.AddAsync(homework);
            await _context.SaveChangesAsync();

            var assignments = new List<HomeworkAssignment>();
            var studentIds = dto.StudentIds.Distinct().ToList();

            // JSON serialize işlemleri (tüm öğrenciler için aynı)
            string? contentResourcesJson = null;
            if ((dto.ContentUrls?.Any() == true) || (dto.ContentFiles?.Any() == true))
            {
                var contentFiles = await ProcessContentFilesAsync(dto.ContentFiles, "homework-content");
                var contentResources = new ContentResourcesDto
                {
                    Urls = dto.ContentUrls?.Select(url => new ResourceUrlDto { Url = url }).ToList() ?? new(),
                    Files = contentFiles
                };
                contentResourcesJson = JsonSerializer.Serialize(contentResources);
            }

            string? testInfoJson = null;
            if ((dto.TestUrls?.Any() == true) || (dto.TestFiles?.Any() == true))
            {
                var testFiles = await ProcessContentFilesAsync(dto.TestFiles, "homework-tests");
                var testInfo = new TestInfoDto
                {
                    DueDate = dto.TestDueDate,
                    Urls = dto.TestUrls?.Select(url => new ResourceUrlDto { Url = url }).ToList() ?? new(),
                    Files = testFiles
                };
                testInfoJson = JsonSerializer.Serialize(testInfo);
            }

            // 2. Her öğrenci için assignment oluştur
            foreach (var studentId in studentIds)
            {
                var assignment = new HomeworkAssignment
                {
                    HomeworkId = homework.Id,
                    StudentId = studentId,
                    TeacherId = teacherId,
                    StartDate = dto.StartDate,
                    DueDate = dto.DueDate,
                    Status = HomeworkAssignmentStatus.Atandi,
                    CreatedAt = DateTime.UtcNow,
                    TestDueDate = dto.TestDueDate,
                    ContentResourcesJson = contentResourcesJson,
                    TestInfoJson = testInfoJson
                };
                assignments.Add(assignment);
            }

            await _context.HomeworkAssignments.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();

            // 2.5 Seçilen ders kaynaklarını HomeworkAttachment olarak kaydet (her assignment için)
            if (dto.CourseResourceIds?.Any() == true)
            {
                var courseResources = await _context.CourseResources
                    .Where(cr => dto.CourseResourceIds.Contains(cr.Id) && !cr.IsDeleted)
                    .ToListAsync();

                foreach (var assignment in assignments)
                {
                    foreach (var resource in courseResources)
                    {
                        var attachment = new HomeworkAttachment
                        {
                            HomeworkAssignmentId = assignment.Id,
                            FileName = resource.FileName ?? resource.Title,
                            FilePath = resource.FilePath ?? resource.ResourceUrl,
                            MimeType = resource.MimeType,
                            FileSize = resource.FileSize ?? 0,
                            IsFromCourseResource = true,
                            CourseResourceId = resource.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.HomeworkAttachments.AddAsync(attachment);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // 3. Öğrencilere bildirim gönder ve takvime ekle
            var students = await _context.Students
                .Include(s => s.User)
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                var student = students.FirstOrDefault(s => s.Id == assignment.StudentId);
                if (student != null)
                {
                    await _notificationService.SendAsync(new CreateNotificationDto
                    {
                        UserId = student.UserId,
                        Title = "Yeni Ödev Atandı",
                        Message = $"'{dto.Title}' başlıklı yeni bir ödeviniz var. Son teslim: {dto.DueDate:dd/MM/yyyy}",
                        Type = NotificationType.Homework,
                        RelatedEntityType = "HomeworkAssignment",
                        RelatedEntityId = assignment.Id
                    });

                    await AddToStudentCalendarAsync(assignment.Id);
                }
            }

            // 4. Sonuçları döndür
            var resultDtos = new List<HomeworkAssignmentDto>();
            foreach (var assignment in assignments)
            {
                var detail = await GetAssignmentDetailAsync(assignment.Id);
                if (detail.Success && detail.Data != null)
                {
                    resultDtos.Add(detail.Data);
                }
            }

            return ApiResponse<List<HomeworkAssignmentDto>>.SuccessResponse(resultDtos, $"{resultDtos.Count} ödev başarıyla atandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk homework assignments");
            return ApiResponse<List<HomeworkAssignmentDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetTeacherAssignmentsAsync(int teacherId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => a.TeacherId == teacherId && !a.IsDeleted)
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<HomeworkAssignmentDto>(dtos, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher assignments");
            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetStudentAssignmentsAsync(int studentId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<HomeworkAssignmentDto>(dtos, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student assignments");
            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<HomeworkAssignmentDto>>> GetStudentAssignmentHistoryAsync(int studentId, int? teacherId = null)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => a.StudentId == studentId && !a.IsDeleted);

            // teacherId belirtilmişse filtrele, yoksa tüm öğretmenlerin ödevlerini getir
            if (teacherId.HasValue)
            {
                query = query.Where(a => a.TeacherId == teacherId.Value);
            }

            var assignments = await query
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var dtos = assignments.Select(MapToDto).ToList();
            return ApiResponse<List<HomeworkAssignmentDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student assignment history");
            return ApiResponse<List<HomeworkAssignmentDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> MarkAsViewedAsync(int assignmentId, int studentId, string? ipAddress, string? userAgent)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.StudentId == studentId);

            if (assignment == null)
                return ApiResponse<bool>.ErrorResponse("Ödev bulunamadı");

            if (!assignment.IsViewed)
            {
                assignment.IsViewed = true;
                assignment.ViewedAt = DateTime.UtcNow;
                assignment.Status = HomeworkAssignmentStatus.Goruldu;

                // Log kaydet
                var viewLog = new HomeworkViewLog
                {
                    HomeworkAssignmentId = assignmentId,
                    StudentId = studentId,
                    ViewedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _context.HomeworkViewLogs.AddAsync(viewLog);
                await _context.SaveChangesAsync();
            }

            return ApiResponse<bool>.SuccessResponse(true, "Ödev görüldü olarak işaretlendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking assignment as viewed");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetPendingReviewsAsync(int? teacherId, int pageNumber, int pageSize, string? status = null)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => !a.IsDeleted);

            // teacherId belirtilmişse filtrele (öğretmen için), yoksa tüm öğretmenlerin ödevleri (admin için)
            if (teacherId.HasValue)
            {
                query = query.Where(a => a.TeacherId == teacherId.Value);
            }

            // status parametresine göre filtrele
            if (status == "all")
            {
                // Tüm ödevler - frontend filtreleme yapacak
                // Hiçbir status filtresi uygulanmaz
            }
            else if (status == "completed")
            {
                // Sadece tamamlanan ödevler (Degerlendirildi)
                query = query.Where(a => a.Status == HomeworkAssignmentStatus.Degerlendirildi);
            }
            else if (status == "pending")
            {
                // Henüz teslim edilmemiş ödevler
                query = query.Where(a => a.Status == HomeworkAssignmentStatus.Atandi ||
                                        a.Status == HomeworkAssignmentStatus.Goruldu ||
                                        a.Status == HomeworkAssignmentStatus.DevamEdiyor);
            }
            else
            {
                // Varsayılan: Teslim edilmiş ve değerlendirilmeyi bekleyenler (TeslimEdildi veya TestTeslimEdildi)
                query = query.Where(a => a.Status == HomeworkAssignmentStatus.TeslimEdildi ||
                                        a.Status == HomeworkAssignmentStatus.TestTeslimEdildi);
            }

            var orderedQuery = query
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .OrderBy(a => a.DueDate);

            var totalCount = await orderedQuery.CountAsync();
            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<HomeworkAssignmentDto>(dtos, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending reviews");
            return ApiResponse<PagedResponse<HomeworkAssignmentDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkAssignmentDto>> GetAssignmentDetailAsync(int assignmentId)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı");

            return ApiResponse<HomeworkAssignmentDto>.SuccessResponse(MapToDto(assignment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment detail");
            return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkAssignmentDto>> GradeAssignmentAsync(int? teacherId, GradeHomeworkDto dto, bool isAdmin = false)
    {
        try
        {
            HomeworkAssignment? assignment;

            if (isAdmin)
            {
                // Admin tüm ödevleri değerlendirebilir - teacherId kontrolü yapılmaz
                assignment = await _context.HomeworkAssignments
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.Homework)
                        .ThenInclude(h => h.Course)
                    .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && !a.IsDeleted);
            }
            else
            {
                // Öğretmen sadece kendi ödevlerini değerlendirebilir
                assignment = await _context.HomeworkAssignments
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.Homework)
                        .ThenInclude(h => h.Course)
                    .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && a.TeacherId == teacherId && !a.IsDeleted);
            }

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı");

            // Genel değerlendirme
            assignment.CompletionPercentage = dto.CompletionPercentage;
            assignment.TeacherFeedback = dto.TeacherFeedback;
            assignment.Score = dto.Score;
            assignment.UpdatedAt = DateTime.UtcNow;

            // Ayrı değerlendirmeler (ödev ve test için)
            assignment.HomeworkScore = dto.HomeworkScore;
            assignment.HomeworkFeedback = dto.HomeworkFeedback;
            assignment.TestScore = dto.TestScore;
            assignment.TestFeedback = dto.TestFeedback;

            // KRITIK: Status değişiklik mantığı
            // HasTest=false ise veya test teslim edildiyse → Degerlendirildi
            // HasTest=true ve test henüz teslim edilmediyse → TeslimEdildi olarak kalır
            string notificationMessage;

            bool canCompleteGrading =
                !assignment.HasTest || // Test yok
                assignment.Status == HomeworkAssignmentStatus.TestTeslimEdildi || // Test teslim edildi
                !string.IsNullOrEmpty(assignment.TestSubmissionUrl) || // Test URL ile teslim edildi
                !string.IsNullOrEmpty(assignment.TestSubmissionText); // Test metin ile teslim edildi

            if (canCompleteGrading)
            {
                // Test yok veya test de teslim edildi → tam değerlendirme yapılabilir
                assignment.Status = HomeworkAssignmentStatus.Degerlendirildi;
                assignment.GradedAt = DateTime.UtcNow;
                notificationMessage = $"'{assignment.Homework.Title}' ödeviniz değerlendirildi. Tamamlanma: %{dto.CompletionPercentage}";
            }
            else
            {
                // HasTest=true ve test henüz teslim edilmedi → ödev puanı verilir ama status değişmez
                // Öğrenci hala test çözecek
                notificationMessage = $"'{assignment.Homework.Title}' ödevinizin ödev kısmı değerlendirildi. Lütfen testi de tamamlayın.";
            }

            await _context.SaveChangesAsync();

            // Öğrenciye bildirim gönder
            await _notificationService.SendAsync(new CreateNotificationDto
            {
                UserId = assignment.Student.UserId,
                Title = assignment.Status == HomeworkAssignmentStatus.Degerlendirildi ? "Ödev Değerlendirildi" : "Ödev Puanı Verildi",
                Message = notificationMessage,
                Type = NotificationType.Success,
                RelatedEntityType = "HomeworkAssignment",
                RelatedEntityId = assignment.Id
            });

            return await GetAssignmentDetailAsync(assignment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error grading assignment");
            return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentHomeworkPerformanceDto>> GetStudentPerformanceAsync(
        int studentId, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => a.StudentId == studentId && !a.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var assignments = await query
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();

            var student = assignments.FirstOrDefault()?.Student;

            var performance = new StudentHomeworkPerformanceDto
            {
                StudentId = studentId,
                StudentName = student != null ? $"{student.User.FirstName} {student.User.LastName}" : "",
                TotalAssignments = assignments.Count,
                CompletedOnTime = assignments.Count(a =>
                    (a.Status == HomeworkAssignmentStatus.TeslimEdildi ||
                    a.Status == HomeworkAssignmentStatus.Degerlendirildi) &&
                    a.GradedAt <= a.DueDate),
                CompletedLate = assignments.Count(a =>
                    (a.Status == HomeworkAssignmentStatus.TeslimEdildi ||
                    a.Status == HomeworkAssignmentStatus.Degerlendirildi) &&
                    a.GradedAt > a.DueDate),
                Pending = assignments.Count(a =>
                    a.Status == HomeworkAssignmentStatus.Atandi ||
                    a.Status == HomeworkAssignmentStatus.Goruldu ||
                    a.Status == HomeworkAssignmentStatus.DevamEdiyor),
                Overdue = assignments.Count(a =>
                    a.DueDate < DateTime.UtcNow &&
                    a.Status != HomeworkAssignmentStatus.TeslimEdildi &&
                    a.Status != HomeworkAssignmentStatus.Degerlendirildi),
                AverageCompletionPercentage = assignments.Any()
                    ? assignments.Average(a => a.CompletionPercentage) : 0,
                AverageScore = assignments.Where(a => a.Score.HasValue).Any()
                    ? assignments.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0
            };

            performance.OnTimeRate = performance.TotalAssignments > 0
                ? (double)performance.CompletedOnTime / performance.TotalAssignments * 100 : 0;

            // Aylık breakdown
            var monthlyGroups = assignments
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
                .Select(g => new MonthlyPerformanceDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    TotalAssignments = g.Count(),
                    Completed = g.Count(a => a.Status == HomeworkAssignmentStatus.Degerlendirildi),
                    AverageScore = g.Where(a => a.Score.HasValue).Any()
                        ? g.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            performance.MonthlyBreakdown = monthlyGroups;

            return ApiResponse<StudentHomeworkPerformanceDto>.SuccessResponse(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student performance");
            return ApiResponse<StudentHomeworkPerformanceDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkPerformanceChartDto>> GetPerformanceChartDataAsync(int studentId, int months = 6)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var assignments = await _context.HomeworkAssignments
                .Where(a => a.StudentId == studentId && a.CreatedAt >= startDate && !a.IsDeleted)
                .ToListAsync();

            var chartData = new HomeworkPerformanceChartDto();

            for (int i = months - 1; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthAssignments = assignments
                    .Where(a => a.CreatedAt.Year == monthDate.Year && a.CreatedAt.Month == monthDate.Month)
                    .ToList();

                chartData.Labels.Add(monthDate.ToString("MMM yyyy"));
                chartData.CompletedData.Add(monthAssignments.Count(a =>
                    a.Status == HomeworkAssignmentStatus.TeslimEdildi ||
                    a.Status == HomeworkAssignmentStatus.Degerlendirildi));
                chartData.PendingData.Add(monthAssignments.Count(a =>
                    a.Status == HomeworkAssignmentStatus.Atandi ||
                    a.Status == HomeworkAssignmentStatus.Goruldu ||
                    a.Status == HomeworkAssignmentStatus.DevamEdiyor));
                chartData.OverdueData.Add(monthAssignments.Count(a =>
                    a.DueDate < DateTime.UtcNow &&
                    a.Status != HomeworkAssignmentStatus.TeslimEdildi &&
                    a.Status != HomeworkAssignmentStatus.Degerlendirildi));
                chartData.AverageScores.Add(monthAssignments.Where(a => a.Score.HasValue).Any()
                    ? monthAssignments.Where(a => a.Score.HasValue).Average(a => a.Score!.Value)
                    : 0);
            }

            return ApiResponse<HomeworkPerformanceChartDto>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance chart data");
            return ApiResponse<HomeworkPerformanceChartDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<int>> SendDueDateRemindersAsync()
    {
        try
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var dayAfter = tomorrow.AddDays(1);

            var dueSoonAssignments = await _context.HomeworkAssignments
                .Where(a =>
                    !a.ReminderSent &&
                    a.DueDate >= tomorrow &&
                    a.DueDate < dayAfter &&
                    a.Status != HomeworkAssignmentStatus.TeslimEdildi &&
                    a.Status != HomeworkAssignmentStatus.Degerlendirildi &&
                    !a.IsDeleted)
                .Include(a => a.Homework)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();

            int sentCount = 0;

            foreach (var assignment in dueSoonAssignments)
            {
                await _notificationService.SendAsync(new CreateNotificationDto
                {
                    UserId = assignment.Student.UserId,
                    Title = "Ödev Hatırlatması",
                    Message = $"'{assignment.Homework.Title}' ödevinin son teslim tarihi yarın! Lütfen teslim etmeyi unutmayın.",
                    Type = NotificationType.Warning,
                    RelatedEntityType = "HomeworkAssignment",
                    RelatedEntityId = assignment.Id
                });

                assignment.ReminderSent = true;
                assignment.ReminderSentAt = DateTime.UtcNow;
                sentCount++;
            }

            await _context.SaveChangesAsync();

            return ApiResponse<int>.SuccessResponse(sentCount, $"{sentCount} hatırlatma gönderildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending due date reminders");
            return ApiResponse<int>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> AddToStudentCalendarAsync(int assignmentId)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Homework)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return ApiResponse<bool>.ErrorResponse("Ödev bulunamadı");

            // Calendar event oluştur
            await _calendarService.CreateAsync(new CreateCalendarEventDto
            {
                StudentId = assignment.StudentId,
                Scope = EventScope.Personal,
                Title = $"Ödev: {assignment.Homework.Title}",
                Description = $"Son Teslim Tarihi\n{assignment.Homework.Description ?? ""}",
                EventType = EventType.Odev,
                StartDate = assignment.DueDate.Date,
                EndDate = assignment.DueDate.Date.AddHours(23).AddMinutes(59),
                AllDayEvent = false,
                Priority = Priority.Yuksek
            });

            return ApiResponse<bool>.SuccessResponse(true, "Takvime eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to calendar");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    private async Task<int> GetDefaultCourseIdAsync(int teacherId)
    {
        var course = await _context.Courses
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync();

        return course?.Id ?? 1;
    }

    public async Task<ApiResponse<HomeworkAssignmentDto>> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitHomeworkDto dto)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.SubmissionFiles)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.StudentId == studentId && !a.IsDeleted);

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı veya bu ödev size ait değil");

            if (assignment.Status == HomeworkAssignmentStatus.Degerlendirildi)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Bu ödev zaten değerlendirilmiş, tekrar teslim edilemez");

            // Teslim bilgilerini güncelle
            assignment.SubmissionText = dto.SubmissionText;
            assignment.SubmissionUrl = dto.SubmissionUrl;
            assignment.SubmittedAt = DateTime.UtcNow;

            // HasTest kontrolü - test yoksa direkt TestTeslimEdildi statüsüne geç
            if (assignment.HasTest)
            {
                assignment.Status = HomeworkAssignmentStatus.TeslimEdildi; // Test bekleniyor
            }
            else
            {
                assignment.Status = HomeworkAssignmentStatus.TestTeslimEdildi; // Değerlendirme bekleniyor (test yok)
            }

            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Öğretmene bildirim gönder
            await _notificationService.SendAsync(new CreateNotificationDto
            {
                UserId = assignment.Teacher.UserId,
                Title = "Ödev Teslim Edildi",
                Message = $"{assignment.Student.User.FirstName} {assignment.Student.User.LastName} '{assignment.Homework.Title}' ödevini teslim etti.",
                Type = NotificationType.Info,
                RelatedEntityType = "HomeworkAssignment",
                RelatedEntityId = assignment.Id
            });

            return ApiResponse<HomeworkAssignmentDto>.SuccessResponse(MapToDto(assignment), "Ödev başarıyla teslim edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting assignment {AssignmentId}", assignmentId);
            return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FileUploadResultDto>> UploadSubmissionFileAsync(int assignmentId, int studentId, Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.StudentId == studentId && !a.IsDeleted);

            if (assignment == null)
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Ödev bulunamadı veya bu ödev size ait değil");

            if (assignment.Status == HomeworkAssignmentStatus.Degerlendirildi)
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Bu ödev zaten değerlendirilmiş, dosya yüklenemez");

            // Dosya boyutu kontrolü (10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (fileStream.Length > maxFileSize)
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Dosya boyutu 10MB'dan büyük olamaz");

            // İzin verilen dosya tipleri
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Bu dosya türü desteklenmiyor. İzin verilen türler: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, TXT, JPG, PNG, GIF, ZIP, RAR");

            // Dosyayı kaydet
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "homework-submissions", assignmentId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            var fileUrl = $"/uploads/homework-submissions/{assignmentId}/{uniqueFileName}";

            // Veritabanına kaydet
            var submissionFile = new HomeworkSubmissionFile
            {
                HomeworkAssignmentId = assignmentId,
                FileName = fileName,
                FileUrl = fileUrl,
                FileSize = fileStream.Length,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            };

            await _context.HomeworkSubmissionFiles.AddAsync(submissionFile);
            await _context.SaveChangesAsync();

            var result = new FileUploadResultDto
            {
                Success = true,
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = fileStream.Length,
                ContentType = contentType
            };

            return ApiResponse<FileUploadResultDto>.SuccessResponse(result, "Dosya başarıyla yüklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading submission file for assignment {AssignmentId}", assignmentId);
            return ApiResponse<FileUploadResultDto>.ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Assignment'tan bağımsız dosya yükleme (frontend akışı için).
    /// Öğrenci önce dosya yükler, dönen URL ile submit çağırır.
    /// </summary>
    public async Task<ApiResponse<FileUploadResultDto>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Dosya boyutu kontrolü (50MB)
            const long maxFileSize = 50 * 1024 * 1024;
            if (fileStream.Length > maxFileSize)
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Dosya boyutu 50MB'dan büyük olamaz");

            // İzin verilen dosya tipleri
            var allowedExtensions = new[] {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt",
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
                ".zip", ".rar", ".7z",
                ".mp3", ".wav", ".ogg",
                ".mp4", ".avi", ".mov", ".wmv", ".webm"
            };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return ApiResponse<FileUploadResultDto>.ErrorResponse(
                    "Bu dosya türü desteklenmiyor. İzin verilen türler: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, TXT, resimler, arşivler, ses ve video dosyaları");

            // Dosyayı kaydet
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "homework-submissions");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            var fileUrl = $"/uploads/homework-submissions/{uniqueFileName}";

            var result = new FileUploadResultDto
            {
                Success = true,
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = fileStream.Length,
                ContentType = contentType
            };

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FileUrl}", fileName, fileUrl);
            return ApiResponse<FileUploadResultDto>.SuccessResponse(result, "Dosya başarıyla yüklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return ApiResponse<FileUploadResultDto>.ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Test teslimi. Sadece TeslimEdildi durumundaki ödevler için test teslim edilebilir.
    /// </summary>
    public async Task<ApiResponse<HomeworkAssignmentDto>> SubmitTestAsync(int assignmentId, int studentId, SubmitTestDto dto)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Homework)
                    .ThenInclude(h => h.Course)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.SubmissionFiles)
                .Include(a => a.Attachments)
                    .ThenInclude(att => att.CourseResource)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.StudentId == studentId && !a.IsDeleted);

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı veya bu ödev size ait değil");

            // Sadece TeslimEdildi durumundaki ödevler için test teslim edilebilir
            if (assignment.Status != HomeworkAssignmentStatus.TeslimEdildi)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Sadece ödev teslim edildikten sonra test teslim edilebilir");

            // Ödevde test var mı kontrol et
            if (string.IsNullOrEmpty(assignment.TestInfoJson) && !assignment.TestDueDate.HasValue)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Bu ödevde test bulunmamaktadır");

            // Test teslim bilgilerini güncelle
            assignment.TestSubmissionText = dto.TestSubmissionText;
            assignment.TestSubmissionUrl = dto.TestSubmissionUrl;
            assignment.TestSubmittedAt = DateTime.UtcNow;
            assignment.Status = HomeworkAssignmentStatus.TestTeslimEdildi;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Öğretmene bildirim gönder
            await _notificationService.SendAsync(new CreateNotificationDto
            {
                UserId = assignment.Teacher.UserId,
                Title = "Test Teslim Edildi",
                Message = $"{assignment.Student.User.FirstName} {assignment.Student.User.LastName} '{assignment.Homework.Title}' ödevinin testini teslim etti.",
                Type = NotificationType.Info,
                RelatedEntityType = "HomeworkAssignment",
                RelatedEntityId = assignment.Id
            });

            return ApiResponse<HomeworkAssignmentDto>.SuccessResponse(MapToDto(assignment), "Test başarıyla teslim edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting test for assignment {AssignmentId}", assignmentId);
            return ApiResponse<HomeworkAssignmentDto>.ErrorResponse(ex.Message);
        }
    }

    private HomeworkAssignmentDto MapToDto(HomeworkAssignment assignment)
    {
        // Content Resources parsing - doğrudan JSON'dan deserialize
        ContentResourcesDto? contentResources = null;
        if (!string.IsNullOrEmpty(assignment.ContentResourcesJson))
        {
            try
            {
                contentResources = JsonSerializer.Deserialize<ContentResourcesDto>(assignment.ContentResourcesJson);
            }
            catch
            {
                contentResources = null;
            }
        }

        // HomeworkAttachments'tan gelen CourseResource'ları ContentResources'a ekle
        if (assignment.Attachments?.Any(a => a.IsFromCourseResource && a.CourseResource != null) == true)
        {
            contentResources ??= new ContentResourcesDto();

            foreach (var attachment in assignment.Attachments.Where(a => a.IsFromCourseResource && a.CourseResource != null))
            {
                var resource = attachment.CourseResource!;
                var resourceType = resource.ResourceType?.ToLowerInvariant();

                // URL tipi kaynaklar (Video, Link gibi)
                if (resourceType == "link" || resourceType == "video" || resourceType == "url")
                {
                    if (!contentResources.Urls.Any(u => u.Url == resource.ResourceUrl))
                    {
                        contentResources.Urls.Add(new ResourceUrlDto
                        {
                            Url = resource.ResourceUrl,
                            Title = resource.Title
                        });
                    }
                }
                else
                {
                    // Dosya tipi kaynaklar (PDF, Document gibi)
                    var downloadUrl = resource.FilePath ?? resource.ResourceUrl;
                    if (!string.IsNullOrEmpty(downloadUrl) && !contentResources.Files.Any(f => f.DownloadUrl == downloadUrl))
                    {
                        contentResources.Files.Add(new ResourceFileDto
                        {
                            Name = resource.FileName ?? resource.Title,
                            DownloadUrl = downloadUrl,
                            FileSize = resource.FileSize
                        });
                    }
                }
            }
        }

        // Test Info parsing - doğrudan JSON'dan deserialize
        TestInfoDto? testInfo = null;
        if (!string.IsNullOrEmpty(assignment.TestInfoJson))
        {
            try
            {
                testInfo = JsonSerializer.Deserialize<TestInfoDto>(assignment.TestInfoJson);
                if (testInfo != null)
                {
                    testInfo.DueDate = assignment.TestDueDate;
                }
            }
            catch
            {
                testInfo = null;
            }
        }
        else if (assignment.TestDueDate.HasValue)
        {
            // Sadece TestDueDate varsa boş bir testInfo oluştur
            testInfo = new TestInfoDto { DueDate = assignment.TestDueDate };
        }

        var hasContentResources = contentResources != null && (contentResources.Urls.Any() || contentResources.Files.Any());
        var hasTest = testInfo != null && (testInfo.DueDate.HasValue || testInfo.Urls.Any() || testInfo.Files.Any());

        return new HomeworkAssignmentDto
        {
            Id = assignment.Id,
            HomeworkId = assignment.HomeworkId,
            HomeworkTitle = assignment.Homework?.Title ?? "",
            HomeworkDescription = assignment.Homework?.Description,
            StudentId = assignment.StudentId,
            StudentName = assignment.Student != null
                ? $"{assignment.Student.User.FirstName} {assignment.Student.User.LastName}"
                : "",
            StudentNo = assignment.Student?.StudentNo ?? "",
            TeacherId = assignment.TeacherId,
            TeacherName = assignment.Teacher != null
                ? $"{assignment.Teacher.User.FirstName} {assignment.Teacher.User.LastName}"
                : "",
            // Ders bilgisi
            CourseId = assignment.Homework?.CourseId,
            CourseName = assignment.Homework?.Course?.CourseName,
            StartDate = assignment.StartDate,
            DueDate = assignment.DueDate,
            IsViewed = assignment.IsViewed,
            ViewedAt = assignment.ViewedAt,
            Status = assignment.Status.ToString(),
            CompletionPercentage = assignment.CompletionPercentage,
            // Öğretmenin yüklediği dosya (Homework'tan)
            AttachmentUrl = assignment.Homework?.ResourceUrl,
            // Öğrenci teslimi
            SubmissionText = assignment.SubmissionText,
            SubmissionUrl = assignment.SubmissionUrl,
            SubmittedAt = assignment.SubmittedAt,
            SubmissionFiles = assignment.SubmissionFiles?.Select(f => new SubmissionFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                UploadedAt = f.UploadedAt
            }).ToList() ?? new List<SubmissionFileDto>(),
            // Test teslimi
            TestSubmissionText = assignment.TestSubmissionText,
            TestSubmissionUrl = assignment.TestSubmissionUrl,
            TestSubmittedAt = assignment.TestSubmittedAt,
            // Değerlendirme
            TeacherFeedback = assignment.TeacherFeedback,
            Score = assignment.Score,
            GradedAt = assignment.GradedAt,
            // Ayrı değerlendirmeler
            HomeworkScore = assignment.HomeworkScore,
            HomeworkFeedback = assignment.HomeworkFeedback,
            TestScore = assignment.TestScore,
            TestFeedback = assignment.TestFeedback,
            // Ders İçeriği ve Test
            ContentResources = contentResources,
            TestInfo = testInfo,
            HasContentResources = hasContentResources,
            HasTest = hasTest
        };
    }

    private List<ResourceUrlDto> ParseUrls(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<ResourceUrlDto>();

        try
        {
            var urls = JsonSerializer.Deserialize<List<string>>(json);
            return urls?.Select(u => new ResourceUrlDto { Url = u, Title = ExtractTitleFromUrl(u) }).ToList()
                ?? new List<ResourceUrlDto>();
        }
        catch
        {
            return new List<ResourceUrlDto>();
        }
    }

    private List<ResourceFileDto> ParseFiles(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<ResourceFileDto>();

        try
        {
            var files = JsonSerializer.Deserialize<List<JsonElement>>(json);
            return files?.Select(f => new ResourceFileDto
            {
                Name = f.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                DownloadUrl = f.TryGetProperty("url", out var url) ? url.GetString() ?? "" : ""
            }).ToList() ?? new List<ResourceFileDto>();
        }
        catch
        {
            return new List<ResourceFileDto>();
        }
    }

    private string ExtractTitleFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            if (uri.Host.Contains("youtube") || uri.Host.Contains("youtu.be"))
                return "YouTube Video";
            if (uri.Host.Contains("vimeo"))
                return "Vimeo Video";
            return uri.Host.Replace("www.", "");
        }
        catch
        {
            return "Link";
        }
    }

    /// <summary>
    /// Base64 encoded dosyayı kaydeder ve URL döner
    /// </summary>
    private async Task<string> SaveBase64FileAsync(string base64Content, string fileName, string subFolder)
    {
        try
        {
            // Base64 header'ını temizle (data:application/pdf;base64, gibi)
            var base64Data = base64Content;
            if (base64Content.Contains(","))
            {
                base64Data = base64Content.Split(',')[1];
            }

            var fileBytes = Convert.FromBase64String(base64Data);
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving base64 file: {FileName}", fileName);
            return string.Empty;
        }
    }

    /// <summary>
    /// ContentFileDto listesini ResourceFileDto listesine dönüştürür, gerekirse dosyaları kaydeder
    /// </summary>
    private async Task<List<ResourceFileDto>> ProcessContentFilesAsync(List<ContentFileDto>? files, string subFolder)
    {
        if (files == null || !files.Any())
            return new List<ResourceFileDto>();

        var result = new List<ResourceFileDto>();

        foreach (var file in files)
        {
            string downloadUrl;

            if (!string.IsNullOrEmpty(file.Base64))
            {
                // Base64 dosyası - kaydet
                downloadUrl = await SaveBase64FileAsync(file.Base64, file.Name, subFolder);
            }
            else if (!string.IsNullOrEmpty(file.Url))
            {
                // Zaten yüklenmiş dosya
                downloadUrl = file.Url;
            }
            else
            {
                // Ne base64 ne de URL var - boş bırak
                downloadUrl = string.Empty;
            }

            result.Add(new ResourceFileDto
            {
                Name = file.Name,
                DownloadUrl = downloadUrl
            });
        }

        return result;
    }
}
