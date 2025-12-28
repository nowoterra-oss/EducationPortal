using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

/// <summary>
/// Danışman erişim kontrolü servisi implementasyonu.
/// Danışmanların sadece kendilerine atanmış öğrencilerin verilerine erişmesini sağlar.
/// </summary>
public class AdvisorAccessService : IAdvisorAccessService
{
    private readonly ApplicationDbContext _context;
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdvisorAccessService(
        ApplicationDbContext context,
        ITeacherRepository teacherRepository,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _teacherRepository = teacherRepository;
        _userManager = userManager;
    }

    public async Task<bool> IsAdvisorAsync(string userId)
    {
        // Danışman sistemi kaldırıldı - artık kimse danışman olarak değerlendirilmiyor
        await Task.CompletedTask; // async imza için
        return false;
    }

    public async Task<int?> GetAdvisorTeacherIdAsync(string userId)
    {
        var teacher = await _teacherRepository.GetByUserIdAsync(userId);
        return teacher?.Id;
    }

    public async Task<List<int>> GetAssignedStudentIdsAsync(int teacherId)
    {
        return await _context.StudentTeacherAssignments
            .Where(sta => sta.TeacherId == teacherId && sta.IsActive && !sta.IsDeleted)
            .Select(sta => sta.StudentId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> CanAccessStudentAsync(string userId, int studentId)
    {
        // Danışman sistemi kaldırıldı - tüm kullanıcılar tüm öğrencilere erişebilir
        await Task.CompletedTask; // async imza için
        return true;
    }

    public async Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds)
    {
        // Danışman sistemi kaldırıldı - tüm kullanıcılar tüm öğrencilere erişebilir
        await Task.CompletedTask; // async imza için
        return true;
    }
}
