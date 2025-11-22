using EduPortal.Application.DTOs.Assessment;

namespace EduPortal.Application.Interfaces;

public interface ICareerAssessmentService
{
    Task<IEnumerable<CareerAssessmentDto>> GetAllAssessmentsAsync();
    Task<IEnumerable<CareerAssessmentDto>> GetAssessmentsByStudentAsync(int studentId);
    Task<IEnumerable<CareerAssessmentDto>> GetAssessmentsByCoachAsync(int coachId);
    Task<IEnumerable<CareerAssessmentSummaryDto>> GetAssessmentSummariesAsync();
    Task<CareerAssessmentDto?> GetAssessmentByIdAsync(int id);
    Task<CareerAssessmentDto> CreateAssessmentAsync(CreateCareerAssessmentDto dto);
    Task<CareerAssessmentDto> UpdateAssessmentAsync(int id, UpdateCareerAssessmentDto dto);
    Task<bool> DeleteAssessmentAsync(int id);
    Task<CareerAssessmentStatisticsDto> GetStatisticsAsync();
}
