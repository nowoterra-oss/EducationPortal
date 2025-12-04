using EduPortal.Application.DTOs.AGP;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class AGPService : IAGPService
{
    private readonly ApplicationDbContext _context;

    public AGPService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<AGPDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.AcademicDevelopmentPlans
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Activities)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var entities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(MapToDto).ToList();

        return (items, totalCount);
    }

    public async Task<AGPDto?> GetByIdAsync(int id)
    {
        var agp = await _context.AcademicDevelopmentPlans
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones)
            .Include(a => a.Periods.OrderBy(p => p.Order))
                .ThenInclude(p => p.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Activities)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return agp == null ? null : MapToDto(agp);
    }

    public async Task<AGPDto> CreateAsync(CreateAGPDto dto)
    {
        var agp = new AcademicDevelopmentPlan
        {
            StudentId = dto.StudentId,
            AcademicYear = dto.AcademicYear,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PlanDocumentUrl = dto.PlanDocumentUrl,
            Status = dto.Status
        };

        // Timeline periods ekleme
        if (dto.Periods != null && dto.Periods.Any())
        {
            agp.Periods = dto.Periods.Select(p => new AgpPeriod
            {
                Title = p.Title,
                StartDate = DateTime.Parse(p.StartDate),
                EndDate = DateTime.Parse(p.EndDate),
                Color = p.Color,
                Order = p.Order,
                Milestones = p.Milestones?.Select(m => new AgpTimelineMilestone
                {
                    Title = m.Title,
                    Date = DateTime.Parse(m.Date),
                    Color = m.Color,
                    Type = m.Type
                }).ToList() ?? new List<AgpTimelineMilestone>(),
                Activities = p.Activities?.Select(a => new AgpActivity
                {
                    Title = a.Title,
                    HoursPerWeek = a.HoursPerWeek,
                    Notes = a.Notes
                }).ToList() ?? new List<AgpActivity>()
            }).ToList();
        }

        _context.AcademicDevelopmentPlans.Add(agp);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(agp.Id) ?? throw new InvalidOperationException("AGP oluşturulamadı");
    }

    public async Task<AGPDto> UpdateAsync(int id, UpdateAGPDto dto)
    {
        var agp = await _context.AcademicDevelopmentPlans
            .Include(a => a.Periods)
                .ThenInclude(p => p.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Activities)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agp == null)
            throw new KeyNotFoundException("AGP bulunamadı");

        // Mevcut alanları güncelle
        agp.AcademicYear = dto.AcademicYear;
        agp.StartDate = dto.StartDate;
        agp.EndDate = dto.EndDate;
        agp.PlanDocumentUrl = dto.PlanDocumentUrl;
        agp.Status = dto.Status;

        // Periods güncelle (varsa)
        if (dto.Periods != null)
        {
            // 1. Mevcut periods ve ilişkili verileri veritabanından sil
            var existingPeriods = await _context.AgpPeriods
                .Where(p => p.AgpId == id)
                .Include(p => p.Milestones)
                .Include(p => p.Activities)
                .ToListAsync();

            foreach (var existingPeriod in existingPeriods)
            {
                _context.AgpTimelineMilestones.RemoveRange(existingPeriod.Milestones);
                _context.AgpActivities.RemoveRange(existingPeriod.Activities);
            }
            _context.AgpPeriods.RemoveRange(existingPeriods);
            await _context.SaveChangesAsync();

            // 2. Yeni periods'ları doğrudan DbSet'e ekle
            foreach (var periodDto in dto.Periods)
            {
                var period = new AgpPeriod
                {
                    AgpId = id,
                    Title = periodDto.Title,
                    StartDate = DateTime.Parse(periodDto.StartDate),
                    EndDate = DateTime.Parse(periodDto.EndDate),
                    Color = periodDto.Color,
                    Order = periodDto.Order,
                    Milestones = periodDto.Milestones?.Select(m => new AgpTimelineMilestone
                    {
                        Title = m.Title,
                        Date = DateTime.Parse(m.Date),
                        Color = m.Color,
                        Type = m.Type
                    }).ToList() ?? new List<AgpTimelineMilestone>(),
                    Activities = periodDto.Activities?.Select(a => new AgpActivity
                    {
                        Title = a.Title,
                        HoursPerWeek = a.HoursPerWeek,
                        Notes = a.Notes
                    }).ToList() ?? new List<AgpActivity>()
                };
                _context.AgpPeriods.Add(period);
            }
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("AGP güncellenemedi");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var agp = await _context.AcademicDevelopmentPlans
            .Include(a => a.Milestones)
            .Include(a => a.Periods)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agp == null)
            return false;

        // Remove milestones and periods first (cascade will handle nested entities)
        _context.AGPMilestones.RemoveRange(agp.Milestones);
        _context.AgpPeriods.RemoveRange(agp.Periods);
        _context.AcademicDevelopmentPlans.Remove(agp);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AGPDto>> GetByStudentAsync(int studentId)
    {
        var agps = await _context.AcademicDevelopmentPlans
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones)
            .Include(a => a.Periods.OrderBy(p => p.Order))
                .ThenInclude(p => p.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Activities)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.StartDate)
            .AsNoTracking()
            .ToListAsync();

        return agps.Select(MapToDto);
    }

    public async Task<IEnumerable<AGPGoalDto>> GetGoalsAsync(int agpId)
    {
        var milestones = await _context.AGPMilestones
            .Where(m => m.AGPId == agpId)
            .OrderBy(m => m.Month)
            .ThenBy(m => m.StartDate)
            .AsNoTracking()
            .ToListAsync();

        return milestones.Select(MapToGoalDto);
    }

    public async Task<AGPGoalDto> AddGoalAsync(int agpId, CreateAGPGoalDto dto)
    {
        var agpExists = await _context.AcademicDevelopmentPlans.AnyAsync(a => a.Id == agpId);
        if (!agpExists)
            throw new KeyNotFoundException("AGP bulunamadı");

        var milestone = new AGPMilestone
        {
            AGPId = agpId,
            Month = dto.Month,
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            CompletionPercentage = dto.CompletionPercentage,
            Notes = dto.Notes
        };

        _context.AGPMilestones.Add(milestone);
        await _context.SaveChangesAsync();

        return MapToGoalDto(milestone);
    }

    public async Task<AGPGoalDto> UpdateGoalAsync(int agpId, int goalId, UpdateAGPGoalDto dto)
    {
        var milestone = await _context.AGPMilestones
            .FirstOrDefaultAsync(m => m.Id == goalId && m.AGPId == agpId);

        if (milestone == null)
            throw new KeyNotFoundException("Hedef bulunamadı");

        milestone.Month = dto.Month;
        milestone.Title = dto.Title;
        milestone.Description = dto.Description;
        milestone.StartDate = dto.StartDate;
        milestone.EndDate = dto.EndDate;
        milestone.Status = dto.Status;
        milestone.CompletionPercentage = dto.CompletionPercentage;
        milestone.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return MapToGoalDto(milestone);
    }

    public async Task<bool> DeleteGoalAsync(int agpId, int goalId)
    {
        var milestone = await _context.AGPMilestones
            .FirstOrDefaultAsync(m => m.Id == goalId && m.AGPId == agpId);

        if (milestone == null)
            return false;

        _context.AGPMilestones.Remove(milestone);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<AGPProgressDto> GetProgressAsync(int agpId)
    {
        var agp = await _context.AcademicDevelopmentPlans
            .Include(a => a.Milestones)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agpId);

        if (agp == null)
            throw new KeyNotFoundException("AGP bulunamadı");

        var milestones = agp.Milestones.ToList();
        var totalMilestones = milestones.Count;
        var completedMilestones = milestones.Count(m => m.Status == MilestoneStatus.Tamamlandi);
        var inProgressMilestones = milestones.Count(m => m.Status == MilestoneStatus.Devam);
        var pendingMilestones = milestones.Count(m => m.Status == MilestoneStatus.Bekliyor);

        var overallPercentage = totalMilestones > 0
            ? (int)milestones.Average(m => m.CompletionPercentage)
            : 0;

        var monthlyProgress = milestones
            .GroupBy(m => m.Month)
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyProgressDto
            {
                Month = g.Key,
                MonthName = GetMonthName(g.Key),
                MilestoneCount = g.Count(),
                CompletedCount = g.Count(m => m.Status == MilestoneStatus.Tamamlandi),
                AverageCompletionPercentage = (int)g.Average(m => m.CompletionPercentage)
            })
            .ToList();

        return new AGPProgressDto
        {
            AGPId = agpId,
            AcademicYear = agp.AcademicYear,
            TotalMilestones = totalMilestones,
            CompletedMilestones = completedMilestones,
            InProgressMilestones = inProgressMilestones,
            PendingMilestones = pendingMilestones,
            OverallCompletionPercentage = overallPercentage,
            MonthlyProgress = monthlyProgress
        };
    }

    private static AGPDto MapToDto(AcademicDevelopmentPlan agp)
    {
        var milestones = agp.Milestones?.ToList() ?? new List<AGPMilestone>();
        var completedCount = milestones.Count(m => m.Status == MilestoneStatus.Tamamlandi);
        var overallProgress = milestones.Count > 0
            ? (int)milestones.Average(m => m.CompletionPercentage)
            : 0;

        return new AGPDto
        {
            Id = agp.Id,
            StudentId = agp.StudentId,
            StudentName = agp.Student?.User != null
                ? $"{agp.Student.User.FirstName} {agp.Student.User.LastName}"
                : string.Empty,
            AcademicYear = agp.AcademicYear,
            StartDate = agp.StartDate,
            EndDate = agp.EndDate,
            PlanDocumentUrl = agp.PlanDocumentUrl,
            Status = agp.Status,
            MilestoneCount = milestones.Count,
            CompletedMilestoneCount = completedCount,
            OverallProgress = overallProgress,
            Milestones = milestones.Select(MapToGoalDto).ToList(),
            Periods = agp.Periods?.OrderBy(p => p.Order).Select(MapToPeriodDto).ToList() ?? new List<AgpPeriodDto>()
        };
    }

    private static AgpPeriodDto MapToPeriodDto(AgpPeriod period)
    {
        return new AgpPeriodDto
        {
            Id = period.Id,
            Title = period.Title,
            StartDate = period.StartDate.ToString("yyyy-MM-dd"),
            EndDate = period.EndDate.ToString("yyyy-MM-dd"),
            Color = period.Color,
            Order = period.Order,
            Milestones = period.Milestones?.Select(m => new AgpMilestoneDto
            {
                Id = m.Id,
                Title = m.Title,
                Date = m.Date.ToString("yyyy-MM-dd"),
                Color = m.Color,
                Type = m.Type
            }).ToList() ?? new List<AgpMilestoneDto>(),
            Activities = period.Activities?.Select(a => new AgpActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                HoursPerWeek = a.HoursPerWeek,
                Notes = a.Notes
            }).ToList() ?? new List<AgpActivityDto>()
        };
    }

    private static AGPGoalDto MapToGoalDto(AGPMilestone milestone)
    {
        return new AGPGoalDto
        {
            Id = milestone.Id,
            AGPId = milestone.AGPId,
            Month = milestone.Month,
            Title = milestone.Title,
            Description = milestone.Description,
            StartDate = milestone.StartDate,
            EndDate = milestone.EndDate,
            Status = milestone.Status,
            CompletionPercentage = milestone.CompletionPercentage,
            Notes = milestone.Notes
        };
    }

    private static string GetMonthName(int month) => month switch
    {
        1 => "Ocak",
        2 => "Şubat",
        3 => "Mart",
        4 => "Nisan",
        5 => "Mayıs",
        6 => "Haziran",
        7 => "Temmuz",
        8 => "Ağustos",
        9 => "Eylül",
        10 => "Ekim",
        11 => "Kasım",
        12 => "Aralık",
        _ => month.ToString()
    };
}
