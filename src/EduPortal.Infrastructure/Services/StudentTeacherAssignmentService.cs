using EduPortal.Application.DTOs.StudentTeacherAssignment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class StudentTeacherAssignmentService : IStudentTeacherAssignmentService
{
    private readonly ApplicationDbContext _context;

    public StudentTeacherAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentTeacherAssignmentDto>> GetAllAsync()
    {
        return await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Include(x => x.Course)
            .Where(x => x.IsActive)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<StudentTeacherAssignmentDto?> GetByIdAsync(int id)
    {
        var assignment = await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);

        return assignment != null ? MapToDto(assignment) : null;
    }

    public async Task<IEnumerable<StudentTeacherAssignmentDto>> GetByStudentIdAsync(int studentId)
    {
        return await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Include(x => x.Course)
            .Where(x => x.StudentId == studentId && x.IsActive)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentTeacherAssignmentDto>> GetByTeacherIdAsync(int teacherId)
    {
        return await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Course)
            .Where(x => x.TeacherId == teacherId && x.IsActive)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentTeacherAssignmentDto>> GetByCourseIdAsync(int courseId)
    {
        return await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Where(x => x.CourseId == courseId && x.IsActive)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<StudentTeacherAssignmentDto> CreateAsync(CreateStudentTeacherAssignmentDto dto)
    {
        var assignment = new StudentTeacherAssignment
        {
            StudentId = dto.StudentId,
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Notes = dto.Notes,
            IsActive = true
        };

        _context.StudentTeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(assignment.Id))!;
    }

    public async Task<IEnumerable<StudentTeacherAssignmentDto>> CreateBulkAsync(List<CreateStudentTeacherAssignmentDto> dtos)
    {
        var assignments = dtos.Select(dto => new StudentTeacherAssignment
        {
            StudentId = dto.StudentId,
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Notes = dto.Notes,
            IsActive = true
        }).ToList();

        _context.StudentTeacherAssignments.AddRange(assignments);
        await _context.SaveChangesAsync();

        var ids = assignments.Select(x => x.Id).ToList();
        return await _context.StudentTeacherAssignments
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Include(x => x.Course)
            .Where(x => ids.Contains(x.Id))
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<StudentTeacherAssignmentDto> UpdateAsync(int id, UpdateStudentTeacherAssignmentDto dto)
    {
        var assignment = await _context.StudentTeacherAssignments.FindAsync(id);
        if (assignment == null)
            throw new Exception($"Assignment with ID {id} not found");

        if (dto.EndDate.HasValue)
            assignment.EndDate = dto.EndDate.Value;

        if (dto.IsActive.HasValue)
            assignment.IsActive = dto.IsActive.Value;

        if (!string.IsNullOrEmpty(dto.Notes))
            assignment.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var assignment = await _context.StudentTeacherAssignments.FindAsync(id);
        if (assignment == null)
            return false;

        _context.StudentTeacherAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var assignment = await _context.StudentTeacherAssignments.FindAsync(id);
        if (assignment == null)
            return false;

        assignment.IsActive = false;
        assignment.EndDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static StudentTeacherAssignmentDto MapToDto(StudentTeacherAssignment entity)
    {
        return new StudentTeacherAssignmentDto
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            StudentName = $"{entity.Student?.User?.FirstName} {entity.Student?.User?.LastName}",
            StudentNo = entity.Student?.StudentNo,
            TeacherId = entity.TeacherId,
            TeacherName = $"{entity.Teacher?.User?.FirstName} {entity.Teacher?.User?.LastName}",
            CourseId = entity.CourseId,
            CourseName = entity.Course?.CourseName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }
}
