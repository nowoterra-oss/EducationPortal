using EduPortal.Application.DTOs.SchoolRecommendation;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class SchoolRecommendationService : ISchoolRecommendationService
{
    private readonly ApplicationDbContext _context;

    public SchoolRecommendationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SchoolRecommendationDto>> GetAllRecommendationsAsync()
    {
        var recommendations = await _context.SchoolRecommendations
            .Include(r => r.Student).ThenInclude(s => s.User)
            .Include(r => r.Coach).ThenInclude(c => c.User)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.RankingScore)
            .ToListAsync();

        return recommendations.Select(MapToDto);
    }

    public async Task<IEnumerable<SchoolRecommendationDto>> GetRecommendationsByStudentAsync(int studentId)
    {
        var recommendations = await _context.SchoolRecommendations
            .Include(r => r.Student).ThenInclude(s => s.User)
            .Include(r => r.Coach).ThenInclude(c => c.User)
            .Where(r => r.StudentId == studentId && !r.IsDeleted)
            .OrderByDescending(r => r.RankingScore)
            .ToListAsync();

        return recommendations.Select(MapToDto);
    }

    public async Task<IEnumerable<SchoolRecommendationDto>> GetRecommendationsByCoachAsync(int coachId)
    {
        var recommendations = await _context.SchoolRecommendations
            .Include(r => r.Student).ThenInclude(s => s.User)
            .Include(r => r.Coach).ThenInclude(c => c.User)
            .Where(r => r.CoachId == coachId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return recommendations.Select(MapToDto);
    }

    public async Task<IEnumerable<SchoolRecommendationSummaryDto>> GetRecommendationSummariesAsync()
    {
        var recommendations = await _context.SchoolRecommendations
            .Include(r => r.Student).ThenInclude(s => s.User)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.RankingScore)
            .ToListAsync();

        return recommendations.Select(r => new SchoolRecommendationSummaryDto
        {
            Id = r.Id,
            StudentName = $"{r.Student.User.FirstName} {r.Student.User.LastName}",
            SchoolName = r.SchoolName,
            SchoolLevel = r.SchoolLevel.ToString(),
            Status = r.Status.ToString(),
            RankingScore = r.RankingScore
        });
    }

    public async Task<SchoolRecommendationDto?> GetRecommendationByIdAsync(int id)
    {
        var recommendation = await _context.SchoolRecommendations
            .Include(r => r.Student).ThenInclude(s => s.User)
            .Include(r => r.Coach).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return recommendation != null ? MapToDto(recommendation) : null;
    }

    public async Task<SchoolRecommendationDto> CreateRecommendationAsync(CreateSchoolRecommendationDto dto)
    {
        var recommendation = new SchoolRecommendation
        {
            StudentId = dto.StudentId,
            CoachId = dto.CoachId,
            SchoolName = dto.SchoolName,
            SchoolLevel = (SchoolLevel)dto.SchoolLevel,
            SchoolType = (SchoolType)dto.SchoolType,
            City = dto.City,
            District = dto.District,
            Reasoning = dto.Reasoning,
            RankingScore = dto.RankingScore,
            Status = RecommendationStatus.Pending,
            Notes = dto.Notes
        };

        _context.SchoolRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        return (await GetRecommendationByIdAsync(recommendation.Id))!;
    }

    public async Task<SchoolRecommendationDto> UpdateRecommendationAsync(int id, UpdateSchoolRecommendationDto dto)
    {
        var recommendation = await _context.SchoolRecommendations.FindAsync(id);
        if (recommendation == null || recommendation.IsDeleted)
            throw new Exception("School recommendation not found");

        recommendation.SchoolName = dto.SchoolName;
        recommendation.SchoolLevel = (SchoolLevel)dto.SchoolLevel;
        recommendation.SchoolType = (SchoolType)dto.SchoolType;
        recommendation.City = dto.City;
        recommendation.District = dto.District;
        recommendation.Status = (RecommendationStatus)dto.Status;
        recommendation.Reasoning = dto.Reasoning;
        recommendation.RankingScore = dto.RankingScore;
        recommendation.Notes = dto.Notes;
        recommendation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetRecommendationByIdAsync(id))!;
    }

    public async Task<bool> DeleteRecommendationAsync(int id)
    {
        var recommendation = await _context.SchoolRecommendations.FindAsync(id);
        if (recommendation == null || recommendation.IsDeleted)
            return false;

        recommendation.IsDeleted = true;
        recommendation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SchoolRecommendationStatisticsDto> GetStatisticsAsync()
    {
        var recommendations = await _context.SchoolRecommendations
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        var stats = new SchoolRecommendationStatisticsDto
        {
            TotalRecommendations = recommendations.Count,
            AcceptedRecommendations = recommendations.Count(r => r.Status == RecommendationStatus.Accepted),
            PendingRecommendations = recommendations.Count(r => r.Status == RecommendationStatus.Pending),
            RejectedRecommendations = recommendations.Count(r => r.Status == RecommendationStatus.Rejected)
        };

        stats.RecommendationsByLevel = recommendations
            .GroupBy(r => r.SchoolLevel.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.RecommendationsByType = recommendations
            .GroupBy(r => r.SchoolType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.RecommendationsByCity = recommendations
            .Where(r => !string.IsNullOrEmpty(r.City))
            .GroupBy(r => r.City!)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.RecommendationsByStatus = recommendations
            .GroupBy(r => r.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.AverageRankingScore = recommendations.Any() && recommendations.Any(r => r.RankingScore.HasValue)
            ? (decimal)recommendations.Where(r => r.RankingScore.HasValue).Average(r => r.RankingScore!.Value)
            : 0;

        return stats;
    }

    private SchoolRecommendationDto MapToDto(SchoolRecommendation recommendation)
    {
        return new SchoolRecommendationDto
        {
            Id = recommendation.Id,
            StudentId = recommendation.StudentId,
            StudentName = $"{recommendation.Student.User.FirstName} {recommendation.Student.User.LastName}",
            StudentNo = recommendation.Student.StudentNo,
            CoachId = recommendation.CoachId,
            CoachName = $"{recommendation.Coach.User.FirstName} {recommendation.Coach.User.LastName}",
            SchoolName = recommendation.SchoolName,
            SchoolLevel = recommendation.SchoolLevel.ToString(),
            SchoolType = recommendation.SchoolType.ToString(),
            City = recommendation.City,
            District = recommendation.District,
            Status = recommendation.Status.ToString(),
            Reasoning = recommendation.Reasoning,
            RankingScore = recommendation.RankingScore,
            Notes = recommendation.Notes,
            CreatedDate = recommendation.CreatedAt,
            LastModifiedDate = recommendation.UpdatedAt
        };
    }
}
