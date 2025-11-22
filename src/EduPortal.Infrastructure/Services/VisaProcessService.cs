using EduPortal.Application.DTOs.Visa;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class VisaProcessService : IVisaProcessService
{
    private readonly ApplicationDbContext _context;

    public VisaProcessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VisaProcessDto>> GetAllVisaProcessesAsync()
    {
        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(v => !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return visaProcesses.Select(MapToDto);
    }

    public async Task<IEnumerable<VisaProcessDto>> GetActiveVisaProcessesAsync()
    {
        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(v => !v.IsDeleted &&
                       (v.Status == VisaStatus.DocumentsPreparation ||
                        v.Status == VisaStatus.ApplicationSubmitted ||
                        v.Status == VisaStatus.InterviewScheduled ||
                        v.Status == VisaStatus.UnderReview))
            .ToListAsync();

        return visaProcesses.Select(MapToDto);
    }

    public async Task<IEnumerable<VisaProcessDto>> GetVisaProcessesByProgramAsync(int programId)
    {
        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(v => v.ProgramId == programId && !v.IsDeleted)
            .ToListAsync();

        return visaProcesses.Select(MapToDto);
    }

    public async Task<IEnumerable<VisaProcessDto>> GetPendingVisaProcessesAsync()
    {
        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(v => !v.IsDeleted &&
                       (v.Status == VisaStatus.DocumentsPreparation ||
                        v.Status == VisaStatus.ApplicationSubmitted))
            .ToListAsync();

        return visaProcesses.Select(MapToDto);
    }

    public async Task<IEnumerable<VisaProcessDto>> GetExpiringVisasAsync(int days = 90)
    {
        var futureDate = DateTime.UtcNow.AddDays(days);

        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .Where(v => !v.IsDeleted &&
                       v.Status == VisaStatus.Approved)
            .ToListAsync();

        // Filter expiring visas (Note: entity doesn't have ExpiryDate, returning approved visas)
        return visaProcesses.Select(MapToDto);
    }

    public async Task<VisaProcessDto?> GetVisaProcessByIdAsync(int id)
    {
        var visaProcess = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        return visaProcess != null ? MapToDto(visaProcess) : null;
    }

    public async Task<VisaTimelineDto> GetVisaTimelineAsync(int id)
    {
        var visaProcess = await _context.VisaProcesses
            .Include(v => v.Program).ThenInclude(p => p.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (visaProcess == null)
            throw new Exception("Visa process not found");

        var timeline = new VisaTimelineDto
        {
            VisaId = id,
            StudentName = $"{visaProcess.Program.Student.User.FirstName} {visaProcess.Program.Student.User.LastName}",
            TargetCountry = visaProcess.Program.TargetCountry,
            Events = new List<VisaTimelineEventDto>
            {
                new VisaTimelineEventDto
                {
                    Stage = "Documents Preparation",
                    Date = visaProcess.ApplicationDate,
                    Status = visaProcess.Status >= VisaStatus.DocumentsPreparation ? "Completed" : "Pending",
                    IsCompleted = visaProcess.Status >= VisaStatus.DocumentsPreparation
                },
                new VisaTimelineEventDto
                {
                    Stage = "Application Submitted",
                    Date = visaProcess.ApplicationDate,
                    Status = visaProcess.Status >= VisaStatus.ApplicationSubmitted ? "Completed" : "Pending",
                    IsCompleted = visaProcess.Status >= VisaStatus.ApplicationSubmitted
                },
                new VisaTimelineEventDto
                {
                    Stage = "Interview Scheduled",
                    Date = visaProcess.InterviewDate,
                    Status = visaProcess.Status >= VisaStatus.InterviewScheduled ? "Completed" : "Pending",
                    IsCompleted = visaProcess.Status >= VisaStatus.InterviewScheduled
                },
                new VisaTimelineEventDto
                {
                    Stage = "Under Review",
                    Date = visaProcess.InterviewDate,
                    Status = visaProcess.Status >= VisaStatus.UnderReview ? "Completed" : "Pending",
                    IsCompleted = visaProcess.Status >= VisaStatus.UnderReview
                },
                new VisaTimelineEventDto
                {
                    Stage = "Decision",
                    Date = visaProcess.DecisionDate,
                    Status = visaProcess.Status == VisaStatus.Approved ? "Approved" :
                            visaProcess.Status == VisaStatus.Rejected ? "Rejected" : "Pending",
                    IsCompleted = visaProcess.Status == VisaStatus.Approved ||
                                 visaProcess.Status == VisaStatus.Rejected,
                    Notes = visaProcess.Status == VisaStatus.Approved ? "Visa approved" : null
                }
            }
        };

        return timeline;
    }

    public async Task<VisaProcessDto> CreateVisaProcessAsync(CreateVisaProcessDto dto)
    {
        var visaProcess = new VisaProcess
        {
            ProgramId = dto.ProgramId,
            VisaType = dto.VisaType,
            Status = VisaStatus.DocumentsPreparation,
            ApplicationDate = dto.ApplicationDate,
            InterviewDate = dto.InterviewDate,
            ConsulateLocation = dto.Embassy,
            ApplicationFee = dto.ApplicationFee,
            Notes = dto.Notes
        };

        _context.VisaProcesses.Add(visaProcess);
        await _context.SaveChangesAsync();

        return (await GetVisaProcessByIdAsync(visaProcess.Id))!;
    }

    public async Task<VisaProcessDto> UpdateVisaProcessAsync(int id, UpdateVisaProcessDto dto)
    {
        var visaProcess = await _context.VisaProcesses.FindAsync(id);
        if (visaProcess == null || visaProcess.IsDeleted)
            throw new Exception("Visa process not found");

        visaProcess.VisaType = dto.VisaType;
        visaProcess.Status = (VisaStatus)dto.Status;
        visaProcess.ApplicationDate = dto.ApplicationDate;
        visaProcess.InterviewDate = dto.InterviewDate;
        visaProcess.DecisionDate = dto.ApprovalDate;
        visaProcess.ConsulateLocation = dto.Embassy;
        visaProcess.ApplicationFee = dto.ApplicationFee;
        visaProcess.Notes = dto.Notes;
        visaProcess.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetVisaProcessByIdAsync(id))!;
    }

    public async Task<bool> DeleteVisaProcessAsync(int id)
    {
        var visaProcess = await _context.VisaProcesses.FindAsync(id);
        if (visaProcess == null || visaProcess.IsDeleted)
            return false;

        visaProcess.IsDeleted = true;
        visaProcess.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<VisaStatisticsDto> GetStatisticsAsync()
    {
        var visaProcesses = await _context.VisaProcesses
            .Include(v => v.Program)
            .Where(v => !v.IsDeleted)
            .ToListAsync();

        var stats = new VisaStatisticsDto
        {
            TotalApplications = visaProcesses.Count,
            PendingApplications = visaProcesses.Count(v =>
                v.Status == VisaStatus.DocumentsPreparation ||
                v.Status == VisaStatus.ApplicationSubmitted),
            ApprovedApplications = visaProcesses.Count(v => v.Status == VisaStatus.Approved),
            RejectedApplications = visaProcesses.Count(v => v.Status == VisaStatus.Rejected),
            InProcessApplications = visaProcesses.Count(v =>
                v.Status == VisaStatus.InterviewScheduled ||
                v.Status == VisaStatus.UnderReview)
        };

        stats.ApplicationsByCountry = visaProcesses
            .GroupBy(v => v.Program.TargetCountry)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.ApplicationsByStatus = visaProcesses
            .GroupBy(v => v.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.TotalApplicationFees = visaProcesses.Sum(v => v.ApplicationFee ?? 0);

        var processedVisas = visaProcesses.Where(v => v.DecisionDate.HasValue).ToList();
        if (processedVisas.Any())
        {
            stats.AverageProcessingDays = (decimal)processedVisas
                .Average(v => (v.DecisionDate!.Value - v.ApplicationDate).TotalDays);
        }

        if (visaProcesses.Any())
        {
            stats.ApprovalRate = (decimal)stats.ApprovedApplications / visaProcesses.Count * 100;
        }

        return stats;
    }

    private VisaProcessDto MapToDto(VisaProcess visaProcess)
    {
        var now = DateTime.UtcNow;
        var daysInProcess = (int)(now - visaProcess.ApplicationDate).TotalDays;

        return new VisaProcessDto
        {
            Id = visaProcess.Id,
            ProgramId = visaProcess.ProgramId,
            StudentName = $"{visaProcess.Program.Student.User.FirstName} {visaProcess.Program.Student.User.LastName}",
            StudentNo = visaProcess.Program.Student.StudentNo,
            TargetCountry = visaProcess.Program.TargetCountry,
            TargetUniversity = visaProcess.Program.TargetUniversity,
            VisaType = visaProcess.VisaType,
            Status = visaProcess.Status.ToString(),
            ApplicationDate = visaProcess.ApplicationDate,
            InterviewDate = visaProcess.InterviewDate,
            ApprovalDate = visaProcess.DecisionDate,
            ExpiryDate = null, // Entity doesn't have this field
            Embassy = visaProcess.ConsulateLocation,
            ApplicationFee = visaProcess.ApplicationFee,
            VisaNumber = null, // Entity doesn't have this field
            Notes = visaProcess.Notes,
            DaysInProcess = daysInProcess,
            IsExpiringSoon = false,
            DaysUntilExpiry = 0,
            CreatedDate = visaProcess.CreatedAt,
            LastModifiedDate = visaProcess.UpdatedAt
        };
    }
}
