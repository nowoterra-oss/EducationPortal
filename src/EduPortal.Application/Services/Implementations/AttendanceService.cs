using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Attendance;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Services.Implementations;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        IMapper mapper)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<AttendanceDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var attendances = await _attendanceRepository.GetAllAsync();
            var attendancesList = attendances.ToList();

            var totalRecords = attendancesList.Count;
            var pagedAttendances = attendancesList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var attendanceDtos = _mapper.Map<List<AttendanceDto>>(pagedAttendances);
            var pagedResponse = new PagedResponse<AttendanceDto>(attendanceDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<AttendanceDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<AttendanceDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AttendanceDto>> RecordAttendanceAsync(AttendanceCreateDto dto)
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
            var existingAttendance = await _attendanceRepository.GetAttendanceByStudentAndDateAsync(
                dto.StudentId,
                dto.CourseId,
                dto.Date);

            Attendance attendance;

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.Status = dto.Status;
                existingAttendance.Notes = dto.Notes;
                await _attendanceRepository.UpdateAsync(existingAttendance);
                attendance = existingAttendance;
            }
            else
            {
                // Create new attendance record
                attendance = _mapper.Map<Attendance>(dto);
                // Note: TeacherId should be set based on the authenticated user in the API controller
                // For now, we'll set it to 1 as a placeholder
                attendance.TeacherId = 1;
                await _attendanceRepository.AddAsync(attendance);
            }

            var attendanceDto = _mapper.Map<AttendanceDto>(attendance);
            return ApiResponse<AttendanceDto>.SuccessResponse(
                attendanceDto,
                existingAttendance != null ? "Yoklama başarıyla güncellendi" : "Yoklama başarıyla kaydedildi");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceDto>.ErrorResponse($"Hata: {ex.Message}");
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

            IEnumerable<Attendance> attendances;

            if (startDate.HasValue && endDate.HasValue)
            {
                attendances = await _attendanceRepository.GetAttendanceByDateRangeAsync(
                    studentId,
                    startDate.Value,
                    endDate.Value);
            }
            else
            {
                attendances = await _attendanceRepository.GetAttendanceByStudentAsync(studentId);
            }

            var attendanceDtos = _mapper.Map<List<AttendanceDto>>(attendances);
            return ApiResponse<List<AttendanceDto>>.SuccessResponse(attendanceDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AttendanceDto>>.ErrorResponse($"Hata: {ex.Message}");
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

            var stats = await _attendanceRepository.GetAttendanceStatsByStudentAsync(studentId);

            // Convert enum values to string keys for better readability
            var summary = new Dictionary<string, int>
            {
                { "Geldi", stats.GetValueOrDefault((int)AttendanceStatus.Geldi, 0) },
                { "GecGeldi", stats.GetValueOrDefault((int)AttendanceStatus.GecGeldi, 0) },
                { "Gelmedi_Mazeretli", stats.GetValueOrDefault((int)AttendanceStatus.Gelmedi_Mazeretli, 0) },
                { "Gelmedi_Mazeretsiz", stats.GetValueOrDefault((int)AttendanceStatus.Gelmedi_Mazeretsiz, 0) },
                { "DersIptal", stats.GetValueOrDefault((int)AttendanceStatus.DersIptal, 0) }
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
            return ApiResponse<Dictionary<string, int>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AttendanceDto>>> GetCourseAttendanceAsync(int courseId)
    {
        try
        {
            var attendances = await _attendanceRepository.GetAttendanceByCourseAsync(courseId);
            var attendanceDtos = _mapper.Map<List<AttendanceDto>>(attendances);

            return ApiResponse<List<AttendanceDto>>.SuccessResponse(attendanceDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AttendanceDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }
}
