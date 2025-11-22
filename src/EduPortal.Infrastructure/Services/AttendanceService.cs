using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Attendance;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        ApplicationDbContext context,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<AttendanceDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var attendances = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var attendanceDtos = attendances.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<AttendanceDto>(attendanceDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<AttendanceDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all attendance records");
            return ApiResponse<PagedResponse<AttendanceDto>>.ErrorResponse($"Devamsızlık kayıtları getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AttendanceDto>> GetByIdAsync(int id)
    {
        try
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (attendance == null)
            {
                return ApiResponse<AttendanceDto>.ErrorResponse("Devamsızlık kaydı bulunamadı");
            }

            var dto = MapToDto(attendance);
            return ApiResponse<AttendanceDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance by ID: {AttendanceId}", id);
            return ApiResponse<AttendanceDto>.ErrorResponse($"Devamsızlık kaydı getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AttendanceDto>> RecordAttendanceAsync(AttendanceCreateDto dto, int teacherId)
    {
        try
        {
            // Check if student exists
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                return ApiResponse<AttendanceDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            // Check if attendance already exists for this student, course and date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == dto.StudentId &&
                    a.CourseId == dto.CourseId &&
                    a.Date.Date == dto.Date.Date &&
                    !a.IsDeleted);

            Attendance attendance;

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.Status = dto.Status;
                existingAttendance.Notes = dto.Notes;
                existingAttendance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                attendance = existingAttendance;
            }
            else
            {
                // Create new attendance record
                attendance = new Attendance
                {
                    StudentId = dto.StudentId,
                    CourseId = dto.CourseId,
                    TeacherId = teacherId,
                    Date = dto.Date,
                    Status = dto.Status,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _context.Attendances.AddAsync(attendance);
                await _context.SaveChangesAsync();
            }

            // Reload with navigation properties
            attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.Id == attendance.Id);

            var attendanceDto = MapToDto(attendance!);
            return ApiResponse<AttendanceDto>.SuccessResponse(
                attendanceDto,
                existingAttendance != null ? "Yoklama başarıyla güncellendi" : "Yoklama başarıyla kaydedildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording attendance");
            return ApiResponse<AttendanceDto>.ErrorResponse($"Yoklama kaydedilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AttendanceDto>> UpdateAsync(int id, AttendanceCreateDto dto)
    {
        try
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (attendance == null)
            {
                return ApiResponse<AttendanceDto>.ErrorResponse("Devamsızlık kaydı bulunamadı");
            }

            attendance.Status = dto.Status;
            attendance.Notes = dto.Notes;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var attendanceDto = MapToDto(attendance);
            return ApiResponse<AttendanceDto>.SuccessResponse(attendanceDto, "Yoklama başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating attendance: {AttendanceId}", id);
            return ApiResponse<AttendanceDto>.ErrorResponse($"Yoklama güncellenirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (attendance == null)
            {
                return ApiResponse<bool>.ErrorResponse("Devamsızlık kaydı bulunamadı");
            }

            // Soft delete
            attendance.IsDeleted = true;
            attendance.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Yoklama kaydı başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting attendance: {AttendanceId}", id);
            return ApiResponse<bool>.ErrorResponse($"Yoklama silinirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AttendanceDto>>> GetStudentAttendanceAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return ApiResponse<List<AttendanceDto>>.ErrorResponse("Öğrenci bulunamadı");
            }

            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(a => a.Date >= startDate.Value && a.Date <= endDate.Value);
            }

            var attendances = await query.OrderByDescending(a => a.Date).ToListAsync();
            var attendanceDtos = attendances.Select(MapToDto).ToList();

            return ApiResponse<List<AttendanceDto>>.SuccessResponse(attendanceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting student attendance: {StudentId}", studentId);
            return ApiResponse<List<AttendanceDto>>.ErrorResponse($"Öğrenci devamsızlığı getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Dictionary<string, int>>> GetAttendanceSummaryAsync(int studentId)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return ApiResponse<Dictionary<string, int>>.ErrorResponse("Öğrenci bulunamadı");
            }

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .ToListAsync();

            var summary = new Dictionary<string, int>
            {
                { "Geldi", attendances.Count(a => a.Status == AttendanceStatus.Geldi) },
                { "GecGeldi", attendances.Count(a => a.Status == AttendanceStatus.GecGeldi) },
                { "Gelmedi_Mazeretli", attendances.Count(a => a.Status == AttendanceStatus.Gelmedi_Mazeretli) },
                { "Gelmedi_Mazeretsiz", attendances.Count(a => a.Status == AttendanceStatus.Gelmedi_Mazeretsiz) },
                { "DersIptal", attendances.Count(a => a.Status == AttendanceStatus.DersIptal) }
            };

            // Calculate totals
            var totalAttendance = summary.Values.Sum();
            summary.Add("Toplam", totalAttendance);

            // Calculate attendance percentage
            if (totalAttendance > 0)
            {
                var presentCount = summary["Geldi"] + summary["GecGeldi"];
                var attendancePercentage = (int)((presentCount / (double)totalAttendance) * 100);
                summary.Add("DevamYüzdesi", attendancePercentage);
            }
            else
            {
                summary.Add("DevamYüzdesi", 0);
            }

            return ApiResponse<Dictionary<string, int>>.SuccessResponse(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance summary: {StudentId}", studentId);
            return ApiResponse<Dictionary<string, int>>.ErrorResponse($"Devamsızlık özeti getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AttendanceDto>>> GetCourseAttendanceAsync(int courseId)
    {
        try
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.CourseId == courseId && !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var attendanceDtos = attendances.Select(MapToDto).ToList();
            return ApiResponse<List<AttendanceDto>>.SuccessResponse(attendanceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting course attendance: {CourseId}", courseId);
            return ApiResponse<List<AttendanceDto>>.ErrorResponse($"Ders devamsızlığı getirilirken bir hata oluştu: {ex.Message}");
        }
    }

    // Private helper method for manual mapping
    private static AttendanceDto MapToDto(Attendance attendance)
    {
        return new AttendanceDto
        {
            Id = attendance.Id,
            StudentId = attendance.StudentId,
            StudentName = attendance.Student?.User != null
                ? $"{attendance.Student.User.FirstName} {attendance.Student.User.LastName}"
                : string.Empty,
            CourseId = attendance.CourseId,
            CourseName = attendance.Course?.CourseName ?? string.Empty,
            TeacherId = attendance.TeacherId,
            TeacherName = attendance.Teacher?.User != null
                ? $"{attendance.Teacher.User.FirstName} {attendance.Teacher.User.LastName}"
                : string.Empty,
            Date = attendance.Date,
            Status = attendance.Status,
            Notes = attendance.Notes
        };
    }
}
