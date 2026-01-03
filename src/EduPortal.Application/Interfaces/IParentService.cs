using EduPortal.Application.DTOs.Parent;

namespace EduPortal.Application.Interfaces;

public interface IParentService
{
    Task<IEnumerable<ParentDto>> GetAllParentsAsync();
    Task<(IEnumerable<ParentSummaryDto> Items, int TotalCount)> GetParentsPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<ParentWithStudentDto> Items, int TotalCount)> GetParentsWithStudentPagedAsync(int pageNumber, int pageSize);
    Task<ParentDto?> GetParentByIdAsync(int id);
    Task<ParentWithStudentDto?> GetParentWithStudentByIdAsync(int id);
    Task<ParentDto?> GetParentByUserIdAsync(string userId);
    Task<ParentDto> CreateParentAsync(CreateParentDto dto);
    Task<ParentDto> UpdateParentAsync(int id, UpdateParentDto dto);
    Task<bool> DeleteParentAsync(int id);
    Task<IEnumerable<ParentDto>> GetParentsByStudentIdAsync(int studentId);
    Task<bool> AddStudentRelationshipAsync(int parentId, StudentRelationshipDto relationship);
    Task<bool> RemoveStudentRelationshipAsync(int parentId, int studentId);

    /// <summary>
    /// Search parents by name, email
    /// </summary>
    Task<(IEnumerable<ParentWithStudentDto> Items, int TotalCount)> SearchAsync(string term, int pageNumber, int pageSize);

    /// <summary>
    /// Create a parent for a specific student (creates ApplicationUser and Parent together)
    /// </summary>
    Task<ParentDto> CreateParentForStudentAsync(int studentId, CreateParentForStudentDto dto);
}
