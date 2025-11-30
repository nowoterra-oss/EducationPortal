using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Scheduling;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class SchedulingService : ISchedulingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(ApplicationDbContext context, ILogger<SchedulingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===============================================
    // STUDENT AVAILABILITY
    // ===============================================

    public async Task<ApiResponse<List<StudentAvailabilityDto>>> GetStudentAvailabilityAsync(int studentId)
    {
        try
        {
            var availabilities = await _context.StudentAvailabilities
                .Include(sa => sa.Student).ThenInclude(s => s.User)
                .Where(sa => sa.StudentId == studentId && !sa.IsDeleted)
                .OrderBy(sa => sa.DayOfWeek)
                .ThenBy(sa => sa.StartTime)
                .ToListAsync();

            var dtos = availabilities.Select(sa => new StudentAvailabilityDto
            {
                Id = sa.Id,
                StudentId = sa.StudentId,
                StudentName = $"{sa.Student.User.FirstName} {sa.Student.User.LastName}",
                DayOfWeek = sa.DayOfWeek,
                StartTime = sa.StartTime,
                EndTime = sa.EndTime,
                Type = sa.Type.ToString(),
                Notes = sa.Notes,
                IsRecurring = sa.IsRecurring
            }).ToList();

            return ApiResponse<List<StudentAvailabilityDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student availability");
            return ApiResponse<List<StudentAvailabilityDto>>.ErrorResponse("Müsaitlik bilgileri yüklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<StudentAvailabilityDto>> CreateStudentAvailabilityAsync(CreateStudentAvailabilityDto dto)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            if (student == null)
                return ApiResponse<StudentAvailabilityDto>.ErrorResponse("Öğrenci bulunamadı");

            var availability = new StudentAvailability
            {
                StudentId = dto.StudentId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Type = (AvailabilityType)dto.Type,
                Notes = dto.Notes,
                IsRecurring = dto.IsRecurring
            };

            _context.StudentAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            var result = new StudentAvailabilityDto
            {
                Id = availability.Id,
                StudentId = availability.StudentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                Type = availability.Type.ToString(),
                Notes = availability.Notes,
                IsRecurring = availability.IsRecurring
            };

            return ApiResponse<StudentAvailabilityDto>.SuccessResponse(result, "Müsaitlik eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student availability");
            return ApiResponse<StudentAvailabilityDto>.ErrorResponse("Müsaitlik eklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteStudentAvailabilityAsync(int id)
    {
        try
        {
            var availability = await _context.StudentAvailabilities.FindAsync(id);
            if (availability == null)
                return ApiResponse<bool>.ErrorResponse("Müsaitlik kaydı bulunamadı");

            availability.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Müsaitlik silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student availability");
            return ApiResponse<bool>.ErrorResponse("Müsaitlik silinirken hata oluştu");
        }
    }

    // ===============================================
    // TEACHER AVAILABILITY
    // ===============================================

    public async Task<ApiResponse<List<TeacherAvailabilityDto>>> GetTeacherAvailabilityAsync(int teacherId)
    {
        try
        {
            var availabilities = await _context.TeacherAvailabilities
                .Include(ta => ta.Teacher).ThenInclude(t => t.User)
                .Where(ta => ta.TeacherId == teacherId && !ta.IsDeleted)
                .OrderBy(ta => ta.DayOfWeek)
                .ThenBy(ta => ta.StartTime)
                .ToListAsync();

            var dtos = availabilities.Select(ta => new TeacherAvailabilityDto
            {
                Id = ta.Id,
                TeacherId = ta.TeacherId,
                TeacherName = $"{ta.Teacher.User.FirstName} {ta.Teacher.User.LastName}",
                DayOfWeek = ta.DayOfWeek,
                StartTime = ta.StartTime,
                EndTime = ta.EndTime,
                Type = ta.Type.ToString(),
                Notes = ta.Notes,
                IsRecurring = ta.IsRecurring
            }).ToList();

            return ApiResponse<List<TeacherAvailabilityDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher availability");
            return ApiResponse<List<TeacherAvailabilityDto>>.ErrorResponse("Müsaitlik bilgileri yüklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<TeacherAvailabilityDto>> CreateTeacherAvailabilityAsync(CreateTeacherAvailabilityDto dto)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == dto.TeacherId);

            if (teacher == null)
                return ApiResponse<TeacherAvailabilityDto>.ErrorResponse("Öğretmen bulunamadı");

            var availability = new TeacherAvailability
            {
                TeacherId = dto.TeacherId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Type = (AvailabilityType)dto.Type,
                Notes = dto.Notes,
                IsRecurring = dto.IsRecurring
            };

            _context.TeacherAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            var result = new TeacherAvailabilityDto
            {
                Id = availability.Id,
                TeacherId = availability.TeacherId,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                Type = availability.Type.ToString(),
                Notes = availability.Notes,
                IsRecurring = availability.IsRecurring
            };

            return ApiResponse<TeacherAvailabilityDto>.SuccessResponse(result, "Müsaitlik eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher availability");
            return ApiResponse<TeacherAvailabilityDto>.ErrorResponse("Müsaitlik eklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeacherAvailabilityAsync(int id)
    {
        try
        {
            var availability = await _context.TeacherAvailabilities.FindAsync(id);
            if (availability == null)
                return ApiResponse<bool>.ErrorResponse("Müsaitlik kaydı bulunamadı");

            availability.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Müsaitlik silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher availability");
            return ApiResponse<bool>.ErrorResponse("Müsaitlik silinirken hata oluştu");
        }
    }

    // ===============================================
    // LESSON SCHEDULE
    // ===============================================

    public async Task<ApiResponse<List<LessonScheduleDto>>> GetStudentLessonsAsync(int studentId)
    {
        try
        {
            var lessons = await _context.LessonSchedules
                .Include(ls => ls.Student).ThenInclude(s => s.User)
                .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                .Include(ls => ls.Course)
                .Include(ls => ls.Classroom)
                .Where(ls => !ls.IsDeleted &&
                       ls.Status == LessonStatus.Scheduled &&
                       ls.StudentId == studentId &&
                       (ls.IsRecurring ||
                        (ls.EffectiveTo ?? ls.EffectiveFrom).Date >= DateTime.Today))
                .OrderBy(ls => ls.DayOfWeek)
                .ThenBy(ls => ls.StartTime)
                .ToListAsync();

            var dtos = lessons.Select(MapToLessonDto).ToList();
            return ApiResponse<List<LessonScheduleDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student lessons");
            return ApiResponse<List<LessonScheduleDto>>.ErrorResponse("Ders programı yüklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<List<LessonScheduleDto>>> GetTeacherLessonsAsync(int teacherId)
    {
        try
        {
            var lessons = await _context.LessonSchedules
                .Include(ls => ls.Student).ThenInclude(s => s.User)
                .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                .Include(ls => ls.Course)
                .Include(ls => ls.Classroom)
                .Where(ls => !ls.IsDeleted &&
                       ls.Status == LessonStatus.Scheduled &&
                       ls.TeacherId == teacherId &&
                       (ls.IsRecurring ||
                        (ls.EffectiveTo ?? ls.EffectiveFrom).Date >= DateTime.Today))
                .OrderBy(ls => ls.DayOfWeek)
                .ThenBy(ls => ls.StartTime)
                .ToListAsync();

            var dtos = lessons.Select(MapToLessonDto).ToList();
            return ApiResponse<List<LessonScheduleDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher lessons");
            return ApiResponse<List<LessonScheduleDto>>.ErrorResponse("Ders programı yüklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<LessonScheduleDto>> CreateLessonScheduleAsync(CreateLessonScheduleDto dto)
    {
        try
        {
            // Validate and adjust EffectiveFrom: must be today or future, and match the DayOfWeek
            var today = DateTime.Today;

            // If EffectiveFrom is in the past, calculate the next occurrence of the selected DayOfWeek
            if (dto.EffectiveFrom.Date < today)
            {
                dto.EffectiveFrom = GetNextDayOfWeek(today, dto.DayOfWeek);
                _logger.LogWarning("EffectiveFrom was in the past, adjusted to next {DayOfWeek}: {Date}",
                    dto.DayOfWeek, dto.EffectiveFrom);
            }
            // If EffectiveFrom is today or future but doesn't match DayOfWeek, adjust to next matching day
            else if (dto.EffectiveFrom.DayOfWeek != dto.DayOfWeek)
            {
                dto.EffectiveFrom = GetNextDayOfWeek(dto.EffectiveFrom, dto.DayOfWeek);
                _logger.LogWarning("EffectiveFrom DayOfWeek mismatch, adjusted to: {Date}", dto.EffectiveFrom);
            }

            // Check for conflicts and get the conflicting lesson details
            var conflictingLesson = await _context.LessonSchedules
                .Include(ls => ls.Student).ThenInclude(s => s.User)
                .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                .Include(ls => ls.Course)
                .FirstOrDefaultAsync(ls => !ls.IsDeleted &&
                               ls.Status == LessonStatus.Scheduled &&
                               (ls.StudentId == dto.StudentId || ls.TeacherId == dto.TeacherId) &&
                               ls.DayOfWeek == dto.DayOfWeek &&
                               // Time overlap check
                               ((dto.StartTime >= ls.StartTime && dto.StartTime < ls.EndTime) ||
                                (dto.EndTime > ls.StartTime && dto.EndTime <= ls.EndTime) ||
                                (dto.StartTime <= ls.StartTime && dto.EndTime >= ls.EndTime)) &&
                               // Date range overlap check
                               ls.EffectiveFrom <= (dto.EffectiveTo ?? DateTime.MaxValue) &&
                               (ls.EffectiveTo == null || ls.EffectiveTo >= dto.EffectiveFrom));

            if (conflictingLesson != null)
            {
                var conflictType = conflictingLesson.StudentId == dto.StudentId ? "Öğrenci" : "Öğretmen";
                var studentName = $"{conflictingLesson.Student.User.FirstName} {conflictingLesson.Student.User.LastName}";
                var teacherName = $"{conflictingLesson.Teacher.User.FirstName} {conflictingLesson.Teacher.User.LastName}";
                var courseName = conflictingLesson.Course?.CourseName ?? "Bilinmeyen Ders";
                var timeRange = $"{conflictingLesson.StartTime:hh\\:mm}-{conflictingLesson.EndTime:hh\\:mm}";
                var dateRange = conflictingLesson.EffectiveTo.HasValue
                    ? $"{conflictingLesson.EffectiveFrom:dd.MM.yyyy} - {conflictingLesson.EffectiveTo:dd.MM.yyyy}"
                    : $"{conflictingLesson.EffectiveFrom:dd.MM.yyyy} - Süresiz";

                var errorMessage = $"Çakışma tespit edildi! {conflictType} meşgul. " +
                    $"Mevcut ders: {courseName} ({studentName} - {teacherName}), " +
                    $"Saat: {timeRange}, Tarih: {dateRange}";

                return ApiResponse<LessonScheduleDto>.ErrorResponse(errorMessage);
            }

            var lesson = new LessonSchedule
            {
                StudentId = dto.StudentId,
                TeacherId = dto.TeacherId,
                CourseId = dto.CourseId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                ClassroomId = dto.ClassroomId,
                Notes = dto.Notes,
                Status = LessonStatus.Scheduled,
                IsRecurring = dto.IsRecurring
            };

            _context.LessonSchedules.Add(lesson);
            await _context.SaveChangesAsync();

            // Load related data
            await _context.Entry(lesson).Reference(l => l.Student).LoadAsync();
            await _context.Entry(lesson.Student).Reference(s => s.User).LoadAsync();
            await _context.Entry(lesson).Reference(l => l.Teacher).LoadAsync();
            await _context.Entry(lesson.Teacher).Reference(t => t.User).LoadAsync();
            await _context.Entry(lesson).Reference(l => l.Course).LoadAsync();
            if (lesson.ClassroomId.HasValue)
                await _context.Entry(lesson).Reference(l => l.Classroom).LoadAsync();

            var result = MapToLessonDto(lesson);
            return ApiResponse<LessonScheduleDto>.SuccessResponse(result, "Ders programı oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson schedule");
            return ApiResponse<LessonScheduleDto>.ErrorResponse("Ders oluşturulurken hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> CancelLessonAsync(int lessonId, bool cancelAll = true, DateTime? cancelDate = null)
    {
        try
        {
            var lesson = await _context.LessonSchedules.FindAsync(lessonId);
            if (lesson == null)
                return ApiResponse<bool>.ErrorResponse("Ders bulunamadı");

            if (cancelAll)
            {
                // Tüm tekrarlı dersleri iptal et
                lesson.Status = LessonStatus.Cancelled;
                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Tüm dersler iptal edildi");
            }
            else if (cancelDate.HasValue)
            {
                // Sadece belirtilen tarihteki dersi iptal et
                var dateStr = cancelDate.Value.ToString("yyyy-MM-dd");
                var cancelledDates = string.IsNullOrEmpty(lesson.CancelledDates)
                    ? new List<string>()
                    : lesson.CancelledDates.Split(',').ToList();

                if (!cancelledDates.Contains(dateStr))
                {
                    cancelledDates.Add(dateStr);
                    lesson.CancelledDates = string.Join(",", cancelledDates);
                    await _context.SaveChangesAsync();
                }

                return ApiResponse<bool>.SuccessResponse(true, $"{cancelDate.Value:dd.MM.yyyy} tarihli ders iptal edildi");
            }

            return ApiResponse<bool>.ErrorResponse("İptal tarihi belirtilmeli");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling lesson");
            return ApiResponse<bool>.ErrorResponse("Ders iptal edilirken hata oluştu");
        }
    }

    // ===============================================
    // WEEKLY CALENDAR
    // ===============================================

    public async Task<ApiResponse<WeeklyCalendarDto>> GetStudentWeeklyCalendarAsync(int studentId, DateTime? weekStartDate = null)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return ApiResponse<WeeklyCalendarDto>.ErrorResponse("Öğrenci bulunamadı");

            // Calculate week boundaries
            var weekStart = weekStartDate?.Date ?? GetWeekStartDate(DateTime.Today);
            var weekEnd = weekStart.AddDays(7);

            var calendar = new WeeklyCalendarDto
            {
                EntityId = studentId,
                EntityName = $"{student.User.FirstName} {student.User.LastName}",
                Schedule = new Dictionary<DayOfWeek, List<TimeSlotDto>>()
            };

            // Initialize all days
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                calendar.Schedule[day] = new List<TimeSlotDto>();
            }

            // Get availability
            var availabilities = await _context.StudentAvailabilities
                .Where(sa => sa.StudentId == studentId && !sa.IsDeleted)
                .ToListAsync();

            foreach (var avail in availabilities)
            {
                calendar.Schedule[avail.DayOfWeek].Add(new TimeSlotDto
                {
                    Id = avail.Id,
                    StartTime = avail.StartTime,
                    EndTime = avail.EndTime,
                    Type = avail.Type.ToString(),
                    Title = GetAvailabilityTitle(avail.Type),
                    Color = GetAvailabilityColor(avail.Type),
                    IsClickable = avail.Type == AvailabilityType.Available
                });
            }

            // Get scheduled lessons - filter by week date range
            var lessons = await _context.LessonSchedules
                .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                .Include(ls => ls.Course)
                .Where(ls => ls.StudentId == studentId &&
                            !ls.IsDeleted &&
                            ls.Status == LessonStatus.Scheduled &&
                            ls.EffectiveFrom <= weekEnd &&
                            (ls.EffectiveTo == null || ls.EffectiveTo >= weekStart))
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                calendar.Schedule[lesson.DayOfWeek].Add(new TimeSlotDto
                {
                    Id = lesson.Id,
                    StartTime = lesson.StartTime,
                    EndTime = lesson.EndTime,
                    Type = "Lesson",
                    Title = lesson.Course.CourseName,
                    SubTitle = $"{lesson.Teacher.User.FirstName} {lesson.Teacher.User.LastName}",
                    Color = "#3B82F6",
                    IsClickable = false
                });
            }

            // Sort each day by start time
            foreach (var day in calendar.Schedule.Keys)
            {
                calendar.Schedule[day] = calendar.Schedule[day].OrderBy(s => s.StartTime).ToList();
            }

            return ApiResponse<WeeklyCalendarDto>.SuccessResponse(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student weekly calendar");
            return ApiResponse<WeeklyCalendarDto>.ErrorResponse("Takvim yüklenirken hata oluştu");
        }
    }

    public async Task<ApiResponse<WeeklyCalendarDto>> GetTeacherWeeklyCalendarAsync(int teacherId, DateTime? weekStartDate = null)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
                return ApiResponse<WeeklyCalendarDto>.ErrorResponse("Öğretmen bulunamadı");

            // Calculate week boundaries
            var weekStart = weekStartDate?.Date ?? GetWeekStartDate(DateTime.Today);
            var weekEnd = weekStart.AddDays(7);

            var calendar = new WeeklyCalendarDto
            {
                EntityId = teacherId,
                EntityName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                Schedule = new Dictionary<DayOfWeek, List<TimeSlotDto>>()
            };

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                calendar.Schedule[day] = new List<TimeSlotDto>();
            }

            // Get availability
            var availabilities = await _context.TeacherAvailabilities
                .Where(ta => ta.TeacherId == teacherId && !ta.IsDeleted)
                .ToListAsync();

            foreach (var avail in availabilities)
            {
                calendar.Schedule[avail.DayOfWeek].Add(new TimeSlotDto
                {
                    Id = avail.Id,
                    StartTime = avail.StartTime,
                    EndTime = avail.EndTime,
                    Type = avail.Type.ToString(),
                    Title = GetAvailabilityTitle(avail.Type),
                    Color = GetTeacherAvailabilityColor(avail.Type),
                    IsClickable = avail.Type == AvailabilityType.Available
                });
            }

            // Get scheduled lessons - filter by week date range
            var lessons = await _context.LessonSchedules
                .Include(ls => ls.Student).ThenInclude(s => s.User)
                .Include(ls => ls.Course)
                .Where(ls => ls.TeacherId == teacherId &&
                            !ls.IsDeleted &&
                            ls.Status == LessonStatus.Scheduled &&
                            ls.EffectiveFrom <= weekEnd &&
                            (ls.EffectiveTo == null || ls.EffectiveTo >= weekStart))
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                calendar.Schedule[lesson.DayOfWeek].Add(new TimeSlotDto
                {
                    Id = lesson.Id,
                    StartTime = lesson.StartTime,
                    EndTime = lesson.EndTime,
                    Type = "Lesson",
                    Title = lesson.Course.CourseName,
                    SubTitle = $"{lesson.Student.User.FirstName} {lesson.Student.User.LastName}",
                    Color = "#F59E0B",
                    IsClickable = false
                });
            }

            foreach (var day in calendar.Schedule.Keys)
            {
                calendar.Schedule[day] = calendar.Schedule[day].OrderBy(s => s.StartTime).ToList();
            }

            return ApiResponse<WeeklyCalendarDto>.SuccessResponse(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher weekly calendar");
            return ApiResponse<WeeklyCalendarDto>.ErrorResponse("Takvim yüklenirken hata oluştu");
        }
    }

    // ===============================================
    // MATCHING LOGIC
    // ===============================================

    public async Task<ApiResponse<MatchingResultDto>> FindMatchingSlotsAsync(int studentId, int teacherId, DayOfWeek? dayOfWeek = null)
    {
        try
        {
            var studentAvail = await _context.StudentAvailabilities
                .Where(sa => sa.StudentId == studentId &&
                             !sa.IsDeleted &&
                             sa.Type == AvailabilityType.Available &&
                             (!dayOfWeek.HasValue || sa.DayOfWeek == dayOfWeek.Value))
                .ToListAsync();

            var teacherAvail = await _context.TeacherAvailabilities
                .Where(ta => ta.TeacherId == teacherId &&
                             !ta.IsDeleted &&
                             ta.Type == AvailabilityType.Available &&
                             (!dayOfWeek.HasValue || ta.DayOfWeek == dayOfWeek.Value))
                .ToListAsync();

            var matchingSlots = new List<TimeSlotDto>();

            foreach (var studentSlot in studentAvail)
            {
                foreach (var teacherSlot in teacherAvail)
                {
                    if (studentSlot.DayOfWeek == teacherSlot.DayOfWeek)
                    {
                        // Check if time slots overlap
                        var overlapStart = studentSlot.StartTime > teacherSlot.StartTime
                            ? studentSlot.StartTime
                            : teacherSlot.StartTime;

                        var overlapEnd = studentSlot.EndTime < teacherSlot.EndTime
                            ? studentSlot.EndTime
                            : teacherSlot.EndTime;

                        if (overlapStart < overlapEnd)
                        {
                            matchingSlots.Add(new TimeSlotDto
                            {
                                StartTime = overlapStart,
                                EndTime = overlapEnd,
                                Type = "Match",
                                Title = $"{studentSlot.DayOfWeek} - Müsait",
                                Color = "#10B981",
                                IsClickable = true
                            });
                        }
                    }
                }
            }

            var result = new MatchingResultDto
            {
                MatchingSlots = matchingSlots.OrderBy(s => s.StartTime).ToList(),
                Message = matchingSlots.Any()
                    ? $"{matchingSlots.Count} müsait zaman dilimi bulundu"
                    : "Uygun zaman dilimi bulunamadı"
            };

            return ApiResponse<MatchingResultDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching slots");
            return ApiResponse<MatchingResultDto>.ErrorResponse("Eşleştirme sırasında hata oluştu");
        }
    }

    // ===============================================
    // HELPER METHODS
    // ===============================================

    private static DateTime GetWeekStartDate(DateTime date)
    {
        // Get Monday as start of week
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static DateTime GetNextDayOfWeek(DateTime fromDate, DayOfWeek targetDay)
    {
        // Calculate days until next target day
        var daysUntilTarget = ((int)targetDay - (int)fromDate.DayOfWeek + 7) % 7;
        // If today is the target day, use today (daysUntilTarget would be 0)
        return fromDate.AddDays(daysUntilTarget).Date;
    }

    private string GetAvailabilityTitle(AvailabilityType type) => type switch
    {
        AvailabilityType.Available => "Boş",
        AvailabilityType.Busy => "Dolu",
        AvailabilityType.Unavailable => "Müsait Değil",
        AvailabilityType.School => "Okul",
        AvailabilityType.Break => "Ara",
        _ => "Bilinmeyen"
    };

    private string GetAvailabilityColor(AvailabilityType type) => type switch
    {
        AvailabilityType.Available => "#10B981", // Green
        AvailabilityType.Busy => "#3B82F6",      // Blue
        AvailabilityType.Unavailable => "#EF4444", // Red
        AvailabilityType.School => "#F59E0B",    // Orange
        AvailabilityType.Break => "#6B7280",     // Gray
        _ => "#6B7280"
    };

    private string GetTeacherAvailabilityColor(AvailabilityType type) => type switch
    {
        AvailabilityType.Available => "#10B981",
        AvailabilityType.Busy => "#F59E0B",      // Orange for teacher
        AvailabilityType.Unavailable => "#EF4444",
        _ => "#6B7280"
    };

    private LessonScheduleDto MapToLessonDto(LessonSchedule lesson)
    {
        return new LessonScheduleDto
        {
            Id = lesson.Id,
            StudentId = lesson.StudentId,
            StudentName = $"{lesson.Student.User.FirstName} {lesson.Student.User.LastName}",
            TeacherId = lesson.TeacherId,
            TeacherName = $"{lesson.Teacher.User.FirstName} {lesson.Teacher.User.LastName}",
            CourseId = lesson.CourseId,
            CourseName = lesson.Course.CourseName,
            DayOfWeek = lesson.DayOfWeek,
            StartTime = lesson.StartTime,
            EndTime = lesson.EndTime,
            EffectiveFrom = lesson.EffectiveFrom,
            EffectiveTo = lesson.EffectiveTo,
            Status = lesson.Status.ToString(),
            ClassroomId = lesson.ClassroomId,
            ClassroomName = lesson.Classroom?.RoomNumber
        };
    }
}
