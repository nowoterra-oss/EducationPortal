using EduPortal.Application.DTOs.StudyAbroad;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class StudyAbroadService : IStudyAbroadService
{
    private readonly ApplicationDbContext _context;

    public StudyAbroadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudyAbroadProgramDto>> GetAllProgramsAsync()
    {
        var programs = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Counselor).ThenInclude(c => c.User)
            .Include(p => p.Documents)
            .Include(p => p.VisaProcesses)
            .Include(p => p.Accommodations)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return programs.Select(MapToDto);
    }

    public async Task<IEnumerable<StudyAbroadProgramDto>> GetActiveProgramsAsync()
    {
        var programs = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Counselor).ThenInclude(c => c.User)
            .Include(p => p.Documents)
            .Include(p => p.VisaProcesses)
            .Include(p => p.Accommodations)
            .Where(p => !p.IsDeleted &&
                       (p.Status == StudyAbroadStatus.Planning ||
                        p.Status == StudyAbroadStatus.Preparing))
            .ToListAsync();

        return programs.Select(MapToDto);
    }

    public async Task<IEnumerable<StudyAbroadProgramDto>> GetProgramsByStudentAsync(int studentId)
    {
        var programs = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Counselor).ThenInclude(c => c.User)
            .Include(p => p.Documents)
            .Include(p => p.VisaProcesses)
            .Include(p => p.Accommodations)
            .Where(p => p.StudentId == studentId && !p.IsDeleted)
            .ToListAsync();

        return programs.Select(MapToDto);
    }

    public async Task<IEnumerable<StudyAbroadProgramDto>> GetProgramsByCounselorAsync(int counselorId)
    {
        var programs = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Counselor).ThenInclude(c => c.User)
            .Include(p => p.Documents)
            .Include(p => p.VisaProcesses)
            .Include(p => p.Accommodations)
            .Where(p => p.CounselorId == counselorId && !p.IsDeleted)
            .ToListAsync();

        return programs.Select(MapToDto);
    }

    public async Task<IEnumerable<ProgramSummaryDto>> GetProgramSummariesAsync()
    {
        var programs = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Documents)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return programs.Select(p => new ProgramSummaryDto
        {
            Id = p.Id,
            StudentName = $"{p.Student.User.FirstName} {p.Student.User.LastName}",
            TargetCountry = p.TargetCountry,
            TargetUniversity = p.TargetUniversity,
            Status = p.Status.ToString(),
            IntendedStartDate = p.IntendedStartDate,
            ProgressPercentage = CalculateProgress(p)
        });
    }

    public async Task<StudyAbroadProgramDto?> GetProgramByIdAsync(int id)
    {
        var program = await _context.StudyAbroadPrograms
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Counselor).ThenInclude(c => c.User)
            .Include(p => p.Documents)
            .Include(p => p.VisaProcesses)
            .Include(p => p.Accommodations)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return program != null ? MapToDto(program) : null;
    }

    public async Task<StudyAbroadProgramDto> CreateProgramAsync(CreateStudyAbroadProgramDto dto)
    {
        var program = new StudyAbroadProgram
        {
            StudentId = dto.StudentId,
            CounselorId = dto.CounselorId,
            TargetCountry = dto.TargetCountry,
            TargetUniversity = dto.TargetUniversity,
            ProgramName = dto.ProgramName,
            Level = (ProgramLevel)dto.Level,
            IntendedStartDate = dto.IntendedStartDate,
            Status = StudyAbroadStatus.Planning,
            Requirements = dto.Requirements,
            EstimatedCost = dto.EstimatedCost,
            Notes = dto.Notes
        };

        _context.StudyAbroadPrograms.Add(program);
        await _context.SaveChangesAsync();

        return (await GetProgramByIdAsync(program.Id))!;
    }

    public async Task<StudyAbroadProgramDto> UpdateProgramAsync(int id, UpdateStudyAbroadProgramDto dto)
    {
        var program = await _context.StudyAbroadPrograms.FindAsync(id);
        if (program == null || program.IsDeleted)
            throw new Exception("Program not found");

        program.TargetCountry = dto.TargetCountry;
        program.TargetUniversity = dto.TargetUniversity;
        program.ProgramName = dto.ProgramName;
        program.Level = (ProgramLevel)dto.Level;
        program.IntendedStartDate = dto.IntendedStartDate;
        program.Status = (StudyAbroadStatus)dto.Status;
        program.Requirements = dto.Requirements;
        program.EstimatedCost = dto.EstimatedCost;
        program.ActualCost = dto.ActualCost;
        program.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return (await GetProgramByIdAsync(id))!;
    }

    public async Task<bool> DeleteProgramAsync(int id)
    {
        var program = await _context.StudyAbroadPrograms.FindAsync(id);
        if (program == null || program.IsDeleted)
            return false;

        program.IsDeleted = true;
        program.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ProgramStatisticsDto> GetStatisticsAsync()
    {
        var programs = await _context.StudyAbroadPrograms
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var stats = new ProgramStatisticsDto
        {
            TotalPrograms = programs.Count,
            ActivePrograms = programs.Count(p => p.Status == StudyAbroadStatus.Preparing),
            CompletedPrograms = programs.Count(p => p.Status == StudyAbroadStatus.Completed),
            CancelledPrograms = programs.Count(p => p.Status == StudyAbroadStatus.Cancelled)
        };

        // Programs by country
        stats.ProgramsByCountry = programs
            .GroupBy(p => p.TargetCountry)
            .ToDictionary(g => g.Key, g => g.Count());

        // Programs by status
        stats.ProgramsByStatus = programs
            .GroupBy(p => p.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Programs by level
        stats.ProgramsByLevel = programs
            .GroupBy(p => p.Level.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Costs
        stats.TotalEstimatedCost = programs.Sum(p => p.EstimatedCost ?? 0);
        stats.TotalActualCost = programs.Sum(p => p.ActualCost ?? 0);
        stats.AverageCost = programs.Any() ? programs.Average(p => p.EstimatedCost ?? 0) : 0;

        return stats;
    }

    private StudyAbroadProgramDto MapToDto(StudyAbroadProgram program)
    {
        var totalDocs = program.Documents.Count;
        var completedDocs = program.Documents.Count(d => d.Status == DocumentStatus.Approved);
        var pendingDocs = program.Documents.Count(d => d.Status == DocumentStatus.NotStarted || d.Status == DocumentStatus.InProgress);

        var hasVisa = program.VisaProcesses.Any();
        var visaStatus = program.VisaProcesses.OrderByDescending(v => v.CreatedAt)
            .FirstOrDefault()?.Status.ToString();

        var hasAccommodation = program.Accommodations.Any();
        var accommodationStatus = program.Accommodations.OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault()?.Status.ToString();

        return new StudyAbroadProgramDto
        {
            Id = program.Id,
            StudentId = program.StudentId,
            StudentName = $"{program.Student.User.FirstName} {program.Student.User.LastName}",
            StudentNo = program.Student.StudentNo,
            CounselorId = program.CounselorId,
            CounselorName = $"{program.Counselor.User.FirstName} {program.Counselor.User.LastName}",
            TargetCountry = program.TargetCountry,
            TargetUniversity = program.TargetUniversity,
            ProgramName = program.ProgramName,
            Level = program.Level.ToString(),
            IntendedStartDate = program.IntendedStartDate,
            Status = program.Status.ToString(),
            Requirements = program.Requirements,
            EstimatedCost = program.EstimatedCost,
            ActualCost = program.ActualCost,
            Notes = program.Notes,
            TotalDocuments = totalDocs,
            CompletedDocuments = completedDocs,
            PendingDocuments = pendingDocs,
            HasVisa = hasVisa,
            VisaStatus = visaStatus,
            HasAccommodation = hasAccommodation,
            AccommodationStatus = accommodationStatus,
            ProgressPercentage = CalculateProgress(program),
            CreatedDate = program.CreatedAt,
            LastModifiedDate = program.UpdatedAt
        };
    }

    private int CalculateProgress(StudyAbroadProgram program)
    {
        int progress = 0;

        // Documents (25%)
        if (program.Documents.Any() && program.Documents.All(d => d.Status == DocumentStatus.Approved))
            progress += 25;

        // Visa (25%)
        if (program.VisaProcesses.Any(v => v.Status == VisaStatus.Approved))
            progress += 25;

        // Accommodation (25%)
        if (program.Accommodations.Any(a => a.Status == AccommodationStatus.Confirmed))
            progress += 25;

        // Status (25%)
        if (program.Status == StudyAbroadStatus.Completed)
            progress += 25;

        return progress;
    }
}
