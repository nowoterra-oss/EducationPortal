using EduPortal.Application.DTOs.UniversityApplication;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class UniversityApplicationService : IUniversityApplicationService
{
    private readonly ApplicationDbContext _context;

    public UniversityApplicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UniversityApplicationDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.UniversityApplications
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<UniversityApplicationDto?> GetByIdAsync(int id)
    {
        var application = await _context.UniversityApplications
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return application == null ? null : MapToDto(application);
    }

    public async Task<UniversityApplicationDto> CreateAsync(CreateUniversityApplicationDto dto)
    {
        var application = new UniversityApplication
        {
            StudentId = dto.StudentId,
            Country = dto.Country,
            UniversityName = dto.UniversityName,
            Department = dto.Department,
            RequirementsUrl = dto.RequirementsUrl,
            ApplicationStartDate = dto.ApplicationStartDate,
            ApplicationDeadline = dto.ApplicationDeadline,
            DecisionDate = dto.DecisionDate,
            Status = dto.Status,
            Notes = dto.Notes
        };

        _context.UniversityApplications.Add(application);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(application.Id) ?? throw new InvalidOperationException("Başvuru oluşturulamadı");
    }

    public async Task<UniversityApplicationDto> UpdateAsync(int id, UpdateUniversityApplicationDto dto)
    {
        var application = await _context.UniversityApplications.FindAsync(id);

        if (application == null)
            throw new KeyNotFoundException("Başvuru bulunamadı");

        application.Country = dto.Country;
        application.UniversityName = dto.UniversityName;
        application.Department = dto.Department;
        application.RequirementsUrl = dto.RequirementsUrl;
        application.ApplicationStartDate = dto.ApplicationStartDate;
        application.ApplicationDeadline = dto.ApplicationDeadline;
        application.DecisionDate = dto.DecisionDate;
        application.Status = dto.Status;
        application.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Başvuru güncellenemedi");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var application = await _context.UniversityApplications.FindAsync(id);

        if (application == null)
            return false;

        _context.UniversityApplications.Remove(application);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<UniversityApplicationDto>> GetByStudentAsync(int studentId)
    {
        var applications = await _context.UniversityApplications
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.ApplicationDeadline)
            .AsNoTracking()
            .ToListAsync();

        return applications.Select(MapToDto);
    }

    public async Task<(IEnumerable<UniversityApplicationDto> Items, int TotalCount)> GetByStatusAsync(ApplicationStatus status, int pageNumber, int pageSize)
    {
        var query = _context.UniversityApplications
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.Status == status)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.ApplicationDeadline)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<UniversityApplicationDto> UpdateStatusAsync(int id, ApplicationStatusDto dto)
    {
        var application = await _context.UniversityApplications.FindAsync(id);

        if (application == null)
            throw new KeyNotFoundException("Başvuru bulunamadı");

        application.Status = dto.Status;
        if (dto.DecisionDate.HasValue)
            application.DecisionDate = dto.DecisionDate;
        if (!string.IsNullOrEmpty(dto.Note))
            application.Notes = string.IsNullOrEmpty(application.Notes)
                ? dto.Note
                : $"{application.Notes}\n[{DateTime.Now:dd.MM.yyyy}] {dto.Note}";

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Durum güncellenemedi");
    }

    public async Task<ApplicationDocumentResultDto> AddDocumentAsync(int applicationId, AddApplicationDocumentDto dto)
    {
        var application = await _context.UniversityApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new KeyNotFoundException("Başvuru bulunamadı");

        // In a real implementation, this would create a record in a separate ApplicationDocuments table
        // For now, we simulate the response
        return new ApplicationDocumentResultDto
        {
            DocumentId = new Random().Next(1000, 9999),
            ApplicationId = applicationId,
            DocumentType = dto.DocumentType,
            Title = dto.Title,
            DocumentUrl = dto.DocumentUrl,
            UploadedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ApplicationTimelineDto>> GetTimelineAsync(int applicationId)
    {
        var application = await _context.UniversityApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new KeyNotFoundException("Başvuru bulunamadı");

        // Generate timeline based on application data
        var timeline = new List<ApplicationTimelineDto>();

        // Application created
        timeline.Add(new ApplicationTimelineDto
        {
            Id = 1,
            Date = application.CreatedAt,
            Event = "Başvuru Oluşturuldu",
            Description = $"{application.UniversityName} - {application.Department} başvurusu oluşturuldu",
            NewStatus = ApplicationStatus.Planlaniyor
        });

        // Application start date if set
        if (application.ApplicationStartDate.HasValue)
        {
            timeline.Add(new ApplicationTimelineDto
            {
                Id = 2,
                Date = application.ApplicationStartDate.Value,
                Event = "Başvuru Dönemi Başladı",
                Description = "Üniversite başvuru dönemi başladı"
            });
        }

        // If status changed from planning
        if (application.Status != ApplicationStatus.Planlaniyor)
        {
            timeline.Add(new ApplicationTimelineDto
            {
                Id = 3,
                Date = application.UpdatedAt ?? application.CreatedAt.AddDays(1),
                Event = "Başvuru Yapıldı",
                Description = "Başvuru üniversiteye gönderildi",
                OldStatus = ApplicationStatus.Planlaniyor,
                NewStatus = ApplicationStatus.BasvuruYapildi
            });
        }

        // Decision if available
        if (application.DecisionDate.HasValue && (application.Status == ApplicationStatus.Kabul || application.Status == ApplicationStatus.Red))
        {
            var eventName = application.Status == ApplicationStatus.Kabul ? "Kabul Edildi" : "Reddedildi";
            timeline.Add(new ApplicationTimelineDto
            {
                Id = 4,
                Date = application.DecisionDate.Value,
                Event = eventName,
                Description = $"Üniversiteden sonuç: {eventName}",
                OldStatus = ApplicationStatus.BasvuruYapildi,
                NewStatus = application.Status
            });
        }

        // Deadline
        timeline.Add(new ApplicationTimelineDto
        {
            Id = 5,
            Date = application.ApplicationDeadline,
            Event = "Son Başvuru Tarihi",
            Description = "Başvuru için son tarih"
        });

        return timeline.OrderBy(t => t.Date).ToList();
    }

    public async Task<ApplicationStatisticsDto> GetStatisticsAsync()
    {
        var applications = await _context.UniversityApplications
            .AsNoTracking()
            .ToListAsync();

        var totalApplications = applications.Count;
        var planningCount = applications.Count(a => a.Status == ApplicationStatus.Planlaniyor);
        var appliedCount = applications.Count(a => a.Status == ApplicationStatus.BasvuruYapildi);
        var acceptedCount = applications.Count(a => a.Status == ApplicationStatus.Kabul);
        var rejectedCount = applications.Count(a => a.Status == ApplicationStatus.Red);
        var pendingCount = applications.Count(a => a.Status == ApplicationStatus.Beklemede);

        var decidedCount = acceptedCount + rejectedCount;
        var acceptanceRate = decidedCount > 0 ? (double)acceptedCount / decidedCount * 100 : 0;

        var upcomingDeadlines = applications.Count(a =>
            a.ApplicationDeadline >= DateTime.Today &&
            a.ApplicationDeadline <= DateTime.Today.AddDays(30) &&
            a.Status == ApplicationStatus.Planlaniyor);

        var byCountry = applications
            .GroupBy(a => a.Country)
            .Select(g => new CountryStatisticsDto
            {
                Country = g.Key,
                ApplicationCount = g.Count(),
                AcceptedCount = g.Count(a => a.Status == ApplicationStatus.Kabul),
                RejectedCount = g.Count(a => a.Status == ApplicationStatus.Red)
            })
            .OrderByDescending(c => c.ApplicationCount)
            .ToList();

        var byMonth = applications
            .GroupBy(a => new { a.ApplicationDeadline.Year, a.ApplicationDeadline.Month })
            .Select(g => new MonthlyStatisticsDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = GetMonthName(g.Key.Month),
                ApplicationCount = g.Count(),
                DeadlineCount = g.Count(a => a.Status == ApplicationStatus.Planlaniyor || a.Status == ApplicationStatus.BasvuruYapildi)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return new ApplicationStatisticsDto
        {
            TotalApplications = totalApplications,
            PlanningCount = planningCount,
            AppliedCount = appliedCount,
            AcceptedCount = acceptedCount,
            RejectedCount = rejectedCount,
            PendingCount = pendingCount,
            AcceptanceRate = Math.Round(acceptanceRate, 2),
            UpcomingDeadlinesCount = upcomingDeadlines,
            ByCountry = byCountry,
            ByMonth = byMonth
        };
    }

    private static UniversityApplicationDto MapToDto(UniversityApplication application)
    {
        return new UniversityApplicationDto
        {
            Id = application.Id,
            StudentId = application.StudentId,
            StudentName = application.Student?.User != null
                ? $"{application.Student.User.FirstName} {application.Student.User.LastName}"
                : string.Empty,
            Country = application.Country,
            UniversityName = application.UniversityName,
            Department = application.Department,
            RequirementsUrl = application.RequirementsUrl,
            ApplicationStartDate = application.ApplicationStartDate,
            ApplicationDeadline = application.ApplicationDeadline,
            DecisionDate = application.DecisionDate,
            Status = application.Status,
            Notes = application.Notes,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
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
