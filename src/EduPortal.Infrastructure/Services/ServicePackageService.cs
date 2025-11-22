using EduPortal.Application.DTOs.Package;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ServicePackageService : IServicePackageService
{
    private readonly ApplicationDbContext _context;

    public ServicePackageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServicePackageDto>> GetAllPackagesAsync()
    {
        var packages = await _context.ServicePackages
            .Include(p => p.Purchases)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Type).ThenBy(p => p.Price)
            .ToListAsync();

        return packages.Select(MapToDto);
    }

    public async Task<IEnumerable<ServicePackageDto>> GetActivePackagesAsync()
    {
        var packages = await _context.ServicePackages
            .Include(p => p.Purchases)
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Type).ThenBy(p => p.Price)
            .ToListAsync();

        return packages.Select(MapToDto);
    }

    public async Task<IEnumerable<ServicePackageSummaryDto>> GetPackageSummariesAsync()
    {
        var packages = await _context.ServicePackages
            .Include(p => p.Purchases)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return packages.Select(p => new ServicePackageSummaryDto
        {
            Id = p.Id,
            PackageName = p.PackageName,
            PackageType = p.Type.ToString(),
            Price = p.Price,
            SessionCount = p.SessionCount,
            IsActive = p.IsActive,
            TotalPurchases = p.Purchases.Count(pur => !pur.IsDeleted)
        });
    }

    public async Task<ServicePackageDto?> GetPackageByIdAsync(int id)
    {
        var package = await _context.ServicePackages
            .Include(p => p.Purchases)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return package != null ? MapToDto(package) : null;
    }

    public async Task<ServicePackageDto> CreatePackageAsync(CreateServicePackageDto dto)
    {
        var package = new ServicePackage
        {
            PackageName = dto.PackageName,
            Type = (PackageType)dto.Type,
            Description = dto.Description,
            Price = dto.Price,
            SessionCount = dto.SessionCount,
            ValidityMonths = dto.ValidityMonths,
            Includes = dto.Includes,
            IsActive = dto.IsActive
        };

        _context.ServicePackages.Add(package);
        await _context.SaveChangesAsync();

        return (await GetPackageByIdAsync(package.Id))!;
    }

    public async Task<ServicePackageDto> UpdatePackageAsync(int id, UpdateServicePackageDto dto)
    {
        var package = await _context.ServicePackages.FindAsync(id);
        if (package == null || package.IsDeleted)
            throw new Exception("Service package not found");

        package.PackageName = dto.PackageName;
        package.Type = (PackageType)dto.Type;
        package.Description = dto.Description;
        package.Price = dto.Price;
        package.SessionCount = dto.SessionCount;
        package.ValidityMonths = dto.ValidityMonths;
        package.Includes = dto.Includes;
        package.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return (await GetPackageByIdAsync(id))!;
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        var package = await _context.ServicePackages.FindAsync(id);
        if (package == null || package.IsDeleted)
            return false;

        // Check if package has active purchases
        var hasActivePurchases = await _context.StudentPackagePurchases
            .AnyAsync(p => p.PackageId == id && !p.IsDeleted && p.IsActive);

        if (hasActivePurchases)
            throw new Exception("Cannot delete package with active purchases");

        package.IsDeleted = true;
        package.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PackageStatisticsDto> GetStatisticsAsync()
    {
        var packages = await _context.ServicePackages
            .Include(p => p.Purchases)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var allPurchases = await _context.StudentPackagePurchases
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        var stats = new PackageStatisticsDto
        {
            TotalPackages = packages.Count,
            ActivePackages = packages.Count(p => p.IsActive),
            TotalPurchases = allPurchases.Count,
            ActivePurchases = allPurchases.Count(p => p.IsActive)
        };

        stats.PackagesByType = packages
            .GroupBy(p => p.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.PurchasesByPackage = packages
            .ToDictionary(
                p => p.PackageName,
                p => p.Purchases.Count(pur => !pur.IsDeleted)
            );

        stats.TotalRevenue = allPurchases.Sum(p => p.AmountPaid);
        stats.MonthlyRevenue = allPurchases
            .Where(p => p.PurchaseDate >= firstDayOfMonth)
            .Sum(p => p.AmountPaid);
        stats.AveragePackagePrice = packages.Any() ? packages.Average(p => p.Price) : 0;

        // Top selling packages
        stats.TopSellingPackages = packages
            .Select(p => new TopSellingPackageDto
            {
                PackageId = p.Id,
                PackageName = p.PackageName,
                PurchaseCount = p.Purchases.Count(pur => !pur.IsDeleted),
                Revenue = p.Purchases.Where(pur => !pur.IsDeleted).Sum(pur => pur.AmountPaid)
            })
            .OrderByDescending(p => p.PurchaseCount)
            .Take(5)
            .ToList();

        return stats;
    }

    private ServicePackageDto MapToDto(ServicePackage package)
    {
        var purchases = package.Purchases.Where(p => !p.IsDeleted).ToList();
        var activePurchases = purchases.Count(p => p.IsActive);
        var totalRevenue = purchases.Sum(p => p.AmountPaid);

        return new ServicePackageDto
        {
            Id = package.Id,
            PackageName = package.PackageName,
            PackageType = package.Type.ToString(),
            Description = package.Description,
            Price = package.Price,
            SessionCount = package.SessionCount,
            ValidityMonths = package.ValidityMonths,
            Includes = package.Includes,
            IsActive = package.IsActive,
            TotalPurchases = purchases.Count,
            ActivePurchases = activePurchases,
            TotalRevenue = totalRevenue,
            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt
        };
    }
}
