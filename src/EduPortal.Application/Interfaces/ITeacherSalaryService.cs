using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Finance;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface ITeacherSalaryService
{
    Task<PagedResult<TeacherSalaryDto>> GetAllPagedAsync(int pageNumber, int pageSize, int? year = null, int? month = null, SalaryStatus? status = null);

    Task<TeacherSalaryDto?> GetByIdAsync(int id);

    Task<List<TeacherSalaryDto>> GetByTeacherAsync(int teacherId);

    Task<TeacherSalaryDto> CreateAsync(TeacherSalaryCreateDto dto);

    Task<List<TeacherSalaryDto>> CreateBulkAsync(TeacherSalaryBulkCreateDto dto);

    Task<TeacherSalaryDto> UpdateAsync(int id, TeacherSalaryCreateDto dto);

    Task<bool> DeleteAsync(int id);

    Task<TeacherSalaryDto> PaySalaryAsync(int id, TeacherSalaryPayDto dto);

    Task<List<TeacherSalaryDto>> GetPendingAsync();

    Task<List<TeacherSalaryDto>> GetOverdueAsync();

    Task<decimal> GetTotalPendingAsync();

    Task<decimal> GetTotalPaidForMonthAsync(int year, int month);
}
