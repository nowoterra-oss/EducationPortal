using EduPortal.Application.DTOs.StudentTeacherAssignment;

namespace EduPortal.Application.Interfaces;

public interface IStudentTeacherAssignmentService
{
    Task<IEnumerable<StudentTeacherAssignmentDto>> GetAllAsync();
    Task<StudentTeacherAssignmentDto?> GetByIdAsync(int id);
    Task<IEnumerable<StudentTeacherAssignmentDto>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<StudentTeacherAssignmentDto>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<StudentTeacherAssignmentDto>> GetByCourseIdAsync(int courseId);
    Task<StudentTeacherAssignmentDto> CreateAsync(CreateStudentTeacherAssignmentDto dto);
    Task<IEnumerable<StudentTeacherAssignmentDto>> CreateBulkAsync(List<CreateStudentTeacherAssignmentDto> dtos);
    Task<StudentTeacherAssignmentDto> UpdateAsync(int id, UpdateStudentTeacherAssignmentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeactivateAsync(int id);
}
