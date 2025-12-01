using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentActivity;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class StudentActivityService : IStudentActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentActivityService> _logger;

    public StudentActivityService(ApplicationDbContext context, ILogger<StudentActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Summer Activities

    public async Task<ApiResponse<List<StudentSummerActivityDto>>> GetSummerActivitiesByStudentAsync(int studentId)
    {
        try
        {
            var activities = await _context.StudentSummerActivities
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.StartDate)
                .Select(a => MapToSummerActivityDto(a))
                .ToListAsync();

            return ApiResponse<List<StudentSummerActivityDto>>.SuccessResponse(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summer activities for student {StudentId}", studentId);
            return ApiResponse<List<StudentSummerActivityDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSummerActivityDto>> GetSummerActivityByIdAsync(int id)
    {
        try
        {
            var activity = await _context.StudentSummerActivities
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null)
                return ApiResponse<StudentSummerActivityDto>.ErrorResponse("Yaz aktivitesi bulunamadi");

            return ApiResponse<StudentSummerActivityDto>.SuccessResponse(MapToSummerActivityDto(activity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summer activity {Id}", id);
            return ApiResponse<StudentSummerActivityDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSummerActivityDto>> CreateSummerActivityAsync(CreateStudentSummerActivityDto dto)
    {
        try
        {
            var activity = new StudentSummerActivity
            {
                StudentId = dto.StudentId,
                ActivityName = dto.ActivityName,
                ActivityType = dto.ActivityType,
                OrganizingInstitution = dto.OrganizingInstitution,
                Location = dto.Location,
                Country = dto.Country,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DurationDays = dto.DurationDays,
                Description = dto.Description,
                SkillsGained = dto.SkillsGained,
                CertificateUrl = dto.CertificateUrl,
                DocumentUrl = dto.DocumentUrl
            };

            _context.StudentSummerActivities.Add(activity);
            await _context.SaveChangesAsync();

            // Reload with navigation
            await _context.Entry(activity).Reference(a => a.Student).LoadAsync();
            await _context.Entry(activity.Student).Reference(s => s.User).LoadAsync();

            return ApiResponse<StudentSummerActivityDto>.SuccessResponse(MapToSummerActivityDto(activity), "Yaz aktivitesi basariyla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating summer activity");
            return ApiResponse<StudentSummerActivityDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSummerActivityDto>> UpdateSummerActivityAsync(UpdateStudentSummerActivityDto dto)
    {
        try
        {
            var activity = await _context.StudentSummerActivities
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(a => a.Id == dto.Id);

            if (activity == null)
                return ApiResponse<StudentSummerActivityDto>.ErrorResponse("Yaz aktivitesi bulunamadi");

            activity.ActivityName = dto.ActivityName;
            activity.ActivityType = dto.ActivityType;
            activity.OrganizingInstitution = dto.OrganizingInstitution;
            activity.Location = dto.Location;
            activity.Country = dto.Country;
            activity.StartDate = dto.StartDate;
            activity.EndDate = dto.EndDate;
            activity.DurationDays = dto.DurationDays;
            activity.Description = dto.Description;
            activity.SkillsGained = dto.SkillsGained;
            activity.CertificateUrl = dto.CertificateUrl;
            activity.DocumentUrl = dto.DocumentUrl;

            await _context.SaveChangesAsync();

            return ApiResponse<StudentSummerActivityDto>.SuccessResponse(MapToSummerActivityDto(activity), "Yaz aktivitesi guncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating summer activity {Id}", dto.Id);
            return ApiResponse<StudentSummerActivityDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteSummerActivityAsync(int id)
    {
        try
        {
            var activity = await _context.StudentSummerActivities.FindAsync(id);
            if (activity == null)
                return ApiResponse<bool>.ErrorResponse("Yaz aktivitesi bulunamadi");

            _context.StudentSummerActivities.Remove(activity);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Yaz aktivitesi silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting summer activity {Id}", id);
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Internships

    public async Task<ApiResponse<List<StudentInternshipDto>>> GetInternshipsByStudentAsync(int studentId)
    {
        try
        {
            var internships = await _context.StudentInternships
                .Include(i => i.Student)
                    .ThenInclude(s => s.User)
                .Where(i => i.StudentId == studentId)
                .OrderByDescending(i => i.StartDate)
                .Select(i => MapToInternshipDto(i))
                .ToListAsync();

            return ApiResponse<List<StudentInternshipDto>>.SuccessResponse(internships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting internships for student {StudentId}", studentId);
            return ApiResponse<List<StudentInternshipDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentInternshipDto>> GetInternshipByIdAsync(int id)
    {
        try
        {
            var internship = await _context.StudentInternships
                .Include(i => i.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null)
                return ApiResponse<StudentInternshipDto>.ErrorResponse("Staj bulunamadi");

            return ApiResponse<StudentInternshipDto>.SuccessResponse(MapToInternshipDto(internship));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting internship {Id}", id);
            return ApiResponse<StudentInternshipDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentInternshipDto>> CreateInternshipAsync(CreateStudentInternshipDto dto)
    {
        try
        {
            var internship = new StudentInternship
            {
                StudentId = dto.StudentId,
                CompanyName = dto.CompanyName,
                Department = dto.Department,
                Position = dto.Position,
                Sector = dto.Sector,
                Location = dto.Location,
                Country = dto.Country,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalHours = dto.TotalHours,
                IsPaid = dto.IsPaid,
                SupervisorName = dto.SupervisorName,
                SupervisorTitle = dto.SupervisorTitle,
                SupervisorEmail = dto.SupervisorEmail,
                Description = dto.Description,
                Responsibilities = dto.Responsibilities,
                SkillsGained = dto.SkillsGained,
                CertificateUrl = dto.CertificateUrl,
                ReferenceLetterUrl = dto.ReferenceLetterUrl
            };

            _context.StudentInternships.Add(internship);
            await _context.SaveChangesAsync();

            await _context.Entry(internship).Reference(i => i.Student).LoadAsync();
            await _context.Entry(internship.Student).Reference(s => s.User).LoadAsync();

            return ApiResponse<StudentInternshipDto>.SuccessResponse(MapToInternshipDto(internship), "Staj basariyla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating internship");
            return ApiResponse<StudentInternshipDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentInternshipDto>> UpdateInternshipAsync(UpdateStudentInternshipDto dto)
    {
        try
        {
            var internship = await _context.StudentInternships
                .Include(i => i.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.Id == dto.Id);

            if (internship == null)
                return ApiResponse<StudentInternshipDto>.ErrorResponse("Staj bulunamadi");

            internship.CompanyName = dto.CompanyName;
            internship.Department = dto.Department;
            internship.Position = dto.Position;
            internship.Sector = dto.Sector;
            internship.Location = dto.Location;
            internship.Country = dto.Country;
            internship.StartDate = dto.StartDate;
            internship.EndDate = dto.EndDate;
            internship.TotalHours = dto.TotalHours;
            internship.IsPaid = dto.IsPaid;
            internship.SupervisorName = dto.SupervisorName;
            internship.SupervisorTitle = dto.SupervisorTitle;
            internship.SupervisorEmail = dto.SupervisorEmail;
            internship.Description = dto.Description;
            internship.Responsibilities = dto.Responsibilities;
            internship.SkillsGained = dto.SkillsGained;
            internship.CertificateUrl = dto.CertificateUrl;
            internship.ReferenceLetterUrl = dto.ReferenceLetterUrl;

            await _context.SaveChangesAsync();

            return ApiResponse<StudentInternshipDto>.SuccessResponse(MapToInternshipDto(internship), "Staj guncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating internship {Id}", dto.Id);
            return ApiResponse<StudentInternshipDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteInternshipAsync(int id)
    {
        try
        {
            var internship = await _context.StudentInternships.FindAsync(id);
            if (internship == null)
                return ApiResponse<bool>.ErrorResponse("Staj bulunamadi");

            _context.StudentInternships.Remove(internship);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Staj silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting internship {Id}", id);
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Social Projects

    public async Task<ApiResponse<List<StudentSocialProjectDto>>> GetSocialProjectsByStudentAsync(int studentId)
    {
        try
        {
            var projects = await _context.StudentSocialProjects
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.StartDate)
                .Select(p => MapToSocialProjectDto(p))
                .ToListAsync();

            return ApiResponse<List<StudentSocialProjectDto>>.SuccessResponse(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting social projects for student {StudentId}", studentId);
            return ApiResponse<List<StudentSocialProjectDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSocialProjectDto>> GetSocialProjectByIdAsync(int id)
    {
        try
        {
            var project = await _context.StudentSocialProjects
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return ApiResponse<StudentSocialProjectDto>.ErrorResponse("Sosyal sorumluluk projesi bulunamadi");

            return ApiResponse<StudentSocialProjectDto>.SuccessResponse(MapToSocialProjectDto(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting social project {Id}", id);
            return ApiResponse<StudentSocialProjectDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSocialProjectDto>> CreateSocialProjectAsync(CreateStudentSocialProjectDto dto)
    {
        try
        {
            var project = new StudentSocialProject
            {
                StudentId = dto.StudentId,
                ProjectName = dto.ProjectName,
                ProjectType = dto.ProjectType,
                OrganizationName = dto.OrganizationName,
                Category = dto.Category,
                Role = dto.Role,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalHours = dto.TotalHours,
                ImpactedPeopleCount = dto.ImpactedPeopleCount,
                Description = dto.Description,
                Objectives = dto.Objectives,
                Outcomes = dto.Outcomes,
                SkillsGained = dto.SkillsGained,
                CertificateUrl = dto.CertificateUrl,
                DocumentUrl = dto.DocumentUrl,
                MediaUrl = dto.MediaUrl
            };

            _context.StudentSocialProjects.Add(project);
            await _context.SaveChangesAsync();

            await _context.Entry(project).Reference(p => p.Student).LoadAsync();
            await _context.Entry(project.Student).Reference(s => s.User).LoadAsync();

            return ApiResponse<StudentSocialProjectDto>.SuccessResponse(MapToSocialProjectDto(project), "Sosyal sorumluluk projesi basariyla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating social project");
            return ApiResponse<StudentSocialProjectDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentSocialProjectDto>> UpdateSocialProjectAsync(UpdateStudentSocialProjectDto dto)
    {
        try
        {
            var project = await _context.StudentSocialProjects
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project == null)
                return ApiResponse<StudentSocialProjectDto>.ErrorResponse("Sosyal sorumluluk projesi bulunamadi");

            project.ProjectName = dto.ProjectName;
            project.ProjectType = dto.ProjectType;
            project.OrganizationName = dto.OrganizationName;
            project.Category = dto.Category;
            project.Role = dto.Role;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.TotalHours = dto.TotalHours;
            project.ImpactedPeopleCount = dto.ImpactedPeopleCount;
            project.Description = dto.Description;
            project.Objectives = dto.Objectives;
            project.Outcomes = dto.Outcomes;
            project.SkillsGained = dto.SkillsGained;
            project.CertificateUrl = dto.CertificateUrl;
            project.DocumentUrl = dto.DocumentUrl;
            project.MediaUrl = dto.MediaUrl;

            await _context.SaveChangesAsync();

            return ApiResponse<StudentSocialProjectDto>.SuccessResponse(MapToSocialProjectDto(project), "Sosyal sorumluluk projesi guncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating social project {Id}", dto.Id);
            return ApiResponse<StudentSocialProjectDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteSocialProjectAsync(int id)
    {
        try
        {
            var project = await _context.StudentSocialProjects.FindAsync(id);
            if (project == null)
                return ApiResponse<bool>.ErrorResponse("Sosyal sorumluluk projesi bulunamadi");

            _context.StudentSocialProjects.Remove(project);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Sosyal sorumluluk projesi silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting social project {Id}", id);
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Mapping Methods

    private static StudentSummerActivityDto MapToSummerActivityDto(StudentSummerActivity a)
    {
        return new StudentSummerActivityDto
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = $"{a.Student?.User?.FirstName} {a.Student?.User?.LastName}".Trim(),
            ActivityName = a.ActivityName,
            ActivityType = a.ActivityType,
            OrganizingInstitution = a.OrganizingInstitution,
            Location = a.Location,
            Country = a.Country,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            DurationDays = a.DurationDays,
            Description = a.Description,
            SkillsGained = a.SkillsGained,
            CertificateUrl = a.CertificateUrl,
            DocumentUrl = a.DocumentUrl,
            CreatedAt = a.CreatedAt
        };
    }

    private static StudentInternshipDto MapToInternshipDto(StudentInternship i)
    {
        return new StudentInternshipDto
        {
            Id = i.Id,
            StudentId = i.StudentId,
            StudentName = $"{i.Student?.User?.FirstName} {i.Student?.User?.LastName}".Trim(),
            CompanyName = i.CompanyName,
            Department = i.Department,
            Position = i.Position,
            Sector = i.Sector,
            Location = i.Location,
            Country = i.Country,
            StartDate = i.StartDate,
            EndDate = i.EndDate,
            TotalHours = i.TotalHours,
            IsPaid = i.IsPaid,
            SupervisorName = i.SupervisorName,
            SupervisorTitle = i.SupervisorTitle,
            SupervisorEmail = i.SupervisorEmail,
            Description = i.Description,
            Responsibilities = i.Responsibilities,
            SkillsGained = i.SkillsGained,
            CertificateUrl = i.CertificateUrl,
            ReferenceLetterUrl = i.ReferenceLetterUrl,
            CreatedAt = i.CreatedAt
        };
    }

    private static StudentSocialProjectDto MapToSocialProjectDto(StudentSocialProject p)
    {
        return new StudentSocialProjectDto
        {
            Id = p.Id,
            StudentId = p.StudentId,
            StudentName = $"{p.Student?.User?.FirstName} {p.Student?.User?.LastName}".Trim(),
            ProjectName = p.ProjectName,
            ProjectType = p.ProjectType,
            OrganizationName = p.OrganizationName,
            Category = p.Category,
            Role = p.Role,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            TotalHours = p.TotalHours,
            ImpactedPeopleCount = p.ImpactedPeopleCount,
            Description = p.Description,
            Objectives = p.Objectives,
            Outcomes = p.Outcomes,
            SkillsGained = p.SkillsGained,
            CertificateUrl = p.CertificateUrl,
            DocumentUrl = p.DocumentUrl,
            MediaUrl = p.MediaUrl,
            CreatedAt = p.CreatedAt
        };
    }

    #endregion
}
