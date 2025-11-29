using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Scheduling;

namespace EduPortal.Application.Interfaces;

public interface ISchedulingService
{
    // Student Availability
    Task<ApiResponse<List<StudentAvailabilityDto>>> GetStudentAvailabilityAsync(int studentId);
    Task<ApiResponse<StudentAvailabilityDto>> CreateStudentAvailabilityAsync(CreateStudentAvailabilityDto dto);
    Task<ApiResponse<bool>> DeleteStudentAvailabilityAsync(int id);

    // Teacher Availability
    Task<ApiResponse<List<TeacherAvailabilityDto>>> GetTeacherAvailabilityAsync(int teacherId);
    Task<ApiResponse<TeacherAvailabilityDto>> CreateTeacherAvailabilityAsync(CreateTeacherAvailabilityDto dto);
    Task<ApiResponse<bool>> DeleteTeacherAvailabilityAsync(int id);

    // Lesson Schedule
    Task<ApiResponse<List<LessonScheduleDto>>> GetStudentLessonsAsync(int studentId);
    Task<ApiResponse<List<LessonScheduleDto>>> GetTeacherLessonsAsync(int teacherId);
    Task<ApiResponse<LessonScheduleDto>> CreateLessonScheduleAsync(CreateLessonScheduleDto dto);
    Task<ApiResponse<bool>> CancelLessonAsync(int lessonId);

    // Weekly Calendar View
    Task<ApiResponse<WeeklyCalendarDto>> GetStudentWeeklyCalendarAsync(int studentId, DateTime? weekStartDate = null);
    Task<ApiResponse<WeeklyCalendarDto>> GetTeacherWeeklyCalendarAsync(int teacherId, DateTime? weekStartDate = null);

    // Matching Logic
    Task<ApiResponse<MatchingResultDto>> FindMatchingSlotsAsync(int studentId, int teacherId, DayOfWeek? dayOfWeek = null);
}
