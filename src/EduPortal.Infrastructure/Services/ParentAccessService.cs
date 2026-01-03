using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

/// <summary>
/// Veli erişim kontrolü servisi implementasyonu.
/// Velilerin sadece kendilerine bağlı öğrencilerin verilerine erişmesini sağlar.
/// </summary>
public class ParentAccessService : IParentAccessService
{
    private readonly ApplicationDbContext _context;

    public ParentAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsParentAsync(string userId)
    {
        return await _context.Parents
            .AnyAsync(p => p.UserId == userId && !p.IsDeleted);
    }

    public async Task<int?> GetParentIdAsync(string userId)
    {
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        return parent?.Id;
    }

    public async Task<List<int>> GetLinkedStudentIdsAsync(int parentId)
    {
        return await _context.StudentParents
            .Where(sp => sp.ParentId == parentId && !sp.IsDeleted)
            .Select(sp => sp.StudentId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> CanAccessStudentAsync(string userId, int studentId)
    {
        var parentId = await GetParentIdAsync(userId);
        if (!parentId.HasValue)
            return false;

        return await _context.StudentParents
            .AnyAsync(sp => sp.ParentId == parentId.Value &&
                           sp.StudentId == studentId &&
                           !sp.IsDeleted);
    }

    public async Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds)
    {
        var parentId = await GetParentIdAsync(userId);
        if (!parentId.HasValue)
            return false;

        var linkedStudentIds = await GetLinkedStudentIdsAsync(parentId.Value);
        return studentIds.All(id => linkedStudentIds.Contains(id));
    }
}
