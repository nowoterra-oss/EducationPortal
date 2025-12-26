using EduPortal.Application.DTOs.Assessment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CareerAssessmentService : ICareerAssessmentService
{
    private readonly ApplicationDbContext _context;

    public CareerAssessmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CareerAssessmentDto>> GetAllAssessmentsAsync()
    {
        var assessments = await _context.CareerAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<CareerAssessmentDto>> GetAssessmentsByStudentAsync(int studentId)
    {
        var assessments = await _context.CareerAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .Where(a => a.StudentId == studentId && !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<CareerAssessmentDto>> GetAssessmentsByCounselorAsync(int counselorId)
    {
        var assessments = await _context.CareerAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .Where(a => a.CounselorId == counselorId && !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(MapToDto);
    }

    public async Task<IEnumerable<CareerAssessmentSummaryDto>> GetAssessmentSummariesAsync()
    {
        var assessments = await _context.CareerAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();

        return assessments.Select(a => new CareerAssessmentSummaryDto
        {
            Id = a.Id,
            StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
            CounselorName = $"{a.Counselor.User.FirstName} {a.Counselor.User.LastName}",
            AssessmentDate = a.AssessmentDate,
            AssessmentType = a.AssessmentType,
            TopCareerSuggestions = a.RecommendedCareers?.Split(',').Select(s => s.Trim()).Take(3).ToList() ?? new List<string>()
        });
    }

    public async Task<CareerAssessmentDto?> GetAssessmentByIdAsync(int id)
    {
        var assessment = await _context.CareerAssessments
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return assessment != null ? MapToDto(assessment) : null;
    }

    public async Task<CareerAssessmentDto> CreateAssessmentAsync(CreateCareerAssessmentDto dto)
    {
        var assessment = new CareerAssessment
        {
            StudentId = dto.StudentId,
            CounselorId = dto.CounselorId,
            AssessmentDate = dto.AssessmentDate,
            AssessmentType = dto.AssessmentType,
            Results = dto.Results,
            Interpretation = dto.Interpretation,
            RecommendedCareers = dto.RecommendedCareers,
            RecommendedFields = dto.RecommendedFields,
            ReportUrl = dto.ReportUrl
        };

        _context.CareerAssessments.Add(assessment);
        await _context.SaveChangesAsync();

        return (await GetAssessmentByIdAsync(assessment.Id))!;
    }

    public async Task<CareerAssessmentDto> UpdateAssessmentAsync(int id, UpdateCareerAssessmentDto dto)
    {
        var assessment = await _context.CareerAssessments.FindAsync(id);
        if (assessment == null || assessment.IsDeleted)
            throw new Exception("Career assessment not found");

        assessment.AssessmentDate = dto.AssessmentDate;
        assessment.AssessmentType = dto.AssessmentType;
        assessment.Results = dto.Results;
        assessment.Interpretation = dto.Interpretation;
        assessment.RecommendedCareers = dto.RecommendedCareers;
        assessment.RecommendedFields = dto.RecommendedFields;
        assessment.ReportUrl = dto.ReportUrl;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetAssessmentByIdAsync(id))!;
    }

    public async Task<bool> DeleteAssessmentAsync(int id)
    {
        var assessment = await _context.CareerAssessments.FindAsync(id);
        if (assessment == null || assessment.IsDeleted)
            return false;

        assessment.IsDeleted = true;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CareerAssessmentStatisticsDto> GetStatisticsAsync()
    {
        var assessments = await _context.CareerAssessments
            .Include(a => a.Counselor).ThenInclude(c => c.User)
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstDayOfYear = new DateTime(now.Year, 1, 1);

        var stats = new CareerAssessmentStatisticsDto
        {
            TotalAssessments = assessments.Count,
            AssessmentsThisMonth = assessments.Count(a => a.AssessmentDate >= firstDayOfMonth),
            AssessmentsThisYear = assessments.Count(a => a.AssessmentDate >= firstDayOfYear)
        };

        stats.AssessmentsByType = assessments
            .GroupBy(a => a.AssessmentType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Top career fields from recommended fields
        var allFields = assessments
            .Where(a => !string.IsNullOrEmpty(a.RecommendedFields))
            .SelectMany(a => a.RecommendedFields!.Split(',').Select(s => s.Trim()))
            .GroupBy(s => s)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
        stats.TopCareerFields = allFields;

        stats.AssessmentsByCounselor = assessments
            .GroupBy(a => $"{a.Counselor.User.FirstName} {a.Counselor.User.LastName}")
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    private CareerAssessmentDto MapToDto(CareerAssessment assessment)
    {
        return new CareerAssessmentDto
        {
            Id = assessment.Id,
            StudentId = assessment.StudentId,
            StudentName = $"{assessment.Student.User.FirstName} {assessment.Student.User.LastName}",
            StudentNo = assessment.Student.StudentNo,
            CounselorId = assessment.CounselorId,
            CounselorName = $"{assessment.Counselor.User.FirstName} {assessment.Counselor.User.LastName}",
            AssessmentDate = assessment.AssessmentDate,
            AssessmentType = assessment.AssessmentType,
            Results = assessment.Results,
            Interpretation = assessment.Interpretation,
            RecommendedCareers = assessment.RecommendedCareers,
            RecommendedFields = assessment.RecommendedFields,
            ReportUrl = assessment.ReportUrl,
            CreatedDate = assessment.CreatedAt,
            LastModifiedDate = assessment.UpdatedAt
        };
    }
}
