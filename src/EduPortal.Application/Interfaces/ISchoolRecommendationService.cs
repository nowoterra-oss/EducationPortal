using EduPortal.Application.DTOs.SchoolRecommendation;

namespace EduPortal.Application.Interfaces;

public interface ISchoolRecommendationService
{
    Task<IEnumerable<SchoolRecommendationDto>> GetAllRecommendationsAsync();
    Task<IEnumerable<SchoolRecommendationDto>> GetRecommendationsByStudentAsync(int studentId);
    Task<IEnumerable<SchoolRecommendationDto>> GetRecommendationsByCoachAsync(int coachId);
    Task<IEnumerable<SchoolRecommendationSummaryDto>> GetRecommendationSummariesAsync();
    Task<SchoolRecommendationDto?> GetRecommendationByIdAsync(int id);
    Task<SchoolRecommendationDto> CreateRecommendationAsync(CreateSchoolRecommendationDto dto);
    Task<SchoolRecommendationDto> UpdateRecommendationAsync(int id, UpdateSchoolRecommendationDto dto);
    Task<bool> DeleteRecommendationAsync(int id);
    Task<SchoolRecommendationStatisticsDto> GetStatisticsAsync();
}
