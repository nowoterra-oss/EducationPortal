using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentGroup;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class StudentGroupService : IStudentGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentGroupService> _logger;

    public StudentGroupService(ApplicationDbContext context, ILogger<StudentGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===============================================
    // GRUP CRUD ISLEMLERI
    // ===============================================

    public async Task<ApiResponse<PagedResponse<StudentGroupDto>>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false)
    {
        try
        {
            var query = _context.StudentGroups
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Student)
                        .ThenInclude(s => s.User)
                .Where(g => !g.IsDeleted);

            if (!includeInactive)
                query = query.Where(g => g.IsActive);

            var totalCount = await query.CountAsync();

            var groups = await query
                .OrderByDescending(g => g.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var groupDtos = groups.Select(MapToDto).ToList();

            return ApiResponse<PagedResponse<StudentGroupDto>>.SuccessResponse(
                new PagedResponse<StudentGroupDto>(groupDtos, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student groups");
            return ApiResponse<PagedResponse<StudentGroupDto>>.ErrorResponse("Gruplar yuklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<StudentGroupDto>> GetByIdAsync(int id)
    {
        try
        {
            var group = await _context.StudentGroups
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

            if (group == null)
                return ApiResponse<StudentGroupDto>.ErrorResponse("Grup bulunamadi");

            return ApiResponse<StudentGroupDto>.SuccessResponse(MapToDto(group));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student group {Id}", id);
            return ApiResponse<StudentGroupDto>.ErrorResponse("Grup yuklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<StudentGroupDto>> CreateAsync(CreateStudentGroupDto dto)
    {
        try
        {
            var group = new StudentGroup
            {
                Name = dto.Name,
                Description = dto.Description,
                GroupType = dto.GroupType,
                MaxCapacity = dto.MaxCapacity,
                Color = dto.Color ?? "#6366F1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentGroups.Add(group);
            await _context.SaveChangesAsync();

            // Ogrencileri ekle
            if (dto.StudentIds?.Any() == true)
            {
                foreach (var studentId in dto.StudentIds)
                {
                    var member = new StudentGroupMember
                    {
                        GroupId = group.Id,
                        StudentId = studentId,
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.StudentGroupMembers.Add(member);
                }
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(group.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student group");
            return ApiResponse<StudentGroupDto>.ErrorResponse("Grup olusturulurken hata olustu");
        }
    }

    public async Task<ApiResponse<StudentGroupDto>> UpdateAsync(int id, UpdateStudentGroupDto dto)
    {
        try
        {
            var group = await _context.StudentGroups.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null)
                return ApiResponse<StudentGroupDto>.ErrorResponse("Grup bulunamadi");

            if (dto.Name != null) group.Name = dto.Name;
            if (dto.Description != null) group.Description = dto.Description;
            if (dto.GroupType != null) group.GroupType = dto.GroupType;
            if (dto.MaxCapacity.HasValue) group.MaxCapacity = dto.MaxCapacity.Value;
            if (dto.Color != null) group.Color = dto.Color;
            if (dto.IsActive.HasValue) group.IsActive = dto.IsActive.Value;

            group.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student group {Id}", id);
            return ApiResponse<StudentGroupDto>.ErrorResponse("Grup guncellenirken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var group = await _context.StudentGroups.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null)
                return ApiResponse<bool>.ErrorResponse("Grup bulunamadi");

            group.IsDeleted = true;
            group.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Grup silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student group {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Grup silinirken hata olustu");
        }
    }

    // ===============================================
    // UYE YONETIMI
    // ===============================================

    public async Task<ApiResponse<StudentGroupDto>> AddStudentsAsync(int groupId, AddStudentsToGroupDto dto)
    {
        try
        {
            var group = await _context.StudentGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

            if (group == null)
                return ApiResponse<StudentGroupDto>.ErrorResponse("Grup bulunamadi");

            // Kapasite kontrolu
            if (group.MaxCapacity.HasValue)
            {
                var currentCount = group.Members.Count(m => m.IsActive);
                if (currentCount + dto.StudentIds.Count > group.MaxCapacity.Value)
                    return ApiResponse<StudentGroupDto>.ErrorResponse($"Grup kapasitesi ({group.MaxCapacity}) asiliyor");
            }

            foreach (var studentId in dto.StudentIds)
            {
                var existingMember = group.Members.FirstOrDefault(m => m.StudentId == studentId);
                if (existingMember != null)
                {
                    existingMember.IsActive = true;
                    existingMember.LeftAt = null;
                    existingMember.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var member = new StudentGroupMember
                    {
                        GroupId = groupId,
                        StudentId = studentId,
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.StudentGroupMembers.Add(member);
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(groupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding students to group {GroupId}", groupId);
            return ApiResponse<StudentGroupDto>.ErrorResponse("Ogrenciler eklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<StudentGroupDto>> RemoveStudentsAsync(int groupId, RemoveStudentsFromGroupDto dto)
    {
        try
        {
            var group = await _context.StudentGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

            if (group == null)
                return ApiResponse<StudentGroupDto>.ErrorResponse("Grup bulunamadi");

            foreach (var studentId in dto.StudentIds)
            {
                var member = group.Members.FirstOrDefault(m => m.StudentId == studentId && m.IsActive);
                if (member != null)
                {
                    member.IsActive = false;
                    member.LeftAt = DateTime.UtcNow;
                    member.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(groupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing students from group {GroupId}", groupId);
            return ApiResponse<StudentGroupDto>.ErrorResponse("Ogrenciler cikarilirken hata olustu");
        }
    }

    public async Task<ApiResponse<List<StudentGroupDto>>> GetStudentGroupsAsync(int studentId)
    {
        try
        {
            var groups = await _context.StudentGroupMembers
                .Include(m => m.Group)
                    .ThenInclude(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.Student)
                            .ThenInclude(s => s.User)
                .Where(m => m.StudentId == studentId && m.IsActive && !m.Group.IsDeleted)
                .Select(m => m.Group)
                .ToListAsync();

            var groupDtos = groups.Select(MapToDto).ToList();
            return ApiResponse<List<StudentGroupDto>>.SuccessResponse(groupDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student groups for student {StudentId}", studentId);
            return ApiResponse<List<StudentGroupDto>>.ErrorResponse("Ogrenci gruplari yuklenirken hata olustu");
        }
    }

    // ===============================================
    // GRUP DERS PROGRAMI - CAKISMA KONTROLU ILE
    // ===============================================

    public async Task<ApiResponse<GroupLessonConflictCheckResult>> CheckGroupLessonConflictsAsync(CreateGroupLessonDto dto)
    {
        try
        {
            var result = new GroupLessonConflictCheckResult();

            // Grup uyelerini al
            var groupMembers = await _context.StudentGroupMembers
                .Include(m => m.Student)
                    .ThenInclude(s => s.User)
                .Where(m => m.GroupId == dto.GroupId && m.IsActive)
                .ToListAsync();

            if (!groupMembers.Any())
            {
                return ApiResponse<GroupLessonConflictCheckResult>.ErrorResponse("Grupta aktif ogrenci yok");
            }

            // 1. OGRETMEN CAKISMA KONTROLU
            var teacherConflict = await _context.LessonSchedules
                .Include(ls => ls.Student).ThenInclude(s => s.User)
                .Include(ls => ls.Course)
                .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                .Where(ls => !ls.IsDeleted &&
                            ls.Status == LessonStatus.Scheduled &&
                            ls.TeacherId == dto.TeacherId &&
                            ls.DayOfWeek == dto.DayOfWeek &&
                            ((ls.StartTime <= dto.StartTime && ls.EndTime > dto.StartTime) ||
                             (ls.StartTime < dto.EndTime && ls.EndTime >= dto.EndTime) ||
                             (ls.StartTime >= dto.StartTime && ls.EndTime <= dto.EndTime)) &&
                            ls.EffectiveFrom <= (dto.EffectiveTo ?? DateTime.MaxValue) &&
                            (ls.EffectiveTo == null || ls.EffectiveTo >= dto.EffectiveFrom))
                .FirstOrDefaultAsync();

            if (teacherConflict != null)
            {
                result.HasConflict = true;
                result.Conflicts.Add(new ConflictDetailDto
                {
                    ConflictType = "Teacher",
                    TeacherId = dto.TeacherId,
                    TeacherName = teacherConflict.Teacher?.User != null ? $"{teacherConflict.Teacher.User.FirstName} {teacherConflict.Teacher.User.LastName}" : "",
                    StudentId = teacherConflict.StudentId,
                    StudentName = $"{teacherConflict.Student.User.FirstName} {teacherConflict.Student.User.LastName}",
                    ConflictingCourseName = teacherConflict.Course?.CourseName ?? "Bilinmeyen Ders",
                    TimeRange = $"{teacherConflict.StartTime:hh\\:mm}-{teacherConflict.EndTime:hh\\:mm}",
                    DateRange = teacherConflict.EffectiveTo.HasValue
                        ? $"{teacherConflict.EffectiveFrom:dd.MM.yyyy} - {teacherConflict.EffectiveTo:dd.MM.yyyy}"
                        : $"{teacherConflict.EffectiveFrom:dd.MM.yyyy} - Suresiz",
                    Message = "Ogretmen bu saatte baska bir dersle mesgul"
                });
            }

            // Grup derslerinde ogretmen cakismasi
            var teacherGroupConflict = await _context.GroupLessonSchedules
                .Include(gls => gls.Group)
                .Include(gls => gls.Course)
                .Where(gls => !gls.IsDeleted &&
                             gls.Status == LessonStatus.Scheduled &&
                             gls.TeacherId == dto.TeacherId &&
                             gls.DayOfWeek == dto.DayOfWeek &&
                             ((gls.StartTime <= dto.StartTime && gls.EndTime > dto.StartTime) ||
                              (gls.StartTime < dto.EndTime && gls.EndTime >= dto.EndTime) ||
                              (gls.StartTime >= dto.StartTime && gls.EndTime <= dto.EndTime)) &&
                             gls.EffectiveFrom <= (dto.EffectiveTo ?? DateTime.MaxValue) &&
                             (gls.EffectiveTo == null || gls.EffectiveTo >= dto.EffectiveFrom))
                .FirstOrDefaultAsync();

            if (teacherGroupConflict != null)
            {
                result.HasConflict = true;
                result.Conflicts.Add(new ConflictDetailDto
                {
                    ConflictType = "Teacher",
                    TeacherId = dto.TeacherId,
                    ConflictingCourseName = teacherGroupConflict.Course?.CourseName ?? "Bilinmeyen Ders",
                    TimeRange = $"{teacherGroupConflict.StartTime:hh\\:mm}-{teacherGroupConflict.EndTime:hh\\:mm}",
                    DateRange = teacherGroupConflict.EffectiveTo.HasValue
                        ? $"{teacherGroupConflict.EffectiveFrom:dd.MM.yyyy} - {teacherGroupConflict.EffectiveTo:dd.MM.yyyy}"
                        : $"{teacherGroupConflict.EffectiveFrom:dd.MM.yyyy} - Suresiz",
                    Message = $"Ogretmen bu saatte '{teacherGroupConflict.Group.Name}' grubuyla dersle mesgul"
                });
            }

            // 2. HER OGRENCI ICIN CAKISMA KONTROLU
            foreach (var member in groupMembers)
            {
                // Bireysel ders cakismasi
                var studentConflict = await _context.LessonSchedules
                    .Include(ls => ls.Teacher).ThenInclude(t => t.User)
                    .Include(ls => ls.Course)
                    .Where(ls => !ls.IsDeleted &&
                                ls.Status == LessonStatus.Scheduled &&
                                ls.StudentId == member.StudentId &&
                                ls.DayOfWeek == dto.DayOfWeek &&
                                ((ls.StartTime <= dto.StartTime && ls.EndTime > dto.StartTime) ||
                                 (ls.StartTime < dto.EndTime && ls.EndTime >= dto.EndTime) ||
                                 (ls.StartTime >= dto.StartTime && ls.EndTime <= dto.EndTime)) &&
                                ls.EffectiveFrom <= (dto.EffectiveTo ?? DateTime.MaxValue) &&
                                (ls.EffectiveTo == null || ls.EffectiveTo >= dto.EffectiveFrom))
                    .FirstOrDefaultAsync();

                if (studentConflict != null)
                {
                    result.HasConflict = true;
                    result.Conflicts.Add(new ConflictDetailDto
                    {
                        ConflictType = "Student",
                        StudentId = member.StudentId,
                        StudentName = $"{member.Student.User.FirstName} {member.Student.User.LastName}",
                        TeacherId = studentConflict.TeacherId,
                        TeacherName = $"{studentConflict.Teacher.User.FirstName} {studentConflict.Teacher.User.LastName}",
                        ConflictingCourseName = studentConflict.Course?.CourseName ?? "Bilinmeyen Ders",
                        TimeRange = $"{studentConflict.StartTime:hh\\:mm}-{studentConflict.EndTime:hh\\:mm}",
                        DateRange = studentConflict.EffectiveTo.HasValue
                            ? $"{studentConflict.EffectiveFrom:dd.MM.yyyy} - {studentConflict.EffectiveTo:dd.MM.yyyy}"
                            : $"{studentConflict.EffectiveFrom:dd.MM.yyyy} - Suresiz",
                        Message = $"{member.Student.User.FirstName} {member.Student.User.LastName} bu saatte bireysel dersi var"
                    });
                }

                // Diger grup derslerinde cakisma
                var studentGroupConflict = await _context.GroupLessonSchedules
                    .Include(gls => gls.Group)
                        .ThenInclude(g => g.Members)
                    .Include(gls => gls.Course)
                    .Where(gls => !gls.IsDeleted &&
                                 gls.Status == LessonStatus.Scheduled &&
                                 gls.GroupId != dto.GroupId &&
                                 gls.Group.Members.Any(m => m.StudentId == member.StudentId && m.IsActive) &&
                                 gls.DayOfWeek == dto.DayOfWeek &&
                                 ((gls.StartTime <= dto.StartTime && gls.EndTime > dto.StartTime) ||
                                  (gls.StartTime < dto.EndTime && gls.EndTime >= dto.EndTime) ||
                                  (gls.StartTime >= dto.StartTime && gls.EndTime <= dto.EndTime)) &&
                                 gls.EffectiveFrom <= (dto.EffectiveTo ?? DateTime.MaxValue) &&
                                 (gls.EffectiveTo == null || gls.EffectiveTo >= dto.EffectiveFrom))
                    .FirstOrDefaultAsync();

                if (studentGroupConflict != null)
                {
                    result.HasConflict = true;
                    result.Conflicts.Add(new ConflictDetailDto
                    {
                        ConflictType = "Student",
                        StudentId = member.StudentId,
                        StudentName = $"{member.Student.User.FirstName} {member.Student.User.LastName}",
                        ConflictingCourseName = studentGroupConflict.Course?.CourseName ?? "Bilinmeyen Ders",
                        TimeRange = $"{studentGroupConflict.StartTime:hh\\:mm}-{studentGroupConflict.EndTime:hh\\:mm}",
                        DateRange = studentGroupConflict.EffectiveTo.HasValue
                            ? $"{studentGroupConflict.EffectiveFrom:dd.MM.yyyy} - {studentGroupConflict.EffectiveTo:dd.MM.yyyy}"
                            : $"{studentGroupConflict.EffectiveFrom:dd.MM.yyyy} - Suresiz",
                        Message = $"{member.Student.User.FirstName} {member.Student.User.LastName} bu saatte '{studentGroupConflict.Group.Name}' grubunda dersi var"
                    });
                }
            }

            return ApiResponse<GroupLessonConflictCheckResult>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking group lesson conflicts");
            return ApiResponse<GroupLessonConflictCheckResult>.ErrorResponse("Cakisma kontrolu yapilirken hata olustu");
        }
    }

    public async Task<ApiResponse<GroupLessonScheduleDto>> CreateGroupLessonAsync(CreateGroupLessonDto dto)
    {
        try
        {
            // Once cakisma kontrolu yap
            var conflictCheck = await CheckGroupLessonConflictsAsync(dto);
            if (!conflictCheck.Success)
                return ApiResponse<GroupLessonScheduleDto>.ErrorResponse(conflictCheck.Message);

            if (conflictCheck.Data?.HasConflict == true)
            {
                var conflictMessages = string.Join("\n", conflictCheck.Data.Conflicts.Select(c => c.Message));
                return ApiResponse<GroupLessonScheduleDto>.ErrorResponse($"Cakisma tespit edildi:\n{conflictMessages}");
            }

            var lesson = new GroupLessonSchedule
            {
                GroupId = dto.GroupId,
                TeacherId = dto.TeacherId,
                CourseId = dto.CourseId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                ClassroomId = dto.ClassroomId,
                Notes = dto.Notes,
                IsRecurring = dto.IsRecurring,
                Status = LessonStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            _context.GroupLessonSchedules.Add(lesson);
            await _context.SaveChangesAsync();

            // Load related data
            await _context.Entry(lesson).Reference(l => l.Group).LoadAsync();
            await _context.Entry(lesson).Reference(l => l.Teacher).LoadAsync();
            if (lesson.Teacher != null)
                await _context.Entry(lesson.Teacher).Reference(t => t.User).LoadAsync();
            await _context.Entry(lesson).Reference(l => l.Course).LoadAsync();

            return ApiResponse<GroupLessonScheduleDto>.SuccessResponse(
                MapToLessonDto(lesson),
                "Grup dersi olusturuldu"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group lesson");
            return ApiResponse<GroupLessonScheduleDto>.ErrorResponse("Grup dersi olusturulurken hata olustu");
        }
    }

    public async Task<ApiResponse<bool>> CancelGroupLessonAsync(int lessonId, bool cancelAll = true, DateTime? cancelDate = null)
    {
        try
        {
            var lesson = await _context.GroupLessonSchedules
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

            if (lesson == null)
                return ApiResponse<bool>.ErrorResponse("Ders bulunamadi");

            if (cancelAll)
            {
                lesson.Status = LessonStatus.Cancelled;
            }
            else if (cancelDate.HasValue)
            {
                var cancelledDates = string.IsNullOrEmpty(lesson.CancelledDates)
                    ? new List<string>()
                    : lesson.CancelledDates.Split(',').ToList();
                cancelledDates.Add(cancelDate.Value.ToString("yyyy-MM-dd"));
                lesson.CancelledDates = string.Join(",", cancelledDates);
            }

            lesson.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Ders iptal edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling group lesson {LessonId}", lessonId);
            return ApiResponse<bool>.ErrorResponse("Ders iptal edilirken hata olustu");
        }
    }

    public async Task<ApiResponse<List<GroupLessonScheduleDto>>> GetGroupLessonsAsync(int groupId)
    {
        try
        {
            var lessons = await _context.GroupLessonSchedules
                .Include(l => l.Group)
                    .ThenInclude(g => g.Members)
                .Include(l => l.Teacher)
                    .ThenInclude(t => t.User)
                .Include(l => l.Course)
                .Include(l => l.Classroom)
                .Where(l => l.GroupId == groupId && !l.IsDeleted)
                .OrderBy(l => l.DayOfWeek)
                .ThenBy(l => l.StartTime)
                .ToListAsync();

            var dtos = lessons.Select(MapToLessonDto).ToList();
            return ApiResponse<List<GroupLessonScheduleDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group lessons for group {GroupId}", groupId);
            return ApiResponse<List<GroupLessonScheduleDto>>.ErrorResponse("Grup dersleri yuklenirken hata olustu");
        }
    }

    public async Task<ApiResponse<List<GroupLessonScheduleDto>>> GetTeacherGroupLessonsAsync(int teacherId)
    {
        try
        {
            var lessons = await _context.GroupLessonSchedules
                .Include(l => l.Group)
                    .ThenInclude(g => g.Members)
                .Include(l => l.Teacher)
                    .ThenInclude(t => t.User)
                .Include(l => l.Course)
                .Include(l => l.Classroom)
                .Where(l => l.TeacherId == teacherId && !l.IsDeleted && l.Status == LessonStatus.Scheduled)
                .OrderBy(l => l.DayOfWeek)
                .ThenBy(l => l.StartTime)
                .ToListAsync();

            var dtos = lessons.Select(MapToLessonDto).ToList();
            return ApiResponse<List<GroupLessonScheduleDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher group lessons for teacher {TeacherId}", teacherId);
            return ApiResponse<List<GroupLessonScheduleDto>>.ErrorResponse("Ogretmen grup dersleri yuklenirken hata olustu");
        }
    }

    // ===============================================
    // OTOMATIK DEAKTIVASYON
    // ===============================================

    public async Task<ApiResponse<int>> DeactivateExpiredGroupsAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var deactivatedCount = 0;

            // Son ders tarihi gecmis aktif gruplari bul
            var expiredGroups = await _context.StudentGroups
                .Include(g => g.LessonSchedules)
                .Where(g => !g.IsDeleted && g.IsActive)
                .ToListAsync();

            foreach (var group in expiredGroups)
            {
                // Grubun aktif dersleri var mi kontrol et
                var hasActiveLessons = group.LessonSchedules.Any(ls =>
                    !ls.IsDeleted &&
                    ls.Status == LessonStatus.Scheduled &&
                    (ls.EffectiveTo == null || ls.EffectiveTo >= today));

                if (!hasActiveLessons && group.LessonSchedules.Any())
                {
                    // Tum dersleri bitmis - grubu pasife al
                    var lastLessonDate = group.LessonSchedules
                        .Where(ls => !ls.IsDeleted && ls.EffectiveTo.HasValue)
                        .Max(ls => ls.EffectiveTo);

                    if (lastLessonDate.HasValue && lastLessonDate.Value < today)
                    {
                        group.IsActive = false;
                        group.UpdatedAt = DateTime.UtcNow;
                        deactivatedCount++;
                        _logger.LogInformation("Grup pasife alindi: {GroupId} - {GroupName}, Son ders tarihi: {LastDate}",
                            group.Id, group.Name, lastLessonDate.Value);
                    }
                }
            }

            if (deactivatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Otomatik grup deaktivasyonu tamamlandi. {Count} grup pasife alindi", deactivatedCount);
            return ApiResponse<int>.SuccessResponse(deactivatedCount, $"{deactivatedCount} grup pasife alindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating expired groups");
            return ApiResponse<int>.ErrorResponse("Gruplari pasife alirken hata olustu");
        }
    }

    // ===============================================
    // HELPER METHODS
    // ===============================================

    private static StudentGroupDto MapToDto(StudentGroup g)
    {
        return new StudentGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            GroupType = g.GroupType,
            MaxCapacity = g.MaxCapacity,
            Color = g.Color,
            IsActive = g.IsActive,
            MemberCount = g.Members.Count(m => m.IsActive),
            Members = g.Members.Where(m => m.IsActive).Select(m => new StudentGroupMemberDto
            {
                Id = m.Id,
                StudentId = m.StudentId,
                StudentNo = m.Student.StudentNo,
                StudentName = $"{m.Student.User.FirstName} {m.Student.User.LastName}",
                Grade = m.Student.CurrentGrade,
                JoinedAt = m.JoinedAt,
                IsActive = m.IsActive
            }).ToList(),
            CreatedAt = g.CreatedAt
        };
    }

    private static GroupLessonScheduleDto MapToLessonDto(GroupLessonSchedule l)
    {
        return new GroupLessonScheduleDto
        {
            Id = l.Id,
            GroupId = l.GroupId,
            GroupName = l.Group?.Name ?? string.Empty,
            MemberCount = l.Group?.Members?.Count(m => m.IsActive) ?? 0,
            TeacherId = l.TeacherId,
            TeacherName = l.Teacher?.User != null ? $"{l.Teacher.User.FirstName} {l.Teacher.User.LastName}" : string.Empty,
            CourseId = l.CourseId,
            CourseName = l.Course?.CourseName ?? string.Empty,
            DayOfWeek = l.DayOfWeek,
            StartTime = l.StartTime,
            EndTime = l.EndTime,
            EffectiveFrom = l.EffectiveFrom,
            EffectiveTo = l.EffectiveTo,
            Status = l.Status.ToString(),
            ClassroomId = l.ClassroomId,
            ClassroomName = l.Classroom?.RoomNumber,
            IsRecurring = l.IsRecurring,
            CancelledDates = string.IsNullOrEmpty(l.CancelledDates)
                ? new List<string>()
                : l.CancelledDates.Split(',').ToList(),
            Notes = l.Notes,
            Color = l.Group?.Color
        };
    }
}
