using EduPortal.Application.DTOs.PackagePurchase;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class StudentPackagePurchaseService : IStudentPackagePurchaseService
{
    private readonly ApplicationDbContext _context;

    public StudentPackagePurchaseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentPackagePurchaseDto>> GetAllPurchasesAsync()
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPackagePurchaseDto>> GetActivePurchasesAsync()
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPackagePurchaseDto>> GetPurchasesByStudentAsync(int studentId)
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .Where(p => p.StudentId == studentId && !p.IsDeleted)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPackagePurchaseDto>> GetPurchasesByPackageAsync(int packageId)
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .Where(p => p.PackageId == packageId && !p.IsDeleted)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(MapToDto);
    }

    public async Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummariesAsync()
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(p => new PurchaseSummaryDto
        {
            Id = p.Id,
            StudentName = $"{p.Student.User.FirstName} {p.Student.User.LastName}",
            PackageName = p.Package.PackageName,
            PurchaseDate = p.PurchaseDate,
            AmountPaid = p.AmountPaid,
            IsActive = p.IsActive,
            RemainingSessions = p.RemainingSessions
        });
    }

    public async Task<StudentPackagePurchaseDto?> GetPurchaseByIdAsync(int id)
    {
        var purchase = await _context.StudentPackagePurchases
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return purchase != null ? MapToDto(purchase) : null;
    }

    public async Task<StudentPackagePurchaseDto> CreatePurchaseAsync(CreateStudentPackagePurchaseDto dto)
    {
        var package = await _context.ServicePackages.FindAsync(dto.PackageId);
        if (package == null || package.IsDeleted)
            throw new Exception("Service package not found");

        // Calculate expiry date if not provided
        var expiryDate = dto.ExpiryDate;
        if (!expiryDate.HasValue && package.ValidityMonths.HasValue)
        {
            expiryDate = dto.PurchaseDate.AddMonths(package.ValidityMonths.Value);
        }

        // Set remaining sessions from DTO or package default
        var remainingSessions = dto.RemainingSessions ?? package.SessionCount ?? 0;

        var purchase = new StudentPackagePurchase
        {
            StudentId = dto.StudentId,
            PackageId = dto.PackageId,
            PurchaseDate = dto.PurchaseDate,
            AmountPaid = dto.AmountPaid,
            RemainingSessions = remainingSessions,
            ExpiryDate = expiryDate,
            IsActive = true
        };

        _context.StudentPackagePurchases.Add(purchase);
        await _context.SaveChangesAsync();

        return (await GetPurchaseByIdAsync(purchase.Id))!;
    }

    public async Task<StudentPackagePurchaseDto> UpdatePurchaseAsync(int id, UpdateStudentPackagePurchaseDto dto)
    {
        var purchase = await _context.StudentPackagePurchases.FindAsync(id);
        if (purchase == null || purchase.IsDeleted)
            throw new Exception("Package purchase not found");

        purchase.PurchaseDate = dto.PurchaseDate;
        purchase.AmountPaid = dto.AmountPaid;
        purchase.RemainingSessions = dto.RemainingSessions;
        purchase.ExpiryDate = dto.ExpiryDate;
        purchase.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return (await GetPurchaseByIdAsync(id))!;
    }

    public async Task<bool> DeletePurchaseAsync(int id)
    {
        var purchase = await _context.StudentPackagePurchases.FindAsync(id);
        if (purchase == null || purchase.IsDeleted)
            return false;

        purchase.IsDeleted = true;
        purchase.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UseSessionAsync(int purchaseId)
    {
        var purchase = await _context.StudentPackagePurchases.FindAsync(purchaseId);
        if (purchase == null || purchase.IsDeleted || !purchase.IsActive)
            return false;

        if (purchase.RemainingSessions <= 0)
            return false;

        purchase.RemainingSessions--;

        if (purchase.RemainingSessions == 0)
        {
            purchase.IsActive = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PurchaseStatisticsDto> GetStatisticsAsync()
    {
        var purchases = await _context.StudentPackagePurchases
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstDayOfYear = new DateTime(now.Year, 1, 1);

        var stats = new PurchaseStatisticsDto
        {
            TotalPurchases = purchases.Count,
            ActivePurchases = purchases.Count(p => p.IsActive),
            ExpiredPurchases = purchases.Count(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value < now),
            PurchasesThisMonth = purchases.Count(p => p.PurchaseDate >= firstDayOfMonth),
            PurchasesThisYear = purchases.Count(p => p.PurchaseDate >= firstDayOfYear)
        };

        stats.TotalRevenue = purchases.Sum(p => p.AmountPaid);
        stats.MonthlyRevenue = purchases
            .Where(p => p.PurchaseDate >= firstDayOfMonth)
            .Sum(p => p.AmountPaid);
        stats.YearlyRevenue = purchases
            .Where(p => p.PurchaseDate >= firstDayOfYear)
            .Sum(p => p.AmountPaid);
        stats.AveragePurchaseAmount = purchases.Any() ? purchases.Average(p => p.AmountPaid) : 0;

        stats.PurchasesByPackageType = purchases
            .GroupBy(p => p.Package.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        stats.RevenueByPackageType = purchases
            .GroupBy(p => p.Package.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));

        // Monthly revenue chart (last 12 months)
        var monthlyData = new List<MonthlyRevenueDto>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date;
            var monthStartFirstDay = new DateTime(monthStart.Year, monthStart.Month, 1);
            var monthEnd = monthStartFirstDay.AddMonths(1).AddDays(-1);

            var monthPurchases = purchases.Where(p =>
                p.PurchaseDate >= monthStartFirstDay && p.PurchaseDate <= monthEnd).ToList();

            monthlyData.Add(new MonthlyRevenueDto
            {
                Month = monthStartFirstDay.ToString("MMM"),
                Year = monthStartFirstDay.Year,
                Revenue = monthPurchases.Sum(p => p.AmountPaid),
                PurchaseCount = monthPurchases.Count
            });
        }
        stats.MonthlyRevenueChart = monthlyData;

        return stats;
    }

    private StudentPackagePurchaseDto MapToDto(StudentPackagePurchase purchase)
    {
        var now = DateTime.UtcNow;
        var isExpired = purchase.ExpiryDate.HasValue && purchase.ExpiryDate.Value < now;
        var daysRemaining = purchase.ExpiryDate.HasValue
            ? (int)(purchase.ExpiryDate.Value - now).TotalDays
            : (int?)null;

        var totalSessions = purchase.Package.SessionCount;
        var usedSessions = totalSessions.HasValue
            ? totalSessions.Value - purchase.RemainingSessions
            : 0;

        return new StudentPackagePurchaseDto
        {
            Id = purchase.Id,
            StudentId = purchase.StudentId,
            StudentName = $"{purchase.Student.User.FirstName} {purchase.Student.User.LastName}",
            StudentNo = purchase.Student.StudentNo,
            PackageId = purchase.PackageId,
            PackageName = purchase.Package.PackageName,
            PackageType = purchase.Package.Type.ToString(),
            PurchaseDate = purchase.PurchaseDate,
            AmountPaid = purchase.AmountPaid,
            ExpiryDate = purchase.ExpiryDate,
            TotalSessions = totalSessions,
            RemainingSessions = purchase.RemainingSessions,
            UsedSessions = usedSessions,
            IsActive = purchase.IsActive,
            IsExpired = isExpired,
            DaysRemaining = daysRemaining,
            CreatedAt = purchase.CreatedAt,
            UpdatedAt = purchase.UpdatedAt
        };
    }
}
