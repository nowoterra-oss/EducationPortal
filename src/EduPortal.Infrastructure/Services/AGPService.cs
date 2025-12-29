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
            .Where(a => !a.IsDeleted)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Activities.Where(act => !act.IsDeleted))
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
            .Where(a => !a.IsDeleted)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Activities.Where(act => !act.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return agp == null ? null : MapToDto(agp);
    }

    public async Task<AGPDto> CreateAsync(CreateAGPDto dto)
    {
        try
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
                var orderIndex = 0;
                agp.Periods = dto.Periods.Select((p, index) =>
                {
                    // Period tarihleri boş ise AGP tarihlerini kullan
                    var periodStartDate = !string.IsNullOrEmpty(p.StartDate) && DateTime.TryParse(p.StartDate, out var pStart)
                        ? pStart : dto.StartDate;
                    var periodEndDate = !string.IsNullOrEmpty(p.EndDate) && DateTime.TryParse(p.EndDate, out var pEnd)
                        ? pEnd : dto.EndDate;

                    // Title boş ise otomatik oluştur
                    var periodTitle = !string.IsNullOrWhiteSpace(p.Title)
                        ? p.Title
                        : $"Dönem {index + 1}";

                    return new AgpPeriod
                    {
                        Title = periodTitle,
                        PeriodName = p.PeriodName,
                        StartDate = periodStartDate,
                        EndDate = periodEndDate,
                        Color = p.Color ?? "#3B82F6",
                        Order = p.Order > 0 ? p.Order : orderIndex++,
                        Milestones = p.Milestones?.Select(m => new AgpTimelineMilestone
                        {
                            Title = m.Title ?? string.Empty,
                            Date = DateTime.TryParse(m.Date, out var milestoneDate) ? milestoneDate : DateTime.UtcNow,
                            Color = m.Color,
                            Type = string.IsNullOrWhiteSpace(m.Type) ? "exam" : m.Type,
                            IsMilestone = m.IsMilestone,
                            ApplicationStartDate = !string.IsNullOrEmpty(m.ApplicationStartDate) && DateTime.TryParse(m.ApplicationStartDate, out var appStart)
                                ? appStart : null,
                            ApplicationEndDate = !string.IsNullOrEmpty(m.ApplicationEndDate) && DateTime.TryParse(m.ApplicationEndDate, out var appEnd)
                                ? appEnd : null,
                            Score = m.Score,
                            MaxScore = m.MaxScore,
                            ResultNotes = m.ResultNotes,
                            IsCompleted = m.IsCompleted
                        }).ToList() ?? new List<AgpTimelineMilestone>(),
                        Activities = p.Activities?.Select(a => new AgpActivity
                        {
                            Title = a.Title ?? string.Empty,
                            StartDate = !string.IsNullOrEmpty(a.StartDate) && DateTime.TryParse(a.StartDate, out var aStart)
                                ? aStart : periodStartDate,
                            EndDate = !string.IsNullOrEmpty(a.EndDate) && DateTime.TryParse(a.EndDate, out var aEnd)
                                ? aEnd : periodEndDate,
                            HoursPerWeek = a.HoursPerWeek,
                            OwnerType = a.OwnerType,
                            Status = !string.IsNullOrEmpty(a.Status) && Enum.TryParse<ActivityStatus>(a.Status, true, out var status)
                                ? status : ActivityStatus.Planned,
                            NeedsReview = a.NeedsReview,
                            Notes = a.Notes
                        }).ToList() ?? new List<AgpActivity>()
                    };
                }).ToList();
            }

            _context.AcademicDevelopmentPlans.Add(agp);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(agp.Id) ?? throw new InvalidOperationException("AGP oluşturulamadı");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"AGP oluşturulurken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}", ex);
        }
    }

    public async Task<AGPDto> UpdateAsync(int id, UpdateAGPDto dto)
    {
        // NOT: Periods'ı Include etmiyoruz çünkü zaten silip yeniden oluşturacağız
        // Bu sayede tracked entity sorunu yaşamıyoruz
        var agp = await _context.AcademicDevelopmentPlans
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
            // 1. Mevcut periods'ların ID'lerini al
            var existingPeriodIds = await _context.AgpPeriods
                .Where(p => p.AgpId == id)
                .Select(p => p.Id)
                .ToListAsync();

            // 2. İlişkili verileri sil (ExecuteDeleteAsync - bypass change tracker)
            if (existingPeriodIds.Any())
            {
                await _context.AgpActivities
                    .Where(a => existingPeriodIds.Contains(a.AgpPeriodId))
                    .ExecuteDeleteAsync();

                await _context.AgpTimelineMilestones
                    .Where(m => existingPeriodIds.Contains(m.AgpPeriodId))
                    .ExecuteDeleteAsync();

                await _context.AgpPeriods
                    .Where(p => p.AgpId == id)
                    .ExecuteDeleteAsync();
            }

            // 3. Yeni periods'ları ekle
            var periodIndex = 0;
            foreach (var periodDto in dto.Periods)
            {
                // Period tarihleri boş ise AGP tarihlerini kullan
                var periodStartDate = !string.IsNullOrEmpty(periodDto.StartDate) && DateTime.TryParse(periodDto.StartDate, out var pStart)
                    ? pStart : dto.StartDate;
                var periodEndDate = !string.IsNullOrEmpty(periodDto.EndDate) && DateTime.TryParse(periodDto.EndDate, out var pEnd)
                    ? pEnd : dto.EndDate;

                // Title boş ise otomatik oluştur
                var periodTitle = !string.IsNullOrWhiteSpace(periodDto.Title)
                    ? periodDto.Title
                    : $"Dönem {periodIndex + 1}";

                var period = new AgpPeriod
                {
                    AgpId = id,
                    Title = periodTitle,
                    StartDate = periodStartDate,
                    EndDate = periodEndDate,
                    Color = periodDto.Color ?? "#3B82F6",
                    Order = periodDto.Order,
                    Milestones = periodDto.Milestones?.Select(m => new AgpTimelineMilestone
                    {
                        Title = m.Title ?? string.Empty,
                        Date = DateTime.TryParse(m.Date, out var milestoneDate) ? milestoneDate : DateTime.UtcNow,
                        Color = m.Color,
                        Type = string.IsNullOrWhiteSpace(m.Type) ? "exam" : m.Type,
                        IsMilestone = m.IsMilestone,
                        ApplicationStartDate = !string.IsNullOrEmpty(m.ApplicationStartDate) && DateTime.TryParse(m.ApplicationStartDate, out var appStart)
                            ? appStart : null,
                        ApplicationEndDate = !string.IsNullOrEmpty(m.ApplicationEndDate) && DateTime.TryParse(m.ApplicationEndDate, out var appEnd)
                            ? appEnd : null,
                        Score = m.Score,
                        MaxScore = m.MaxScore,
                        ResultNotes = m.ResultNotes,
                        IsCompleted = m.IsCompleted
                    }).ToList() ?? new List<AgpTimelineMilestone>(),
                    Activities = periodDto.Activities?.Select(a => new AgpActivity
                    {
                        Title = a.Title ?? string.Empty,
                        StartDate = !string.IsNullOrEmpty(a.StartDate) && DateTime.TryParse(a.StartDate, out var aStart)
                            ? aStart : periodStartDate,
                        EndDate = !string.IsNullOrEmpty(a.EndDate) && DateTime.TryParse(a.EndDate, out var aEnd)
                            ? aEnd : periodEndDate,
                        HoursPerWeek = a.HoursPerWeek,
                        OwnerType = a.OwnerType,
                        Status = !string.IsNullOrEmpty(a.Status) && Enum.TryParse<ActivityStatus>(a.Status, true, out var status)
                            ? status : ActivityStatus.Planned,
                        NeedsReview = a.NeedsReview,
                        Notes = a.Notes
                    }).ToList() ?? new List<AgpActivity>()
                };
                _context.AgpPeriods.Add(period);
                periodIndex++;
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
                .ThenInclude(p => p.Milestones)
            .Include(a => a.Periods)
                .ThenInclude(p => p.Activities)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (agp == null)
            return false;

        // Soft delete - tüm ilişkili kayıtları da soft delete yap
        agp.IsDeleted = true;
        agp.UpdatedAt = DateTime.UtcNow;

        foreach (var milestone in agp.Milestones)
        {
            milestone.IsDeleted = true;
            milestone.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var period in agp.Periods)
        {
            period.IsDeleted = true;
            period.UpdatedAt = DateTime.UtcNow;

            foreach (var periodMilestone in period.Milestones)
            {
                periodMilestone.IsDeleted = true;
                periodMilestone.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var activity in period.Activities)
            {
                activity.IsDeleted = true;
                activity.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AGPDto>> GetByStudentAsync(int studentId)
    {
        var agps = await _context.AcademicDevelopmentPlans
            .Where(a => a.StudentId == studentId && !a.IsDeleted)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Milestones.Where(m => !m.IsDeleted))
            .Include(a => a.Periods.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Activities.Where(act => !act.IsDeleted))
            .OrderByDescending(a => a.StartDate)
            .AsNoTracking()
            .ToListAsync();

        return agps.Select(MapToDto);
    }

    public async Task<IEnumerable<AGPGoalDto>> GetGoalsAsync(int agpId)
    {
        var milestones = await _context.AGPMilestones
            .Where(m => m.AGPId == agpId && !m.IsDeleted)
            .OrderBy(m => m.Month)
            .ThenBy(m => m.StartDate)
            .AsNoTracking()
            .ToListAsync();

        return milestones.Select(MapToGoalDto);
    }

    public async Task<AGPGoalDto> AddGoalAsync(int agpId, CreateAGPGoalDto dto)
    {
        var agpExists = await _context.AcademicDevelopmentPlans.AnyAsync(a => a.Id == agpId && !a.IsDeleted);
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
            .FirstOrDefaultAsync(m => m.Id == goalId && m.AGPId == agpId && !m.IsDeleted);

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
            .FirstOrDefaultAsync(m => m.Id == goalId && m.AGPId == agpId && !m.IsDeleted);

        if (milestone == null)
            return false;

        // Soft delete
        milestone.IsDeleted = true;
        milestone.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<AGPProgressDto> GetProgressAsync(int agpId)
    {
        var agp = await _context.AcademicDevelopmentPlans
            .Where(a => !a.IsDeleted)
            .Include(a => a.Milestones.Where(m => !m.IsDeleted))
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
            PeriodName = period.PeriodName,
            Title = period.Title,
            StartDate = period.StartDate.ToString("yyyy-MM-dd"),
            EndDate = period.EndDate.ToString("yyyy-MM-dd"),
            Color = period.Color,
            Order = period.Order,
            Milestones = period.Milestones?.Where(m => !m.IsDeleted).Select(m => new AgpMilestoneDto
            {
                Id = m.Id,
                Title = m.Title,
                Date = m.Date.ToString("yyyy-MM-dd"),
                Color = m.Color,
                Type = m.Type,
                Category = m.Type, // Frontend uyumluluğu için
                IsMilestone = m.IsMilestone,
                ApplicationStartDate = m.ApplicationStartDate?.ToString("yyyy-MM-dd"),
                ApplicationEndDate = m.ApplicationEndDate?.ToString("yyyy-MM-dd"),
                Score = m.Score,
                MaxScore = m.MaxScore,
                ResultNotes = m.ResultNotes,
                IsCompleted = m.IsCompleted
            }).ToList() ?? new List<AgpMilestoneDto>(),
            Activities = period.Activities?.Where(a => !a.IsDeleted).Select(a => new AgpActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                StartDate = a.StartDate.ToString("yyyy-MM-dd"),
                EndDate = a.EndDate.ToString("yyyy-MM-dd"),
                HoursPerWeek = a.HoursPerWeek,
                OwnerType = a.OwnerType,
                Status = a.Status.ToString(),
                NeedsReview = a.NeedsReview,
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
