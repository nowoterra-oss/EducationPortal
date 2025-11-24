using EduPortal.Application.DTOs.Class;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ClassService : IClassService
{
    private readonly ApplicationDbContext _context;

    public ClassService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ClassSummaryDto> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, int? grade = null, string? academicYear = null)
    {
        var query = _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
            .Where(c => !c.IsDeleted);

        if (grade.HasValue)
            query = query.Where(c => c.Grade == grade.Value);

        if (!string.IsNullOrEmpty(academicYear))
            query = query.Where(c => c.AcademicYear == academicYear);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Branch)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(c => new ClassSummaryDto
        {
            Id = c.Id,
            ClassName = c.ClassName,
            Grade = c.Grade,
            Branch = c.Branch,
            ClassTeacherName = c.ClassTeacher != null
                ? $"{c.ClassTeacher.User.FirstName} {c.ClassTeacher.User.LastName}"
                : null,
            CurrentStudentCount = c.Students.Count,
            Capacity = c.Capacity,
            AcademicYear = c.AcademicYear,
            IsActive = c.IsActive
        });

        return (dtos, totalCount);
    }

    public async Task<ClassDto?> GetByIdAsync(int id)
    {
        var classEntity = await _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.BranchLocation)
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return classEntity != null ? MapToDto(classEntity) : null;
    }

    public async Task<ClassDto> CreateAsync(CreateClassDto dto)
    {
        var classEntity = new Class
        {
            ClassName = dto.ClassName,
            Grade = dto.Grade,
            Branch = dto.Branch,
            ClassTeacherId = dto.ClassTeacherId,
            Capacity = dto.Capacity,
            AcademicYear = dto.AcademicYear,
            BranchId = dto.BranchId,
            IsActive = dto.IsActive
        };

        _context.Classes.Add(classEntity);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(classEntity.Id))!;
    }

    public async Task<ClassDto> UpdateAsync(int id, UpdateClassDto dto)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity == null || classEntity.IsDeleted)
            throw new KeyNotFoundException("Sınıf bulunamadı");

        classEntity.ClassName = dto.ClassName;
        classEntity.Grade = dto.Grade;
        classEntity.Branch = dto.Branch;
        classEntity.ClassTeacherId = dto.ClassTeacherId;
        classEntity.Capacity = dto.Capacity;
        classEntity.AcademicYear = dto.AcademicYear;
        classEntity.BranchId = dto.BranchId;
        classEntity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity == null || classEntity.IsDeleted)
            return false;

        classEntity.IsDeleted = true;
        classEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(IEnumerable<ClassStudentDto> Items, int TotalCount)> GetStudentsPagedAsync(
        int classId, int pageNumber, int pageSize)
    {
        var query = _context.StudentClassAssignments
            .Include(sca => sca.Student)
                .ThenInclude(s => s.User)
            .Where(sca => sca.ClassId == classId && !sca.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(sca => sca.Student.User.LastName)
            .ThenBy(sca => sca.Student.User.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(sca => new ClassStudentDto
        {
            StudentId = sca.StudentId,
            StudentNo = sca.Student.StudentNo,
            StudentName = $"{sca.Student.User.FirstName} {sca.Student.User.LastName}",
            Email = sca.Student.User.Email,
            AssignmentDate = sca.AssignmentDate,
            IsActive = sca.IsActive
        });

        return (dtos, totalCount);
    }

    public async Task<ClassStatisticsDto?> GetStatisticsAsync(int classId)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
                .ThenInclude(s => s.Student)
                    .ThenInclude(st => st.User)
            .Include(c => c.Homeworks.Where(h => !h.IsDeleted))
            .Include(c => c.Exams.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == classId && !c.IsDeleted);

        if (classEntity == null)
            return null;

        var studentIds = classEntity.Students.Select(s => s.StudentId).ToList();

        // Homework completion calculation
        var totalHomeworks = classEntity.Homeworks.Count;
        var completedHomeworks = 0;
        if (totalHomeworks > 0 && studentIds.Any())
        {
            completedHomeworks = await _context.HomeworkSubmissions
                .CountAsync(hs => studentIds.Contains(hs.StudentId) &&
                                  classEntity.Homeworks.Select(h => h.Id).Contains(hs.HomeworkId) &&
                                  !hs.IsDeleted);
        }

        // Exam score calculation
        var examScores = new List<decimal>();
        if (classEntity.Exams.Any() && studentIds.Any())
        {
            examScores = await _context.InternalExamResults
                .Where(ier => studentIds.Contains(ier.StudentId) &&
                              classEntity.Exams.Select(e => e.Id).Contains(ier.InternalExamId) &&
                              !ier.IsDeleted)
                .Select(ier => ier.Score)
                .ToListAsync();
        }

        // Attendance calculation
        var attendanceRate = 0m;
        if (studentIds.Any())
        {
            var attendanceRecords = await _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId) && !a.IsDeleted)
                .ToListAsync();

            if (attendanceRecords.Any())
            {
                var presentCount = attendanceRecords.Count(a => a.IsPresent);
                attendanceRate = (decimal)presentCount / attendanceRecords.Count * 100;
            }
        }

        // Gender statistics
        var maleCount = 0;
        var femaleCount = 0;
        foreach (var assignment in classEntity.Students)
        {
            if (assignment.Student?.User?.Gender == Domain.Enums.Gender.Male)
                maleCount++;
            else if (assignment.Student?.User?.Gender == Domain.Enums.Gender.Female)
                femaleCount++;
        }

        return new ClassStatisticsDto
        {
            ClassId = classEntity.Id,
            ClassName = classEntity.ClassName,
            Capacity = classEntity.Capacity,
            CurrentStudentCount = classEntity.Students.Count,
            OccupancyRate = classEntity.Capacity > 0
                ? (decimal)classEntity.Students.Count / classEntity.Capacity * 100
                : 0,
            MaleStudentCount = maleCount,
            FemaleStudentCount = femaleCount,
            TotalHomeworks = totalHomeworks,
            CompletedHomeworks = completedHomeworks,
            HomeworkCompletionRate = totalHomeworks > 0 && studentIds.Any()
                ? (decimal)completedHomeworks / (totalHomeworks * studentIds.Count) * 100
                : 0,
            TotalExams = classEntity.Exams.Count,
            AverageExamScore = examScores.Any() ? examScores.Average() : 0,
            AverageAttendanceRate = attendanceRate
        };
    }

    public async Task<IEnumerable<ClassDto>> GetByGradeAsync(int grade)
    {
        var classes = await _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.BranchLocation)
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
            .Where(c => c.Grade == grade && !c.IsDeleted)
            .OrderBy(c => c.Branch)
            .ToListAsync();

        return classes.Select(MapToDto);
    }

    public async Task<IEnumerable<ClassDto>> GetByAcademicYearAsync(string academicYear)
    {
        var classes = await _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.BranchLocation)
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
            .Where(c => c.AcademicYear == academicYear && !c.IsDeleted)
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Branch)
            .ToListAsync();

        return classes.Select(MapToDto);
    }

    public async Task<IEnumerable<ClassDto>> GetActiveClassesAsync()
    {
        var classes = await _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.BranchLocation)
            .Include(c => c.Students.Where(s => !s.IsDeleted && s.IsActive))
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Branch)
            .ToListAsync();

        return classes.Select(MapToDto);
    }

    private ClassDto MapToDto(Class classEntity)
    {
        return new ClassDto
        {
            Id = classEntity.Id,
            ClassName = classEntity.ClassName,
            Grade = classEntity.Grade,
            Branch = classEntity.Branch,
            ClassTeacherId = classEntity.ClassTeacherId,
            ClassTeacherName = classEntity.ClassTeacher != null
                ? $"{classEntity.ClassTeacher.User.FirstName} {classEntity.ClassTeacher.User.LastName}"
                : null,
            Capacity = classEntity.Capacity,
            CurrentStudentCount = classEntity.Students.Count,
            AcademicYear = classEntity.AcademicYear,
            BranchId = classEntity.BranchId,
            BranchLocationName = classEntity.BranchLocation?.BranchName,
            IsActive = classEntity.IsActive,
            CreatedAt = classEntity.CreatedAt
        };
    }
}
