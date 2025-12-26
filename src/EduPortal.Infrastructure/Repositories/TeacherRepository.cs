using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context)
    {
    }
    public override async Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.User.LastName)
            .ThenBy(t => t.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Teacher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }
    public async Task<Teacher?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
    }

    public async Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Include(t => t.Courses)
            .Include(t => t.CounselorProfile)
            .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);
    }

    public async Task<Teacher?> GetTeacherWithExtendedDetailsAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Include(t => t.Address)
            .Include(t => t.TeacherBranches).ThenInclude(b => b.Course)
            .Include(t => t.TeacherCertificates)
            .Include(t => t.TeacherReferences)
            .Include(t => t.TeacherWorkTypes)
            .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> GetTeachersByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Where(t => t.BranchId == branchId && t.IsActive)
            .OrderBy(t => t.User.LastName)
            .ThenBy(t => t.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Where(t =>
                t.User.FirstName.Contains(searchTerm) ||
                t.User.LastName.Contains(searchTerm) ||
                t.User.Email!.Contains(searchTerm) ||
                (t.Specialization != null && t.Specialization.Contains(searchTerm)))
            .OrderBy(t => t.User.LastName)
            .ThenBy(t => t.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Branch)
            .Where(t => t.IsActive)
            .OrderBy(t => t.User.LastName)
            .ThenBy(t => t.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var teacher = await _dbSet
            .Include(t => t.Courses)
            .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);

        return teacher?.Courses ?? new List<Course>();
    }
}
