using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentGroup;

namespace EduPortal.Application.Services.Interfaces;

public interface IStudentGroupService
{
    // Grup CRUD
    Task<ApiResponse<PagedResponse<StudentGroupDto>>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false);
    Task<ApiResponse<StudentGroupDto>> GetByIdAsync(int id);
    Task<ApiResponse<StudentGroupDto>> CreateAsync(CreateStudentGroupDto dto);
    Task<ApiResponse<StudentGroupDto>> UpdateAsync(int id, UpdateStudentGroupDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);

    // Uye Yonetimi
    Task<ApiResponse<StudentGroupDto>> AddStudentsAsync(int groupId, AddStudentsToGroupDto dto);
    Task<ApiResponse<StudentGroupDto>> RemoveStudentsAsync(int groupId, RemoveStudentsFromGroupDto dto);
    Task<ApiResponse<List<StudentGroupDto>>> GetStudentGroupsAsync(int studentId);

    // Grup Dersleri
    Task<ApiResponse<GroupLessonScheduleDto>> CreateGroupLessonAsync(CreateGroupLessonDto dto);
    Task<ApiResponse<bool>> CancelGroupLessonAsync(int lessonId, bool cancelAll = true, DateTime? cancelDate = null);
    Task<ApiResponse<List<GroupLessonScheduleDto>>> GetGroupLessonsAsync(int groupId);
    Task<ApiResponse<List<GroupLessonScheduleDto>>> GetTeacherGroupLessonsAsync(int teacherId);

    // Cakisma Kontrolu
    Task<ApiResponse<GroupLessonConflictCheckResult>> CheckGroupLessonConflictsAsync(CreateGroupLessonDto dto);
}
