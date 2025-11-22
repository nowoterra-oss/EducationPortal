using EduPortal.Application.DTOs.Package;

namespace EduPortal.Application.Interfaces;

public interface IServicePackageService
{
    Task<IEnumerable<ServicePackageDto>> GetAllPackagesAsync();
    Task<IEnumerable<ServicePackageDto>> GetActivePackagesAsync();
    Task<IEnumerable<ServicePackageSummaryDto>> GetPackageSummariesAsync();
    Task<ServicePackageDto?> GetPackageByIdAsync(int id);
    Task<ServicePackageDto> CreatePackageAsync(CreateServicePackageDto dto);
    Task<ServicePackageDto> UpdatePackageAsync(int id, UpdateServicePackageDto dto);
    Task<bool> DeletePackageAsync(int id);
    Task<PackageStatisticsDto> GetStatisticsAsync();
}
