using EduPortal.Application.DTOs.SportsAssessment;

namespace EduPortal.Application.Interfaces;

public interface ISportsAssessmentService
{
    Task<IEnumerable<SportsAssessmentDto>> GetAllAssessmentsAsync();
    Task<IEnumerable<SportsAssessmentDto>> GetAssessmentsByStudentAsync(int studentId);
    Task<IEnumerable<SportsAssessmentDto>> GetAssessmentsByCounselorAsync(int counselorId);
    Task<IEnumerable<SportsAssessmentSummaryDto>> GetAssessmentSummariesAsync();
    Task<SportsAssessmentDto?> GetAssessmentByIdAsync(int id);
    Task<SportsAssessmentDto> CreateAssessmentAsync(CreateSportsAssessmentDto dto);
    Task<SportsAssessmentDto> UpdateAssessmentAsync(int id, UpdateSportsAssessmentDto dto);
    Task<bool> DeleteAssessmentAsync(int id);
    Task<SportsAssessmentStatisticsDto> GetStatisticsAsync();
}
