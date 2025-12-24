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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roles = await _userManager.GetRolesAsync(user);
        return roles.Contains("Danisman");
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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roles = await _userManager.GetRolesAsync(user);

        // Admin her zaman erişebilir
        if (roles.Contains("Admin")) return true;

        // Danışman değilse (örn: normal Öğretmen veya diğer roller) erişebilir
        if (!roles.Contains("Danisman")) return true;

        // Danışman ise sadece atanmış öğrencilerine erişebilir
        var teacherId = await GetAdvisorTeacherIdAsync(userId);
        if (!teacherId.HasValue) return false;

        var assignedStudentIds = await GetAssignedStudentIdsAsync(teacherId.Value);
        return assignedStudentIds.Contains(studentId);
    }

    public async Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roles = await _userManager.GetRolesAsync(user);

        // Admin her zaman erişebilir
        if (roles.Contains("Admin")) return true;

        // Danışman değilse erişebilir
        if (!roles.Contains("Danisman")) return true;

        // Danışman ise tüm öğrencilerin atanmış olması gerekir
        var teacherId = await GetAdvisorTeacherIdAsync(userId);
        if (!teacherId.HasValue) return false;

        var assignedStudentIds = await GetAssignedStudentIdsAsync(teacherId.Value);
        return studentIds.All(id => assignedStudentIds.Contains(id));
    }
}
