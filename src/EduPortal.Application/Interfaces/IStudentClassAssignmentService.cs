using EduPortal.Application.DTOs.StudentClassAssignment;

namespace EduPortal.Application.Interfaces;

public interface IStudentClassAssignmentService
{
    Task<(IEnumerable<StudentClassAssignmentDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, bool? isActive = null);
    Task<StudentClassAssignmentDto?> GetByIdAsync(int id);
    Task<StudentClassAssignmentDto> CreateAsync(CreateStudentClassAssignmentDto dto);
    Task<IEnumerable<StudentClassAssignmentDto>> CreateBulkAsync(IEnumerable<CreateStudentClassAssignmentDto> dtos);
    Task<StudentClassAssignmentDto> UpdateAsync(int id, UpdateStudentClassAssignmentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<StudentClassAssignmentDto>> GetByStudentAsync(int studentId);
    Task<(IEnumerable<StudentClassAssignmentDto> Items, int TotalCount)> GetByClassAsync(int classId, int pageNumber, int pageSize);
}
