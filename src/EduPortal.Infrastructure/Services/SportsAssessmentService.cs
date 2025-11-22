using EduPortal.Application.DTOs.SportsAssessment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class SportsAssessmentService : ISportsAssessmentService
{
    private readonly ApplicationDbContext _context;

    public SportsAssessmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SportsAssessmentDto>> GetAllAssessmentsAsync()
    {
        var assessments = await _context.SportsAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Coach).ThenInclude(c => c.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<SportsAssessmentDto>> GetAssessmentsByStudentAsync(int studentId)
    {
        var assessments = await _context.SportsAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Coach).ThenInclude(c => c.User)
            .Where(a => a.StudentId == studentId && !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<SportsAssessmentDto>> GetAssessmentsByCoachAsync(int coachId)
    {
        var assessments = await _context.SportsAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Coach).ThenInclude(c => c.User)
            .Where(a => a.CoachId == coachId && !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<SportsAssessmentSummaryDto>> GetAssessmentSummariesAsync()
    {
        var assessments = await _context.SportsAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(a => new SportsAssessmentSummaryDto
        {
            Id = a.Id,
            StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
            CurrentSport = a.CurrentSport,
            SkillLevel = a.SkillLevel,
            AssessmentDate = a.AssessmentDate
        });
    }

    public async Task<SportsAssessmentDto?> GetAssessmentByIdAsync(int id)
    {
        var assessment = await _context.SportsAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Coach).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return assessment != null ? MapToDto(assessment) : null;
    }

    public async Task<SportsAssessmentDto> CreateAssessmentAsync(CreateSportsAssessmentDto dto)
    {
        var assessment = new SportsAssessment
        {
            StudentId = dto.StudentId,
            CoachId = dto.CoachId,
            AssessmentDate = dto.AssessmentDate,
            CurrentSport = dto.CurrentSport,
            YearsOfExperience = dto.YearsOfExperience,
            SkillLevel = dto.SkillLevel,
            Height = dto.Height,
            Weight = dto.Weight,
            PhysicalAttributes = dto.PhysicalAttributes,
            Strengths = dto.Strengths,
            Weaknesses = dto.Weaknesses,
            RecommendedSports = dto.RecommendedSports,
            DevelopmentPlan = dto.DevelopmentPlan,
            ReportUrl = dto.ReportUrl
        };

        _context.SportsAssessments.Add(assessment);
        await _context.SaveChangesAsync();

        return (await GetAssessmentByIdAsync(assessment.Id))!;
    }

    public async Task<SportsAssessmentDto> UpdateAssessmentAsync(int id, UpdateSportsAssessmentDto dto)
    {
        var assessment = await _context.SportsAssessments.FindAsync(id);
        if (assessment == null || assessment.IsDeleted)
            throw new Exception("Sports assessment not found");

        assessment.AssessmentDate = dto.AssessmentDate;
        assessment.CurrentSport = dto.CurrentSport;
        assessment.YearsOfExperience = dto.YearsOfExperience;
        assessment.SkillLevel = dto.SkillLevel;
        assessment.Height = dto.Height;
        assessment.Weight = dto.Weight;
        assessment.PhysicalAttributes = dto.PhysicalAttributes;
        assessment.Strengths = dto.Strengths;
        assessment.Weaknesses = dto.Weaknesses;
        assessment.RecommendedSports = dto.RecommendedSports;
        assessment.DevelopmentPlan = dto.DevelopmentPlan;
        assessment.ReportUrl = dto.ReportUrl;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetAssessmentByIdAsync(id))!;
    }

    public async Task<bool> DeleteAssessmentAsync(int id)
    {
        var assessment = await _context.SportsAssessments.FindAsync(id);
        if (assessment == null || assessment.IsDeleted)
            return false;

        assessment.IsDeleted = true;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SportsAssessmentStatisticsDto> GetStatisticsAsync()
    {
        var assessments = await _context.SportsAssessments
            .Include(a => a.Coach).ThenInclude(c => c.User)
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstDayOfYear = new DateTime(now.Year, 1, 1);

        var stats = new SportsAssessmentStatisticsDto
        {
            TotalAssessments = assessments.Count,
            AssessmentsThisMonth = assessments.Count(a => a.AssessmentDate >= firstDayOfMonth),
            AssessmentsThisYear = assessments.Count(a => a.AssessmentDate >= firstDayOfYear)
        };

        stats.AssessmentsBySport = assessments
            .Where(a => !string.IsNullOrEmpty(a.CurrentSport))
            .GroupBy(a => a.CurrentSport!)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.AssessmentsBySkillLevel = assessments
            .Where(a => !string.IsNullOrEmpty(a.SkillLevel))
            .GroupBy(a => a.SkillLevel!)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.AssessmentsByCoach = assessments
            .GroupBy(a => $"{a.Coach.User.FirstName} {a.Coach.User.LastName}")
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    private SportsAssessmentDto MapToDto(SportsAssessment assessment)
    {
        return new SportsAssessmentDto
        {
            Id = assessment.Id,
            StudentId = assessment.StudentId,
            StudentName = $"{assessment.Student.User.FirstName} {assessment.Student.User.LastName}",
            StudentNo = assessment.Student.StudentNo,
            CoachId = assessment.CoachId,
            CoachName = $"{assessment.Coach.User.FirstName} {assessment.Coach.User.LastName}",
            AssessmentDate = assessment.AssessmentDate,
            CurrentSport = assessment.CurrentSport,
            YearsOfExperience = assessment.YearsOfExperience,
            SkillLevel = assessment.SkillLevel,
            Height = assessment.Height,
            Weight = assessment.Weight,
            PhysicalAttributes = assessment.PhysicalAttributes,
            Strengths = assessment.Strengths,
            Weaknesses = assessment.Weaknesses,
            RecommendedSports = assessment.RecommendedSports,
            DevelopmentPlan = assessment.DevelopmentPlan,
            ReportUrl = assessment.ReportUrl,
            CreatedDate = assessment.CreatedAt,
            LastModifiedDate = assessment.UpdatedAt
        };
    }
}
