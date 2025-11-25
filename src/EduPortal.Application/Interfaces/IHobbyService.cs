using EduPortal.Application.DTOs.Hobby;

namespace EduPortal.Application.Interfaces;

public interface IHobbyService
{
    Task<IEnumerable<HobbyDto>> GetAllAsync();
    Task<HobbyDto?> GetByIdAsync(int id);
    Task<HobbyDto> CreateAsync(CreateHobbyDto dto);
    Task<HobbyDto> UpdateAsync(int id, UpdateHobbyDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<HobbyDto>> GetByStudentAsync(int studentId);
    Task<IEnumerable<HobbyDto>> GetByCategoryAsync(string category);
}
