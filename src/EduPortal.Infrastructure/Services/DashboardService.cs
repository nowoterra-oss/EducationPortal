using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Dashboard;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<AdminDashboardStatsDto>> GetAdminDashboardStatsAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            var stats = new AdminDashboardStatsDto
            {
                TotalStudents = await _context.Students.CountAsync(s => !s.IsDeleted),
                TotalTeachers = await _context.Teachers.CountAsync(t => !t.IsDeleted),
                TotalCourses = await _context.Courses.CountAsync(),
                ActiveStudents = await _context.Students.CountAsync(s => !s.IsDeleted),
                TotalClasses = await _context.Classes.CountAsync(c => !c.IsDeleted),

                PendingPayments = await _context.Payments
                    .CountAsync(p => !p.IsDeleted && p.Status == PaymentStatus.Bekliyor),

                PendingPaymentAmount = await _context.Payments
                    .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Bekliyor)
                    .SumAsync(p => p.Amount),

                UnreadMessages = await _context.Messages
                    .CountAsync(m => !m.IsDeleted && !m.IsRead),

                TodayAttendanceCount = await _context.Attendances
                    .CountAsync(a => !a.IsDeleted && a.Date.Date == today),

                AttendanceRate = await CalculateOverallAttendanceRateAsync()
            };

            // Get recent activities
            stats.RecentActivities = await GetRecentActivitiesAsync(10)
                .ContinueWith(t => t.Result.Data ?? new List<RecentActivityDto>());

            return ApiResponse<AdminDashboardStatsDto>.SuccessResponse(stats);
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminDashboardStatsDto>.ErrorResponse(ex.Message);
        }
    }

    private async Task<decimal> CalculateOverallAttendanceRateAsync()
    {
        var totalAttendance = await _context.Attendances
            .Where(a => !a.IsDeleted)
            .CountAsync();

        if (totalAttendance == 0) return 0;

        var presentCount = await _context.Attendances
            .Where(a => !a.IsDeleted && a.Status == AttendanceStatus.Geldi)
            .CountAsync();

        return Math.Round((decimal)presentCount / totalAttendance * 100, 2);
    }

    public async Task<ApiResponse<TeacherDashboardStatsDto>> GetTeacherDashboardStatsAsync(int teacherId)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted);

            if (teacher == null)
                return ApiResponse<TeacherDashboardStatsDto>.ErrorResponse("Teacher not found");

            var today = DateTime.UtcNow.Date;
            var todayDayOfWeek = today.DayOfWeek;

            var stats = new TeacherDashboardStatsDto
            {
                TeacherId = teacherId,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",

                TotalCourses = await _context.StudentTeacherAssignments
                    .Where(sta => sta.TeacherId == teacherId && !sta.IsDeleted && sta.IsActive)
                    .Select(sta => sta.CourseId)
                    .Distinct()
                    .CountAsync(),

                TotalStudents = await _context.StudentTeacherAssignments
                    .Where(sta => sta.TeacherId == teacherId && !sta.IsDeleted && sta.IsActive)
                    .Select(sta => sta.StudentId)
                    .Distinct()
                    .CountAsync(),

                TodayClassCount = await _context.WeeklySchedules
                    .CountAsync(ws => ws.TeacherId == teacherId &&
                                     !ws.IsDeleted &&
                                     ws.DayOfWeek == todayDayOfWeek),

                PendingHomeworks = await _context.Homeworks
                    .CountAsync(h => h.TeacherId == teacherId &&
                                    !h.IsDeleted &&
                                    h.DueDate >= today),

                PendingGradings = await _context.StudentHomeworkSubmissions
                    .Include(shs => shs.Homework)
                    .CountAsync(shs => shs.Homework.TeacherId == teacherId &&
                                      !shs.IsDeleted &&
                                      shs.Status == HomeworkStatus.TeslimEdildi)
            };

            // Get today's schedule
            stats.TodaySchedule = await _context.WeeklySchedules
                .Where(ws => ws.TeacherId == teacherId &&
                            !ws.IsDeleted &&
                            ws.DayOfWeek == todayDayOfWeek)
                .Include(ws => ws.Course)
                .Include(ws => ws.Class)
                .Include(ws => ws.Classroom)
                .Select(ws => new TodayScheduleDto
                {
                    ScheduleId = ws.Id,
                    CourseName = ws.Course.CourseName,
                    ClassName = ws.Class != null ? ws.Class.ClassName : "N/A",
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                    Classroom = ws.Classroom != null ? ws.Classroom.RoomNumber : null,
                    StudentCount = ws.Class != null ? ws.Class.Students.Count : 0
                })
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return ApiResponse<TeacherDashboardStatsDto>.SuccessResponse(stats);
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDashboardStatsDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDashboardStatsDto>> GetStudentDashboardStatsAsync(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

            if (student == null)
                return ApiResponse<StudentDashboardStatsDto>.ErrorResponse("Student not found");

            var today = DateTime.UtcNow.Date;

            var stats = new StudentDashboardStatsDto
            {
                StudentId = studentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentNo = student.StudentNo,

                EnrolledCourses = await _context.StudentTeacherAssignments
                    .CountAsync(sta => sta.StudentId == studentId &&
                                      !sta.IsDeleted &&
                                      sta.IsActive),

                PendingHomeworks = await _context.StudentHomeworkSubmissions
                    .CountAsync(shs => shs.StudentId == studentId &&
                                      !shs.IsDeleted &&
                                      (shs.Status == HomeworkStatus.Bekliyor ||
                                       shs.Status == HomeworkStatus.GecTeslim)),

                UpcomingExams = await _context.InternalExams
                    .Where(e => !e.IsDeleted && e.ExamDate >= today)
                    .CountAsync(),

                AttendanceRate = await CalculateStudentAttendanceRateAsync(studentId),

                AverageGrade = await CalculateStudentAverageGradeAsync(studentId),

                UnreadMessages = await _context.Messages
                    .CountAsync(m => m.RecipientId == student.UserId &&
                                    !m.IsRead)
            };

            // Get upcoming events
            stats.UpcomingEvents = await _context.CalendarEvents
                .Where(ce => !ce.IsDeleted &&
                            ce.StartDate >= today &&
                            ce.StartDate <= today.AddDays(7))
                .OrderBy(ce => ce.StartDate)
                .Take(5)
                .Select(ce => new UpcomingEventDto
                {
                    EventId = ce.Id,
                    EventType = ce.EventType.ToString(),
                    Title = ce.Title,
                    EventDate = ce.StartDate,
                    Description = ce.Description
                })
                .ToListAsync();

            // Get recent grades
            stats.RecentGrades = await _context.ExamResults
                .Where(er => er.StudentId == studentId && !er.IsDeleted)
                .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
                .OrderByDescending(er => er.Exam.ExamDate)
                .Take(5)
                .Select(er => new RecentGradeDto
                {
                    ExamId = er.ExamId,
                    ExamName = er.Exam.Title,
                    CourseName = er.Exam.Course.CourseName,
                    Score = er.Score,
                    MaxScore = er.Exam.MaxScore,
                    ExamDate = er.Exam.ExamDate
                })
                .ToListAsync();

            return ApiResponse<StudentDashboardStatsDto>.SuccessResponse(stats);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDashboardStatsDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ChartDataDto>> GetEnrollmentChartAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var enrollments = await _context.Students
                .Where(s => !s.IsDeleted &&
                           s.CreatedAt >= startDate &&
                           s.CreatedAt <= endDate)
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var chartData = new ChartDataDto
            {
                ChartType = "line",
                Title = "Öğrenci Kayıt Grafiği",
                Labels = enrollments.Select(e => $"{e.Month}/{e.Year}").ToList(),
                Data = enrollments.Select(e => (decimal)e.Count).ToList()
            };

            return ApiResponse<ChartDataDto>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            return ApiResponse<ChartDataDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ChartDataDto>> GetRevenueChartAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var revenues = await _context.Payments
                .Where(p => !p.IsDeleted &&
                           p.Status == PaymentStatus.Odendi &&
                           p.PaymentDate >= startDate &&
                           p.PaymentDate <= endDate)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var chartData = new ChartDataDto
            {
                ChartType = "bar",
                Title = "Gelir Grafiği",
                Labels = revenues.Select(r => $"{r.Month}/{r.Year}").ToList(),
                Data = revenues.Select(r => r.TotalAmount).ToList()
            };

            return ApiResponse<ChartDataDto>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            return ApiResponse<ChartDataDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ChartDataDto>> GetAttendanceChartAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var attendanceData = await _context.Attendances
                .Where(a => !a.IsDeleted &&
                           a.Date >= startDate &&
                           a.Date <= endDate)
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    PresentCount = g.Count(a => a.Status == AttendanceStatus.Geldi),
                    AbsentCount = g.Count(a => a.Status == AttendanceStatus.Gelmedi_Mazeretsiz),
                    LateCount = g.Count(a => a.Status == AttendanceStatus.GecGeldi)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var chartData = new ChartDataDto
            {
                ChartType = "line",
                Title = "Devamsızlık Grafiği",
                Labels = attendanceData.Select(a => a.Date.ToString("dd/MM")).ToList(),
                Data = attendanceData.Select(a => (decimal)a.PresentCount).ToList()
            };

            return ApiResponse<ChartDataDto>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            return ApiResponse<ChartDataDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count = 10)
    {
        try
        {
            var activities = new List<RecentActivityDto>();

            // Son kayıt olan öğrenciler
            var recentStudents = await _context.Students
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .Take(count / 2)
                .Select(s => new RecentActivityDto
                {
                    ActivityType = "StudentEnrollment",
                    Description = $"{s.User.FirstName} {s.User.LastName} sisteme kaydoldu",
                    Timestamp = s.CreatedAt,
                    UserId = s.UserId,
                    UserName = $"{s.User.FirstName} {s.User.LastName}",
                    EntityType = "Student",
                    EntityId = s.Id
                })
                .ToListAsync();

            // Son ödemeler
            var recentPayments = await _context.Payments
                .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Odendi)
                .Include(p => p.Student)
                .ThenInclude(s => s.User)
                .OrderByDescending(p => p.PaymentDate)
                .Take(count / 2)
                .Select(p => new RecentActivityDto
                {
                    ActivityType = "Payment",
                    Description = $"{p.Student.User.FirstName} {p.Student.User.LastName} ödeme yaptı ({p.Amount:C})",
                    Timestamp = p.PaymentDate,
                    UserId = p.Student.UserId,
                    UserName = $"{p.Student.User.FirstName} {p.Student.User.LastName}",
                    EntityType = "Payment",
                    EntityId = p.Id
                })
                .ToListAsync();

            activities.AddRange(recentStudents);
            activities.AddRange(recentPayments);

            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();

            return ApiResponse<List<RecentActivityDto>>.SuccessResponse(sortedActivities);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<RecentActivityDto>>.ErrorResponse(ex.Message);
        }
    }

    private async Task<decimal> CalculateStudentAttendanceRateAsync(int studentId)
    {
        var totalAttendance = await _context.Attendances
            .CountAsync(a => a.StudentId == studentId && !a.IsDeleted);

        if (totalAttendance == 0) return 0;

        var presentCount = await _context.Attendances
            .CountAsync(a => a.StudentId == studentId &&
                            !a.IsDeleted &&
                            a.Status == AttendanceStatus.Geldi);

        return Math.Round((decimal)presentCount / totalAttendance * 100, 2);
    }

    private async Task<decimal> CalculateStudentAverageGradeAsync(int studentId)
    {
        var grades = await _context.ExamResults
            .Where(er => er.StudentId == studentId && !er.IsDeleted)
            .Include(er => er.Exam)
            .Select(er => (er.Score / er.Exam.MaxScore) * 100)
            .ToListAsync();

        return grades.Any() ? Math.Round(grades.Average(), 2) : 0;
    }
}
