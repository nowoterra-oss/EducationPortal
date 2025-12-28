using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class AgpPeriodRepository : GenericRepository<AgpPeriod>, IAgpPeriodRepository
{
    public AgpPeriodRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AgpPeriod>> GetByAgpIdAsync(int agpId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AgpId == agpId && !p.IsDeleted)
            .OrderBy(p => p.Order)
            .ThenBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<AgpPeriod?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(p => p.Activities.Where(a => !a.IsDeleted))
            .Include(p => p.Agp)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<AgpPeriod>> GetByAgpIdWithDetailsAsync(int agpId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(p => p.Activities.Where(a => !a.IsDeleted))
            .Include(p => p.Agp)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
            .Where(p => p.AgpId == agpId && !p.IsDeleted)
            .OrderBy(p => p.Order)
            .ThenBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AgpPeriod>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(p => p.Activities.Where(a => !a.IsDeleted))
            .Include(p => p.Agp)
            .Where(p => p.Agp.StudentId == studentId && !p.IsDeleted && !p.Agp.IsDeleted)
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AgpPeriod>> FindOverlappingPeriodsAsync(
        int agpId,
        DateTime startDate,
        DateTime endDate,
        int? excludePeriodId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.AgpId == agpId && !p.IsDeleted)
            .Where(p => (p.StartDate <= endDate && p.EndDate >= startDate));

        if (excludePeriodId.HasValue)
        {
            query = query.Where(p => p.Id != excludePeriodId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<DateTime?> GetEarliestStartDateAsync(int agpId, CancellationToken cancellationToken = default)
    {
        var period = await _dbSet
            .Where(p => p.AgpId == agpId && !p.IsDeleted)
            .OrderBy(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return period?.StartDate;
    }

    public async Task<DateTime?> GetLatestEndDateAsync(int agpId, CancellationToken cancellationToken = default)
    {
        var period = await _dbSet
            .Where(p => p.AgpId == agpId && !p.IsDeleted)
            .OrderByDescending(p => p.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        return period?.EndDate;
    }
}
