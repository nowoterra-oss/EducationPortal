using EduPortal.Application.DTOs.Class;

namespace EduPortal.Application.Interfaces;

public interface IClassService
{
    Task<(IEnumerable<ClassSummaryDto> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, int? grade = null, string? academicYear = null);
    Task<ClassDto?> GetByIdAsync(int id);
    Task<ClassDto> CreateAsync(CreateClassDto dto);
    Task<ClassDto> UpdateAsync(int id, UpdateClassDto dto);
    Task<bool> DeleteAsync(int id);

    // Students
    Task<(IEnumerable<ClassStudentDto> Items, int TotalCount)> GetStudentsPagedAsync(int classId, int pageNumber, int pageSize);

    // Statistics
    Task<ClassStatisticsDto?> GetStatisticsAsync(int classId);

    // Additional queries
    Task<IEnumerable<ClassDto>> GetByGradeAsync(int grade);
    Task<IEnumerable<ClassDto>> GetByAcademicYearAsync(string academicYear);
    Task<IEnumerable<ClassDto>> GetActiveClassesAsync();
}
