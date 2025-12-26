using EduPortal.Application.DTOs.StudyAbroad;

namespace EduPortal.Application.Interfaces;

public interface IStudyAbroadService
{
    Task<IEnumerable<StudyAbroadProgramDto>> GetAllProgramsAsync();
    Task<IEnumerable<StudyAbroadProgramDto>> GetActiveProgramsAsync();
    Task<IEnumerable<StudyAbroadProgramDto>> GetProgramsByStudentAsync(int studentId);
    Task<IEnumerable<StudyAbroadProgramDto>> GetProgramsByCounselorAsync(int counselorId);
    Task<IEnumerable<ProgramSummaryDto>> GetProgramSummariesAsync();
    Task<StudyAbroadProgramDto?> GetProgramByIdAsync(int id);
    Task<StudyAbroadProgramDto> CreateProgramAsync(CreateStudyAbroadProgramDto dto);
    Task<StudyAbroadProgramDto> UpdateProgramAsync(int id, UpdateStudyAbroadProgramDto dto);
    Task<bool> DeleteProgramAsync(int id);
    Task<ProgramStatisticsDto> GetStatisticsAsync();
}
