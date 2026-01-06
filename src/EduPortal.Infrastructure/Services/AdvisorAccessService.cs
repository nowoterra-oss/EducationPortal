using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

/// <summary>
/// Danışman erişim kontrolü servisi implementasyonu.
/// Öğretmenlerin (danışman olarak atanmış) sadece kendilerine atanmış öğrencilerin verilerine erişmesini sağlar.
/// StudentTeacherAssignment tablosundaki AssignmentType.Advisor atamaları kullanılır.
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
        // Kullanıcının öğretmen olup olmadığını ve danışman ataması olup olmadığını kontrol et
        var teacher = await _teacherRepository.GetByUserIdAsync(userId);
        if (teacher == null)
            return false;

        // Bu öğretmenin aktif danışman ataması var mı?
        return await _context.StudentTeacherAssignments
            .AnyAsync(sta => sta.TeacherId == teacher.Id &&
                             sta.AssignmentType == AssignmentType.Advisor &&
                             sta.IsActive &&
                             !sta.IsDeleted);
    }

    public async Task<int?> GetAdvisorTeacherIdAsync(string userId)
    {
        var teacher = await _teacherRepository.GetByUserIdAsync(userId);
        return teacher?.Id;
    }

    public async Task<List<int>> GetAssignedStudentIdsAsync(int teacherId)
    {
        return await _context.StudentTeacherAssignments
            .Where(sta => sta.TeacherId == teacherId &&
                          sta.AssignmentType == AssignmentType.Advisor &&
                          sta.IsActive &&
                          !sta.IsDeleted)
            .Select(sta => sta.StudentId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> CanAccessStudentAsync(string userId, int studentId)
    {
        // Kullanıcının öğretmen ID'sini al
        var teacherId = await GetAdvisorTeacherIdAsync(userId);
        if (teacherId == null)
            return false;

        // Bu öğretmenin bu öğrenciye danışman olarak atanıp atanmadığını kontrol et
        return await _context.StudentTeacherAssignments
            .AnyAsync(sta => sta.TeacherId == teacherId.Value &&
                             sta.StudentId == studentId &&
                             sta.AssignmentType == AssignmentType.Advisor &&
                             sta.IsActive &&
                             !sta.IsDeleted);
    }

    public async Task<bool> CanAccessStudentsAsync(string userId, IEnumerable<int> studentIds)
    {
        if (!studentIds.Any())
            return true;

        // Kullanıcının öğretmen ID'sini al
        var teacherId = await GetAdvisorTeacherIdAsync(userId);
        if (teacherId == null)
            return false;

        // Bu öğretmene danışman olarak atanmış öğrenci ID'lerini al
        var assignedStudentIds = await GetAssignedStudentIdsAsync(teacherId.Value);

        // İstenen tüm öğrencilerin atanmış olup olmadığını kontrol et
        return studentIds.All(id => assignedStudentIds.Contains(id));
    }
}
