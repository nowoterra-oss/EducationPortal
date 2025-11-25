using EduPortal.Application.DTOs.AcademicTerm;

namespace EduPortal.Application.Interfaces;

public interface IAcademicTermService
{
    Task<(IEnumerable<AcademicTermDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<AcademicTermDto?> GetByIdAsync(int id);
    Task<AcademicTermDto> CreateAsync(CreateAcademicTermDto dto);
    Task<AcademicTermDto> UpdateAsync(int id, UpdateAcademicTermDto dto);
    Task<bool> DeleteAsync(int id);
    Task<AcademicTermDto?> GetCurrentAsync();
    Task<AcademicTermDto> ActivateAsync(int id);
}
