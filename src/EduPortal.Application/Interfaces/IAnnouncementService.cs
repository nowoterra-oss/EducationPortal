using EduPortal.Application.DTOs.Announcement;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IAnnouncementService
{
    Task<(IEnumerable<AnnouncementDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, AnnouncementType? type = null);
    Task<AnnouncementDto?> GetByIdAsync(int id);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto dto, string userId);
    Task<AnnouncementDto> UpdateAsync(int id, UpdateAnnouncementDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<AnnouncementDto> Items, int TotalCount)> GetActiveAsync(int pageNumber, int pageSize);
    Task<IEnumerable<AnnouncementDto>> GetPinnedAsync();
    Task<AnnouncementDto> PinAsync(int id);
    Task<AnnouncementDto> UnpinAsync(int id);
}
