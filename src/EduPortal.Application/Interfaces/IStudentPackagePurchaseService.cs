using EduPortal.Application.DTOs.PackagePurchase;

namespace EduPortal.Application.Interfaces;

public interface IStudentPackagePurchaseService
{
    Task<IEnumerable<StudentPackagePurchaseDto>> GetAllPurchasesAsync();
    Task<IEnumerable<StudentPackagePurchaseDto>> GetActivePurchasesAsync();
    Task<IEnumerable<StudentPackagePurchaseDto>> GetPurchasesByStudentAsync(int studentId);
    Task<IEnumerable<StudentPackagePurchaseDto>> GetPurchasesByPackageAsync(int packageId);
    Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummariesAsync();
    Task<StudentPackagePurchaseDto?> GetPurchaseByIdAsync(int id);
    Task<StudentPackagePurchaseDto> CreatePurchaseAsync(CreateStudentPackagePurchaseDto dto);
    Task<StudentPackagePurchaseDto> UpdatePurchaseAsync(int id, UpdateStudentPackagePurchaseDto dto);
    Task<bool> DeletePurchaseAsync(int id);
    Task<bool> UseSessionAsync(int purchaseId);
    Task<PurchaseStatisticsDto> GetStatisticsAsync();
}
