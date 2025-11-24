using EduPortal.Application.DTOs.StudentClassAssignment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class StudentClassAssignmentService : IStudentClassAssignmentService
{
    private readonly ApplicationDbContext _context;

    public StudentClassAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<StudentClassAssignmentDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, bool? isActive = null)
    {
        var query = _context.StudentClassAssignments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Class)
            .Include(a => a.AcademicTerm)
            .AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AssignmentDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<StudentClassAssignmentDto?> GetByIdAsync(int id)
    {
        var assignment = await _context.StudentClassAssignments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Class)
            .Include(a => a.AcademicTerm)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return assignment == null ? null : MapToDto(assignment);
    }

    public async Task<StudentClassAssignmentDto> CreateAsync(CreateStudentClassAssignmentDto dto)
    {
        // Check for existing active assignment
        var existingAssignment = await _context.StudentClassAssignments
            .FirstOrDefaultAsync(a =>
                a.StudentId == dto.StudentId &&
                a.ClassId == dto.ClassId &&
                a.AcademicTermId == dto.AcademicTermId &&
                a.IsActive);

        if (existingAssignment != null)
            throw new InvalidOperationException("Bu öğrenci zaten bu dönemde bu sınıfa atanmış");

        var assignment = new StudentClassAssignment
        {
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
            AcademicTermId = dto.AcademicTermId,
            AssignmentDate = dto.AssignmentDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            Notes = dto.Notes
        };

        _context.StudentClassAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(assignment.Id) ?? throw new InvalidOperationException("Atama oluşturulamadı");
    }

    public async Task<IEnumerable<StudentClassAssignmentDto>> CreateBulkAsync(IEnumerable<CreateStudentClassAssignmentDto> dtos)
    {
        var results = new List<StudentClassAssignmentDto>();

        foreach (var dto in dtos)
        {
            try
            {
                var result = await CreateAsync(dto);
                results.Add(result);
            }
            catch (InvalidOperationException)
            {
                // Skip duplicates
                continue;
            }
        }

        return results;
    }

    public async Task<StudentClassAssignmentDto> UpdateAsync(int id, UpdateStudentClassAssignmentDto dto)
    {
        var assignment = await _context.StudentClassAssignments.FindAsync(id);

        if (assignment == null)
            throw new KeyNotFoundException("Atama bulunamadı");

        assignment.ClassId = dto.ClassId;
        assignment.AcademicTermId = dto.AcademicTermId;
        assignment.AssignmentDate = dto.AssignmentDate;
        assignment.EndDate = dto.EndDate;
        assignment.IsActive = dto.IsActive;
        assignment.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Atama güncellenemedi");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var assignment = await _context.StudentClassAssignments.FindAsync(id);

        if (assignment == null)
            return false;

        _context.StudentClassAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<StudentClassAssignmentDto>> GetByStudentAsync(int studentId)
    {
        var assignments = await _context.StudentClassAssignments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Class)
            .Include(a => a.AcademicTerm)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AssignmentDate)
            .AsNoTracking()
            .ToListAsync();

        return assignments.Select(MapToDto);
    }

    public async Task<(IEnumerable<StudentClassAssignmentDto> Items, int TotalCount)> GetByClassAsync(int classId, int pageNumber, int pageSize)
    {
        var query = _context.StudentClassAssignments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Class)
            .Include(a => a.AcademicTerm)
            .Where(a => a.ClassId == classId)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AssignmentDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return (items, totalCount);
    }

    private static StudentClassAssignmentDto MapToDto(StudentClassAssignment assignment)
    {
        return new StudentClassAssignmentDto
        {
            Id = assignment.Id,
            StudentId = assignment.StudentId,
            StudentName = assignment.Student?.User != null
                ? $"{assignment.Student.User.FirstName} {assignment.Student.User.LastName}"
                : string.Empty,
            StudentNumber = assignment.Student?.StudentNumber,
            ClassId = assignment.ClassId,
            ClassName = assignment.Class?.ClassName ?? string.Empty,
            AcademicTermId = assignment.AcademicTermId,
            AcademicTermName = assignment.AcademicTerm?.TermName ?? string.Empty,
            AssignmentDate = assignment.AssignmentDate,
            EndDate = assignment.EndDate,
            IsActive = assignment.IsActive,
            Notes = assignment.Notes,
            CreatedAt = assignment.CreatedAt
        };
    }
}
