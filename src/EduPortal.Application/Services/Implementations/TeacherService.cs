using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Teacher;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduPortal.Application.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public TeacherService(
        ITeacherRepository teacherRepository,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _teacherRepository = teacherRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<TeacherDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var teachersList = teachers.ToList();

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
            var teacher = await _teacherRepository.GetByIdAsync(id);
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

            // Generate a default password
            var defaultPassword = $"Teacher{DateTime.Now.Ticks}!";
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
            var teacher = await _teacherRepository.GetByIdAsync(dto.Id);
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

            await _teacherRepository.UpdateAsync(teacher);

            var teacherDto = _mapper.Map<TeacherDto>(teacher);
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

    public async Task<ApiResponse<List<TeacherDto>>> SearchAsync(string searchTerm)
    {
        try
        {
            var teachers = await _teacherRepository.SearchTeachersAsync(searchTerm);
            var teacherDtos = _mapper.Map<List<TeacherDto>>(teachers);

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
