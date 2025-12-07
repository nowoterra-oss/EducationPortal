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
                CreatedAt = DateTime.UtcNow
            };

            await _context.HomeworkAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

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
                    CreatedAt = DateTime.UtcNow
                };
                assignments.Add(assignment);
            }

            await _context.HomeworkAssignments.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();

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
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
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
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
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

    public async Task<ApiResponse<List<HomeworkAssignmentDto>>> GetStudentAssignmentHistoryAsync(int studentId, int teacherId)
    {
        try
        {
            var assignments = await _context.HomeworkAssignments
                .Where(a => a.StudentId == studentId && a.TeacherId == teacherId && !a.IsDeleted)
                .Include(a => a.Homework)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
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

    public async Task<ApiResponse<PagedResponse<HomeworkAssignmentDto>>> GetPendingReviewsAsync(int teacherId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.HomeworkAssignments
                .Where(a => a.TeacherId == teacherId &&
                           a.Status == HomeworkAssignmentStatus.TeslimEdildi &&
                           !a.IsDeleted)
                .Include(a => a.Homework)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .OrderBy(a => a.DueDate);

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
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
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

    public async Task<ApiResponse<HomeworkAssignmentDto>> GradeAssignmentAsync(int teacherId, GradeHomeworkDto dto)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Homework)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && a.TeacherId == teacherId);

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı");

            assignment.CompletionPercentage = dto.CompletionPercentage;
            assignment.TeacherFeedback = dto.TeacherFeedback;
            assignment.Score = dto.Score;
            assignment.GradedAt = DateTime.UtcNow;
            assignment.Status = HomeworkAssignmentStatus.Degerlendirildi;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Öğrenciye bildirim gönder
            await _notificationService.SendAsync(new CreateNotificationDto
            {
                UserId = assignment.Student.UserId,
                Title = "Ödev Değerlendirildi",
                Message = $"'{assignment.Homework.Title}' ödeviniz değerlendirildi. Tamamlanma: %{dto.CompletionPercentage}",
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
            .FirstOrDefaultAsync();

        return course?.Id ?? 1;
    }

    public async Task<ApiResponse<HomeworkAssignmentDto>> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitHomeworkDto dto)
    {
        try
        {
            var assignment = await _context.HomeworkAssignments
                .Include(a => a.Homework)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.SubmissionFiles)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.StudentId == studentId && !a.IsDeleted);

            if (assignment == null)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Ödev bulunamadı veya bu ödev size ait değil");

            if (assignment.Status == HomeworkAssignmentStatus.Degerlendirildi)
                return ApiResponse<HomeworkAssignmentDto>.ErrorResponse("Bu ödev zaten değerlendirilmiş, tekrar teslim edilemez");

            // Teslim bilgilerini güncelle
            assignment.SubmissionText = dto.SubmissionText;
            assignment.SubmissionUrl = dto.SubmissionUrl;
            assignment.SubmittedAt = DateTime.UtcNow;
            assignment.Status = HomeworkAssignmentStatus.TeslimEdildi;
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

    private HomeworkAssignmentDto MapToDto(HomeworkAssignment assignment)
    {
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
            // Değerlendirme
            TeacherFeedback = assignment.TeacherFeedback,
            Score = assignment.Score,
            GradedAt = assignment.GradedAt
        };
    }
}
