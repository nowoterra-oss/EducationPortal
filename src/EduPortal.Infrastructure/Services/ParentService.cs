using EduPortal.Application.DTOs.Parent;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ParentService : IParentService
{
    private readonly ApplicationDbContext _context;

    public ParentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ParentDto>> GetAllParentsAsync()
    {
        var parents = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return parents.Select(MapToDto);
    }

    public async Task<(IEnumerable<ParentSummaryDto> Items, int TotalCount)> GetParentsPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
            .Where(p => !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var parents = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = parents.Select(p => new ParentSummaryDto
        {
            Id = p.Id,
            FullName = $"{p.User.FirstName} {p.User.LastName}",
            Email = p.User.Email,
            PhoneNumber = p.User.PhoneNumber,
            Occupation = p.Occupation,
            StudentCount = p.Students.Count(sp => !sp.IsDeleted)
        });

        return (items, totalCount);
    }

    public async Task<ParentDto?> GetParentByIdAsync(int id)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return parent != null ? MapToDto(parent) : null;
    }

    public async Task<ParentDto?> GetParentByUserIdAsync(string userId)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        return parent != null ? MapToDto(parent) : null;
    }

    public async Task<ParentDto> CreateParentAsync(CreateParentDto dto)
    {
        var parent = new Parent
        {
            UserId = dto.UserId,
            Occupation = dto.Occupation,
            WorkPhone = dto.WorkPhone
        };

        _context.Parents.Add(parent);
        await _context.SaveChangesAsync();

        // Eğer öğrenci ilişkileri belirtildiyse ekle
        if (dto.StudentRelationships != null && dto.StudentRelationships.Any())
        {
            foreach (var relationship in dto.StudentRelationships)
            {
                var studentParent = new StudentParent
                {
                    ParentId = parent.Id,
                    StudentId = relationship.StudentId,
                    Relationship = relationship.Relationship,
                    IsPrimaryContact = relationship.IsPrimaryContact,
                    IsEmergencyContact = relationship.IsEmergencyContact
                };
                _context.StudentParents.Add(studentParent);
            }
            await _context.SaveChangesAsync();
        }

        return (await GetParentByIdAsync(parent.Id))!;
    }

    public async Task<ParentDto> UpdateParentAsync(int id, UpdateParentDto dto)
    {
        var parent = await _context.Parents.FindAsync(id);
        if (parent == null || parent.IsDeleted)
            throw new Exception("Parent not found");

        parent.Occupation = dto.Occupation;
        parent.WorkPhone = dto.WorkPhone;
        parent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetParentByIdAsync(id))!;
    }

    public async Task<bool> DeleteParentAsync(int id)
    {
        var parent = await _context.Parents.FindAsync(id);
        if (parent == null || parent.IsDeleted)
            return false;

        parent.IsDeleted = true;
        parent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ParentDto>> GetParentsByStudentIdAsync(int studentId)
    {
        var parentIds = await _context.StudentParents
            .Where(sp => sp.StudentId == studentId && !sp.IsDeleted)
            .Select(sp => sp.ParentId)
            .ToListAsync();

        var parents = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => parentIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        return parents.Select(MapToDto);
    }

    public async Task<bool> AddStudentRelationshipAsync(int parentId, StudentRelationshipDto relationship)
    {
        var parent = await _context.Parents.FindAsync(parentId);
        if (parent == null || parent.IsDeleted)
            return false;

        var existingRelationship = await _context.StudentParents
            .FirstOrDefaultAsync(sp => sp.ParentId == parentId && sp.StudentId == relationship.StudentId && !sp.IsDeleted);

        if (existingRelationship != null)
            return false; // İlişki zaten var

        var studentParent = new StudentParent
        {
            ParentId = parentId,
            StudentId = relationship.StudentId,
            Relationship = relationship.Relationship,
            IsPrimaryContact = relationship.IsPrimaryContact,
            IsEmergencyContact = relationship.IsEmergencyContact
        };

        _context.StudentParents.Add(studentParent);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveStudentRelationshipAsync(int parentId, int studentId)
    {
        var relationship = await _context.StudentParents
            .FirstOrDefaultAsync(sp => sp.ParentId == parentId && sp.StudentId == studentId && !sp.IsDeleted);

        if (relationship == null)
            return false;

        relationship.IsDeleted = true;
        relationship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private ParentDto MapToDto(Parent parent)
    {
        return new ParentDto
        {
            Id = parent.Id,
            UserId = parent.UserId,
            UserName = parent.User.UserName ?? "",
            FullName = $"{parent.User.FirstName} {parent.User.LastName}",
            Email = parent.User.Email,
            PhoneNumber = parent.User.PhoneNumber,
            Occupation = parent.Occupation,
            WorkPhone = parent.WorkPhone,
            Students = parent.Students
                .Where(sp => !sp.IsDeleted)
                .Select(sp => new ParentStudentInfo
                {
                    StudentId = sp.StudentId,
                    StudentName = sp.Student != null && sp.Student.User != null
                        ? $"{sp.Student.User.FirstName} {sp.Student.User.LastName}"
                        : "",
                    Relationship = sp.Relationship,
                    IsPrimaryContact = sp.IsPrimaryContact,
                    IsEmergencyContact = sp.IsEmergencyContact
                }).ToList(),
            CreatedDate = parent.CreatedAt
        };
    }
}
