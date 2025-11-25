using EduPortal.Application.DTOs.Club;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ClubService : IClubService
{
    private readonly ApplicationDbContext _context;

    public ClubService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClubDto>> GetAllAsync()
    {
        return await _context.Clubs
            .Include(c => c.Advisor)
                .ThenInclude(t => t!.User)
            .AsNoTracking()
            .OrderBy(c => c.ClubName)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<ClubDto?> GetByIdAsync(int id)
    {
        var club = await _context.Clubs
            .Include(c => c.Advisor)
                .ThenInclude(t => t!.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return club == null ? null : MapToDto(club);
    }

    public async Task<ClubDto> CreateAsync(CreateClubDto dto)
    {
        var club = new Club
        {
            ClubName = dto.ClubName,
            Description = dto.Description,
            AdvisorTeacherId = dto.AdvisorTeacherId,
            MaxMembers = dto.MaxMembers,
            CurrentMembers = 0,
            MeetingDay = dto.MeetingDay,
            MeetingTime = dto.MeetingTime,
            MeetingRoom = dto.MeetingRoom,
            IsActive = dto.IsActive,
            AcceptingMembers = dto.AcceptingMembers,
            AcademicYear = dto.AcademicYear,
            Requirements = dto.Requirements,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clubs.Add(club);
        await _context.SaveChangesAsync();

        var created = await _context.Clubs
            .Include(c => c.Advisor)
                .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(c => c.Id == club.Id);

        return MapToDto(created!);
    }

    public async Task<ClubDto> UpdateAsync(int id, UpdateClubDto dto)
    {
        var club = await _context.Clubs
            .Include(c => c.Advisor)
                .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (club == null)
            throw new KeyNotFoundException("Kulüp bulunamadı");

        club.ClubName = dto.ClubName;
        club.Description = dto.Description;
        club.AdvisorTeacherId = dto.AdvisorTeacherId;
        club.MaxMembers = dto.MaxMembers;
        club.MeetingDay = dto.MeetingDay;
        club.MeetingTime = dto.MeetingTime;
        club.MeetingRoom = dto.MeetingRoom;
        club.IsActive = dto.IsActive;
        club.AcceptingMembers = dto.AcceptingMembers;
        club.AcademicYear = dto.AcademicYear;
        club.Requirements = dto.Requirements;
        club.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(club);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var club = await _context.Clubs.FindAsync(id);
        if (club == null)
            return false;

        _context.Clubs.Remove(club);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(IEnumerable<ClubMemberDto> Items, int TotalCount)> GetMembersAsync(int clubId, int pageNumber, int pageSize)
    {
        var club = await _context.Clubs.FindAsync(clubId);
        if (club == null)
            throw new KeyNotFoundException("Kulüp bulunamadı");

        var query = _context.StudentClubMemberships
            .Include(m => m.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .Where(m => m.ClubName == club.ClubName);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.Student.User.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ClubMemberDto
            {
                StudentId = m.StudentId,
                StudentName = m.Student.User != null ? $"{m.Student.User.FirstName} {m.Student.User.LastName}" : string.Empty,
                StudentNo = m.Student.StudentNo,
                ClubType = m.ClubType,
                Role = m.Role,
                StartDate = m.StartDate,
                EndDate = m.EndDate
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<ClubDto>> GetByStudentAsync(int studentId)
    {
        var memberships = await _context.StudentClubMemberships
            .AsNoTracking()
            .Where(m => m.StudentId == studentId && m.ClubType == "OkulIci")
            .Select(m => m.ClubName)
            .ToListAsync();

        return await _context.Clubs
            .Include(c => c.Advisor)
                .ThenInclude(t => t!.User)
            .AsNoTracking()
            .Where(c => memberships.Contains(c.ClubName))
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<ClubMemberDto> JoinClubAsync(int clubId, int studentId)
    {
        var club = await _context.Clubs.FindAsync(clubId);
        if (club == null)
            throw new KeyNotFoundException("Kulüp bulunamadı");

        if (!club.AcceptingMembers)
            throw new InvalidOperationException("Bu kulüp şu anda yeni üye kabul etmiyor");

        if (club.CurrentMembers >= club.MaxMembers)
            throw new InvalidOperationException("Kulüp kapasitesi dolu");

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
            throw new KeyNotFoundException("Öğrenci bulunamadı");

        // Check if already a member
        var existingMembership = await _context.StudentClubMemberships
            .FirstOrDefaultAsync(m => m.StudentId == studentId && m.ClubName == club.ClubName);

        if (existingMembership != null)
            throw new InvalidOperationException("Öğrenci zaten bu kulübün üyesi");

        var membership = new StudentClubMembership
        {
            StudentId = studentId,
            ClubType = "OkulIci",
            ClubName = club.ClubName,
            Role = "Üye",
            StartDate = DateTime.UtcNow
        };

        _context.StudentClubMemberships.Add(membership);
        club.CurrentMembers++;
        await _context.SaveChangesAsync();

        return new ClubMemberDto
        {
            StudentId = studentId,
            StudentName = student.User != null ? $"{student.User.FirstName} {student.User.LastName}" : string.Empty,
            StudentNo = student.StudentNo,
            ClubType = membership.ClubType,
            Role = membership.Role,
            StartDate = membership.StartDate
        };
    }

    public async Task<bool> LeaveClubAsync(int clubId, int studentId)
    {
        var club = await _context.Clubs.FindAsync(clubId);
        if (club == null)
            throw new KeyNotFoundException("Kulüp bulunamadı");

        var membership = await _context.StudentClubMemberships
            .FirstOrDefaultAsync(m => m.StudentId == studentId && m.ClubName == club.ClubName);

        if (membership == null)
            return false;

        _context.StudentClubMemberships.Remove(membership);
        if (club.CurrentMembers > 0)
            club.CurrentMembers--;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ClubDto MapToDto(Club c)
    {
        return new ClubDto
        {
            Id = c.Id,
            ClubName = c.ClubName,
            Description = c.Description,
            AdvisorTeacherId = c.AdvisorTeacherId,
            AdvisorName = c.Advisor?.User != null ? $"{c.Advisor.User.FirstName} {c.Advisor.User.LastName}" : null,
            MaxMembers = c.MaxMembers,
            CurrentMembers = c.CurrentMembers,
            MeetingDay = c.MeetingDay,
            MeetingTime = c.MeetingTime,
            MeetingRoom = c.MeetingRoom,
            IsActive = c.IsActive,
            AcceptingMembers = c.AcceptingMembers,
            AcademicYear = c.AcademicYear,
            Requirements = c.Requirements,
            CreatedAt = c.CreatedAt
        };
    }
}
