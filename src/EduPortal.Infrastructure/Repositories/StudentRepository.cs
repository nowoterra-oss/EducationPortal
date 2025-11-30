using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Repositories;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.User.LastName)
            .ThenBy(s => s.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Student?> GetByStudentNoAsync(string studentNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentNo == studentNo, cancellationToken);
    }

    public async Task<Student?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Student?> GetStudentWithDetailsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Parents)
            .Include(s => s.Siblings)
            .Include(s => s.Hobbies)
            .Include(s => s.Clubs)
            .Include(s => s.Competitions)
            .Include(s => s.InternationalExams)
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(int grade, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.CurrentGrade == grade)
            .OrderBy(s => s.User.LastName)
            .ThenBy(s => s.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s =>
                s.StudentNo.Contains(searchTerm) ||
                s.User.FirstName.Contains(searchTerm) ||
                s.User.LastName.Contains(searchTerm) ||
                s.User.Email!.Contains(searchTerm))
            .OrderBy(s => s.User.LastName)
            .ThenBy(s => s.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> StudentNoExistsAsync(string studentNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.StudentNo == studentNo, cancellationToken);
    }

    public async Task<int> GetLastStudentSequenceForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var yearPrefix = (year % 100).ToString("D2"); // 2025 -> "25"

        // Silinmiş öğrencileri de dahil et çünkü StudentNo unique index'e sahip
        var lastStudent = await _dbSet
            .IgnoreQueryFilters() // Soft delete filtresini atla
            .Where(s => s.StudentNo.StartsWith(yearPrefix))
            .OrderByDescending(s => s.StudentNo)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastStudent == null)
            return 0;

        // StudentNo formatı: "25001" -> sıra numarası 1
        if (lastStudent.StudentNo.Length >= 3)
        {
            var sequencePart = lastStudent.StudentNo.Substring(2);
            if (int.TryParse(sequencePart, out int sequence))
                return sequence;
        }

        return 0;
    }

    public async Task<bool> IdentityNumberExistsAsync(string identityNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.IdentityNumber == identityNumber && !s.IsDeleted, cancellationToken);
    }
}
