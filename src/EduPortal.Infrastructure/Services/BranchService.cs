using EduPortal.Application.DTOs.Branch;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _context;

    public BranchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranchesAsync()
    {
        var branches = await _context.Branches
            .Include(b => b.Manager)
            .Where(b => !b.IsDeleted)
            .ToListAsync();

        return branches.Select(MapToDto);
    }

    public async Task<BranchDto?> GetBranchByIdAsync(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.Manager)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        return branch != null ? MapToDto(branch) : null;
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchDto dto)
    {
        var branch = new Branch
        {
            BranchName = dto.BranchName,
            BranchCode = dto.BranchCode,
            Type = (BranchType)dto.Type,
            Address = dto.Address,
            City = dto.City,
            District = dto.District,
            Phone = dto.Phone,
            Email = dto.Email,
            ManagerId = dto.ManagerId,
            Capacity = dto.Capacity,
            OpeningDate = dto.OpeningDate,
            IsActive = true,
            Notes = dto.Notes
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return (await GetBranchByIdAsync(branch.Id))!;
    }

    public async Task<BranchDto> UpdateBranchAsync(int id, UpdateBranchDto dto)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null || branch.IsDeleted)
            throw new Exception("Branch not found");

        branch.BranchName = dto.BranchName;
        branch.Type = (BranchType)dto.Type;
        branch.Address = dto.Address;
        branch.City = dto.City;
        branch.District = dto.District;
        branch.Phone = dto.Phone;
        branch.Email = dto.Email;
        branch.ManagerId = dto.ManagerId;
        branch.Capacity = dto.Capacity;
        branch.IsActive = dto.IsActive;
        branch.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return (await GetBranchByIdAsync(id))!;
    }

    public async Task<bool> DeleteBranchAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null || branch.IsDeleted)
            return false;

        branch.IsDeleted = true;
        branch.DeletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<BranchStatisticsDto> GetBranchStatisticsAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null || branch.IsDeleted)
            throw new Exception("Branch not found");

        var stats = new BranchStatisticsDto
        {
            BranchId = id,
            BranchName = branch.BranchName,
            Capacity = branch.Capacity
        };

        stats.TotalStudents = await _context.Students
            .CountAsync(s => s.BranchId == id && !s.IsDeleted);

        stats.ActiveStudents = await _context.Students
            .CountAsync(s => s.BranchId == id && !s.IsDeleted);

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        stats.NewStudentsThisMonth = await _context.Students
            .CountAsync(s => s.BranchId == id && !s.IsDeleted && s.CreatedDate >= firstDayOfMonth);

        stats.TotalTeachers = await _context.Teachers
            .CountAsync(t => t.BranchId == id && !t.IsDeleted);

        stats.TotalCoaches = await _context.Coaches
            .CountAsync(c => c.BranchId == id && !c.IsDeleted);

        stats.TotalClasses = await _context.Classes
            .CountAsync(c => c.BranchId == id && !c.IsDeleted);

        stats.TotalClassrooms = await _context.Classrooms
            .CountAsync(c => c.BranchId == id && !c.IsDeleted);

        stats.ActiveCoachingPrograms = await _context.StudentCoachAssignments
            .CountAsync(sca => sca.Coach.BranchId == id && sca.IsActive && !sca.IsDeleted);

        stats.CompletedSessionsThisMonth = await _context.CoachingSessions
            .CountAsync(cs => cs.BranchId == id &&
                             cs.Status == SessionStatus.Completed &&
                             cs.SessionDate >= firstDayOfMonth &&
                             !cs.IsDeleted);

        stats.CapacityUtilization = branch.Capacity > 0
            ? (decimal)stats.TotalStudents / branch.Capacity * 100
            : 0;

        return stats;
    }

    public async Task<IEnumerable<BranchStatisticsDto>> GetAllBranchesPerformanceAsync()
    {
        var branches = await _context.Branches
            .Where(b => !b.IsDeleted && b.IsActive)
            .Select(b => b.Id)
            .ToListAsync();

        var stats = new List<BranchStatisticsDto>();
        foreach (var branchId in branches)
        {
            stats.Add(await GetBranchStatisticsAsync(branchId));
        }

        return stats;
    }

    public async Task<bool> TransferStudentAsync(TransferStudentDto dto)
    {
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null || student.IsDeleted)
            return false;

        var transfer = new StudentBranchTransfer
        {
            StudentId = dto.StudentId,
            FromBranchId = dto.FromBranchId,
            ToBranchId = dto.ToBranchId,
            TransferDate = dto.TransferDate,
            Reason = (TransferReason)dto.Reason,
            Notes = dto.Notes,
            ApprovedBy = "system",
            Status = TransferStatus.Completed
        };

        _context.StudentBranchTransfers.Add(transfer);
        student.BranchId = dto.ToBranchId;
        await _context.SaveChangesAsync();

        return true;
    }

    private BranchDto MapToDto(Branch branch)
    {
        var studentCount = _context.Students.Count(s => s.BranchId == branch.Id && !s.IsDeleted);
        var teacherCount = _context.Teachers.Count(t => t.BranchId == branch.Id && !t.IsDeleted);
        var coachCount = _context.Coaches.Count(c => c.BranchId == branch.Id && !c.IsDeleted);

        return new BranchDto
        {
            Id = branch.Id,
            BranchName = branch.BranchName,
            BranchCode = branch.BranchCode,
            Type = branch.Type.ToString(),
            Address = branch.Address,
            City = branch.City,
            District = branch.District,
            Phone = branch.Phone,
            Email = branch.Email,
            ManagerId = branch.ManagerId,
            ManagerName = branch.Manager != null ? $"{branch.Manager.FirstName} {branch.Manager.LastName}" : null,
            Capacity = branch.Capacity,
            OpeningDate = branch.OpeningDate,
            IsActive = branch.IsActive,
            Notes = branch.Notes,
            CurrentStudentCount = studentCount,
            CurrentTeacherCount = teacherCount,
            CurrentCoachCount = coachCount,
            CapacityUtilization = branch.Capacity > 0 ? (decimal)studentCount / branch.Capacity * 100 : 0,
            CreatedDate = branch.CreatedDate,
            LastModifiedDate = branch.LastModifiedDate
        };
    }
}
