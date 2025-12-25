using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Teacher;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Application.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly DbContext _dbContext;

    public TeacherService(
        ITeacherRepository teacherRepository,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        DbContext dbContext)
    {
        _teacherRepository = teacherRepository;
        _userManager = userManager;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<PagedResponse<TeacherDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false)
    {
        try
        {
            var teachers = await _teacherRepository.GetAllAsync();
            // Filter based on includeInactive parameter
            var teachersList = includeInactive
                ? teachers.ToList()
                : teachers.Where(t => t.IsActive).ToList();

            var totalRecords = teachersList.Count;
            var pagedTeachers = teachersList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var teacherDtos = _mapper.Map<List<TeacherDto>>(pagedTeachers);
            var pagedResponse = new PagedResponse<TeacherDto>(teacherDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<TeacherDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<TeacherDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> GetByIdAsync(int id)
    {
        try
        {
            // Use extended details to include all related data
            var teacher = await _teacherRepository.GetTeacherWithExtendedDetailsAsync(id);
            if (teacher == null)
            {
                return ApiResponse<TeacherDto>.ErrorResponse("Öğretmen bulunamadı");
            }

            var dto = _mapper.Map<TeacherDto>(teacher);
            return ApiResponse<TeacherDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> CreateAsync(TeacherCreateDto dto)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApiResponse<TeacherDto>.ErrorResponse("Bu email adresi zaten kullanılıyor");
            }

            // Create ApplicationUser first
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Varsayılan şifre olarak TC Kimlik Numarası kullanılır (yoksa email)
            var defaultPassword = !string.IsNullOrEmpty(dto.IdentityNumber) ? dto.IdentityNumber : dto.Email;
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<TeacherDto>.ErrorResponse("Kullanıcı oluşturulamadı", errors);
            }

            // Assign Teacher role
            await _userManager.AddToRoleAsync(user, "Ogretmen");

            // Create Teacher entity
            var teacher = _mapper.Map<Teacher>(dto);
            teacher.UserId = user.Id;
            teacher.IsActive = true;

            await _teacherRepository.AddAsync(teacher);

            var teacherDto = _mapper.Map<TeacherDto>(teacher);
            return ApiResponse<TeacherDto>.SuccessResponse(teacherDto, "Öğretmen başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> UpdateAsync(TeacherUpdateDto dto)
    {
        try
        {
            // Get teacher with extended details
            var teacher = await _teacherRepository.GetTeacherWithExtendedDetailsAsync(dto.Id);
            if (teacher == null)
            {
                return ApiResponse<TeacherDto>.ErrorResponse("Öğretmen bulunamadı");
            }

            // Update teacher properties
            if (dto.Specialization != null)
                teacher.Specialization = dto.Specialization;

            if (dto.Experience.HasValue)
                teacher.Experience = dto.Experience.Value;

            if (dto.IsActive.HasValue)
                teacher.IsActive = dto.IsActive.Value;

            if (dto.BranchId.HasValue)
                teacher.BranchId = dto.BranchId.Value;

            if (dto.IsAlsoCoach.HasValue)
                teacher.IsAlsoCoach = dto.IsAlsoCoach.Value;

            // Update identity fields
            if (dto.IdentityType.HasValue)
                teacher.IdentityType = dto.IdentityType.Value;

            if (dto.IdentityNumber != null)
                teacher.IdentityNumber = dto.IdentityNumber;

            if (dto.Nationality != null)
                teacher.Nationality = dto.Nationality;

            // Update extended fields
            if (dto.Department != null)
                teacher.Department = dto.Department;

            if (dto.Biography != null)
                teacher.Biography = dto.Biography;

            if (dto.Education != null)
                teacher.Education = dto.Education;

            if (dto.Certifications != null)
                teacher.Certifications = dto.Certifications;

            if (dto.OfficeLocation != null)
                teacher.OfficeLocation = dto.OfficeLocation;

            if (dto.OfficeHours != null)
                teacher.OfficeHours = dto.OfficeHours;

            if (dto.HireDate.HasValue)
                teacher.HireDate = dto.HireDate.Value;

            if (dto.ProfilePhotoUrl != null)
                teacher.ProfilePhotoUrl = dto.ProfilePhotoUrl;

            // Update new extended form fields
            if (dto.ExperienceScore.HasValue)
                teacher.ExperienceScore = dto.ExperienceScore.Value;

            if (dto.CvUrl != null)
                teacher.CvUrl = dto.CvUrl;

            // Update Address (one-to-one)
            if (dto.Address != null)
            {
                if (teacher.Address == null)
                {
                    teacher.Address = new TeacherAddress { TeacherId = teacher.Id };
                }
                teacher.Address.Street = dto.Address.Street;
                teacher.Address.District = dto.Address.District;
                teacher.Address.City = dto.Address.City;
                teacher.Address.PostalCode = dto.Address.PostalCode;
                teacher.Address.Country = dto.Address.Country;
            }

            // Update Branches (replace all)
            if (dto.Branches != null)
            {
                // Remove existing branches
                var existingBranches = _dbContext.Set<TeacherBranch>().Where(b => b.TeacherId == teacher.Id);
                _dbContext.Set<TeacherBranch>().RemoveRange(existingBranches);

                // Add new branches
                foreach (var branchDto in dto.Branches)
                {
                    _dbContext.Set<TeacherBranch>().Add(new TeacherBranch
                    {
                        TeacherId = teacher.Id,
                        CourseId = branchDto.CourseId
                    });
                }
            }

            // Update Certificates (replace all)
            if (dto.Certificates != null)
            {
                // Remove existing certificates
                var existingCerts = _dbContext.Set<TeacherCertificate>().Where(c => c.TeacherId == teacher.Id);
                _dbContext.Set<TeacherCertificate>().RemoveRange(existingCerts);

                // Add new certificates
                foreach (var certDto in dto.Certificates)
                {
                    _dbContext.Set<TeacherCertificate>().Add(new TeacherCertificate
                    {
                        TeacherId = teacher.Id,
                        Name = certDto.Name,
                        Institution = certDto.Institution,
                        IssueDate = certDto.IssueDate,
                        ExpiryDate = certDto.ExpiryDate,
                        FileUrl = certDto.FileUrl
                    });
                }
            }

            // Update References (replace all)
            if (dto.References != null)
            {
                // Remove existing references
                var existingRefs = _dbContext.Set<TeacherReference>().Where(r => r.TeacherId == teacher.Id);
                _dbContext.Set<TeacherReference>().RemoveRange(existingRefs);

                // Add new references
                foreach (var refDto in dto.References)
                {
                    _dbContext.Set<TeacherReference>().Add(new TeacherReference
                    {
                        TeacherId = teacher.Id,
                        FullName = refDto.FullName,
                        Title = refDto.Title,
                        Organization = refDto.Organization,
                        PhoneNumber = refDto.PhoneNumber,
                        Email = refDto.Email
                    });
                }
            }

            // Update WorkTypes (replace all)
            if (dto.WorkTypes != null)
            {
                // Remove existing work types
                var existingWorkTypes = _dbContext.Set<TeacherWorkType>().Where(w => w.TeacherId == teacher.Id);
                _dbContext.Set<TeacherWorkType>().RemoveRange(existingWorkTypes);

                // Add new work types
                foreach (var workType in dto.WorkTypes)
                {
                    _dbContext.Set<TeacherWorkType>().Add(new TeacherWorkType
                    {
                        TeacherId = teacher.Id,
                        WorkType = (WorkType)workType
                    });
                }
            }

            // Update Student Assignments (Advisor/Coach) - if any assignment list is provided, replace all
            if (dto.AdvisorStudentIds != null || dto.CoachStudentIds != null)
            {
                // 1. Remove ALL existing assignments for this teacher first
                var existingAssignments = await _dbContext.Set<StudentTeacherAssignment>()
                    .Where(a => a.TeacherId == teacher.Id)
                    .ToListAsync();
                _dbContext.Set<StudentTeacherAssignment>().RemoveRange(existingAssignments);

                // 2. Add new advisor assignments
                if (dto.AdvisorStudentIds != null)
                {
                    foreach (var studentId in dto.AdvisorStudentIds)
                    {
                        _dbContext.Set<StudentTeacherAssignment>().Add(new StudentTeacherAssignment
                        {
                            TeacherId = teacher.Id,
                            StudentId = studentId,
                            CourseId = null,
                            AssignmentType = AssignmentType.Advisor,
                            StartDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                // 3. Add new coach assignments
                if (dto.CoachStudentIds != null)
                {
                    foreach (var studentId in dto.CoachStudentIds)
                    {
                        _dbContext.Set<StudentTeacherAssignment>().Add(new StudentTeacherAssignment
                        {
                            TeacherId = teacher.Id,
                            StudentId = studentId,
                            CourseId = null,
                            AssignmentType = AssignmentType.Coach,
                            StartDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }
            }

            // Update user phone number if provided
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var user = await _userManager.FindByIdAsync(teacher.UserId);
                if (user != null)
                {
                    user.PhoneNumber = dto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Refresh teacher with all related data for response
            var updatedTeacher = await _teacherRepository.GetTeacherWithExtendedDetailsAsync(dto.Id);
            var teacherDto = _mapper.Map<TeacherDto>(updatedTeacher);
            return ApiResponse<TeacherDto>.SuccessResponse(teacherDto, "Öğretmen başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return ApiResponse<bool>.ErrorResponse("Öğretmen bulunamadı");
            }

            // Soft delete - just mark as inactive
            teacher.IsActive = false;
            await _teacherRepository.UpdateAsync(teacher);

            return ApiResponse<bool>.SuccessResponse(true, "Öğretmen başarıyla silindi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TeacherDto>>> SearchAsync(string searchTerm, bool includeInactive = false)
    {
        try
        {
            var teachers = await _teacherRepository.SearchTeachersAsync(searchTerm);
            // Filter based on includeInactive parameter
            var filteredTeachers = includeInactive
                ? teachers.ToList()
                : teachers.Where(t => t.IsActive).ToList();
            var teacherDtos = _mapper.Map<List<TeacherDto>>(filteredTeachers);

            return ApiResponse<List<TeacherDto>>.SuccessResponse(teacherDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TeacherDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<object>>> GetTeacherCoursesAsync(int teacherId)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                return ApiResponse<List<object>>.ErrorResponse("Öğretmen bulunamadı");
            }

            var courses = await _teacherRepository.GetTeacherCoursesAsync(teacherId);
            var courseDtos = courses.Select(c => new
            {
                c.Id,
                CourseName = c.CourseName,
                CourseCode = c.CourseCode,
                Subject = c.Subject,
                Level = c.Level,
                Credits = c.Credits,
                c.IsActive
            }).ToList<object>();

            return ApiResponse<List<object>>.SuccessResponse(courseDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<object>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }
}
