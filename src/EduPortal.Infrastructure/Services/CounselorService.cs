using EduPortal.Application.DTOs.Counselor;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CounselorService : ICounselorService
{
    private readonly ApplicationDbContext _context;

    public CounselorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CounselorDto>> GetAllCounselorsAsync()
    {
        var counselors = await _context.Counselors
            .Include(c => c.User)
            .Include(c => c.Students)
            .Include(c => c.CounselingMeetings)
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        return counselors.Select(MapToDto);
    }

    public async Task<(IEnumerable<CounselorSummaryDto> Items, int TotalCount)> GetCounselorsPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Counselors
            .Include(c => c.User)
            .Include(c => c.Students)
            .Where(c => !c.IsDeleted);

        var totalCount = await query.CountAsync();

        var counselors = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = counselors.Select(c => new CounselorSummaryDto
        {
            Id = c.Id,
            FullName = $"{c.User.FirstName} {c.User.LastName}",
            Email = c.User.Email,
            Specialization = c.Specialization,
            IsActive = c.IsActive,
            ActiveStudentCount = c.Students.Count(s => s.IsActive && !s.IsDeleted)
        });

        return (items, totalCount);
    }

    public async Task<CounselorDto?> GetCounselorByIdAsync(int id)
    {
        var counselor = await _context.Counselors
            .Include(c => c.User)
            .Include(c => c.Students)
            .Include(c => c.CounselingMeetings)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return counselor != null ? MapToDto(counselor) : null;
    }

    public async Task<CounselorDto?> GetCounselorByUserIdAsync(string userId)
    {
        var counselor = await _context.Counselors
            .Include(c => c.User)
            .Include(c => c.Students)
            .Include(c => c.CounselingMeetings)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        return counselor != null ? MapToDto(counselor) : null;
    }

    public async Task<CounselorDto> CreateCounselorAsync(CreateCounselorDto dto)
    {
        var counselor = new Counselor
        {
            UserId = dto.UserId,
            Specialization = dto.Specialization,
            IsActive = dto.IsActive
        };

        _context.Counselors.Add(counselor);
        await _context.SaveChangesAsync();

        return (await GetCounselorByIdAsync(counselor.Id))!;
    }

    public async Task<CounselorDto> UpdateCounselorAsync(int id, UpdateCounselorDto dto)
    {
        var counselor = await _context.Counselors.FindAsync(id);
        if (counselor == null || counselor.IsDeleted)
            throw new Exception("Counselor not found");

        counselor.Specialization = dto.Specialization;
        counselor.IsActive = dto.IsActive;
        counselor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetCounselorByIdAsync(id))!;
    }

    public async Task<bool> DeleteCounselorAsync(int id)
    {
        var counselor = await _context.Counselors.FindAsync(id);
        if (counselor == null || counselor.IsDeleted)
            return false;

        counselor.IsDeleted = true;
        counselor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CounselorStudentDto>> GetCounselorStudentsAsync(int counselorId)
    {
        var assignments = await _context.StudentCounselorAssignments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.CounselorId == counselorId && !a.IsDeleted)
            .ToListAsync();

        var result = new List<CounselorStudentDto>();

        foreach (var assignment in assignments)
        {
            var meetingCount = await _context.CounselingMeetings
                .CountAsync(m => m.CounselorId == counselorId && m.StudentId == assignment.StudentId && !m.IsDeleted);

            var lastMeeting = await _context.CounselingMeetings
                .Where(m => m.CounselorId == counselorId && m.StudentId == assignment.StudentId && !m.IsDeleted)
                .OrderByDescending(m => m.MeetingDate)
                .FirstOrDefaultAsync();

            result.Add(new CounselorStudentDto
            {
                StudentId = assignment.StudentId,
                StudentName = $"{assignment.Student.User.FirstName} {assignment.Student.User.LastName}",
                Email = assignment.Student.User.Email,
                AssignmentStartDate = assignment.StartDate,
                AssignmentEndDate = assignment.EndDate,
                IsActive = assignment.IsActive,
                MeetingCount = meetingCount,
                LastMeetingDate = lastMeeting?.MeetingDate
            });
        }

        return result;
    }

    public async Task<bool> AssignStudentAsync(int counselorId, int studentId)
    {
        var counselor = await _context.Counselors.FindAsync(counselorId);
        if (counselor == null || counselor.IsDeleted)
            return false;

        var existingAssignment = await _context.StudentCounselorAssignments
            .FirstOrDefaultAsync(a => a.CounselorId == counselorId && a.StudentId == studentId && a.IsActive && !a.IsDeleted);

        if (existingAssignment != null)
            return false; // Already assigned

        var assignment = new StudentCounselorAssignment
        {
            CounselorId = counselorId,
            StudentId = studentId,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.StudentCounselorAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnassignStudentAsync(int counselorId, int studentId)
    {
        var assignment = await _context.StudentCounselorAssignments
            .FirstOrDefaultAsync(a => a.CounselorId == counselorId && a.StudentId == studentId && a.IsActive && !a.IsDeleted);

        if (assignment == null)
            return false;

        assignment.IsActive = false;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(IEnumerable<CounselingSessionDto> Items, int TotalCount)> GetCounselorSessionsPagedAsync(int counselorId, int pageNumber, int pageSize)
    {
        var query = _context.CounselingMeetings
            .Include(m => m.Student)
                .ThenInclude(s => s.User)
            .Include(m => m.Counselor)
                .ThenInclude(c => c.User)
            .Where(m => m.CounselorId == counselorId && !m.IsDeleted);

        var totalCount = await query.CountAsync();

        var meetings = await query
            .OrderByDescending(m => m.MeetingDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = meetings.Select(m => new CounselingSessionDto
        {
            Id = m.Id,
            StudentId = m.StudentId,
            StudentName = $"{m.Student.User.FirstName} {m.Student.User.LastName}",
            CounselorId = m.CounselorId,
            CounselorName = $"{m.Counselor.User.FirstName} {m.Counselor.User.LastName}",
            MeetingDate = m.MeetingDate,
            Duration = m.Duration,
            Notes = m.Notes,
            Assignments = m.Assignments,
            NextMeetingDate = m.NextMeetingDate,
            SendEmailToParent = m.SendEmailToParent,
            SendSMSToParent = m.SendSMSToParent,
            CreatedAt = m.CreatedAt
        });

        return (items, totalCount);
    }

    public async Task<IEnumerable<CounselorDto>> GetActiveCounselorsAsync()
    {
        var counselors = await _context.Counselors
            .Include(c => c.User)
            .Include(c => c.Students)
            .Include(c => c.CounselingMeetings)
            .Where(c => !c.IsDeleted && c.IsActive)
            .ToListAsync();

        return counselors.Select(MapToDto);
    }

    private CounselorDto MapToDto(Counselor counselor)
    {
        return new CounselorDto
        {
            Id = counselor.Id,
            UserId = counselor.UserId,
            UserName = counselor.User.UserName ?? "",
            FullName = $"{counselor.User.FirstName} {counselor.User.LastName}",
            Email = counselor.User.Email,
            PhoneNumber = counselor.User.PhoneNumber,
            Specialization = counselor.Specialization,
            IsActive = counselor.IsActive,
            ActiveStudentCount = counselor.Students.Count(s => s.IsActive && !s.IsDeleted),
            TotalMeetingsCount = counselor.CounselingMeetings.Count(m => !m.IsDeleted),
            CreatedAt = counselor.CreatedAt
        };
    }
}
