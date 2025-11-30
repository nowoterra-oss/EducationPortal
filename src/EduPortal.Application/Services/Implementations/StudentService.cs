using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduPortal.Application.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public StudentService(
        IStudentRepository studentRepository,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<StudentDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var students = await _studentRepository.GetAllAsync();
            var studentsList = students.ToList();

            var totalRecords = studentsList.Count;
            var pagedStudents = studentsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var studentDtos = _mapper.Map<List<StudentDto>>(pagedStudents);
            var pagedResponse = new PagedResponse<StudentDto>(studentDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<StudentDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<StudentDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetByIdAsync(int id)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            var dto = _mapper.Map<StudentDto>(student);
            return ApiResponse<StudentDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetByStudentNoAsync(string studentNo)
    {
        try
        {
            var student = await _studentRepository.GetByStudentNoAsync(studentNo);
            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            var dto = _mapper.Map<StudentDto>(student);
            return ApiResponse<StudentDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<string> GenerateStudentNoAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var yearPrefix = (currentYear % 100).ToString("D2"); // 2025 -> "25"

        var lastSequence = await _studentRepository.GetLastStudentSequenceForYearAsync(currentYear);
        var newSequence = lastSequence + 1;

        // Format: YY + 3 basamaklı sıra numarası (örn: 25001)
        return $"{yearPrefix}{newSequence:D3}";
    }

    public async Task<ApiResponse<string>> GetNextStudentNoPreviewAsync()
    {
        try
        {
            var nextNo = await GenerateStudentNoAsync();
            return ApiResponse<string>.SuccessResponse(nextNo, "Sonraki öğrenci numarası");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> CreateAsync(StudentCreateDto dto)
    {
        try
        {
            // Öğrenci numarasını otomatik oluştur
            var studentNo = await GenerateStudentNoAsync();

            // Benzersizlik kontrolü (ekstra güvenlik)
            var exists = await _studentRepository.StudentNoExistsAsync(studentNo);
            if (exists)
            {
                // Çakışma durumunda tekrar dene (çok nadir durum)
                studentNo = await GenerateStudentNoAsync();
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

            // Generate a default password (in production, this should be sent via email or set by user)
            var defaultPassword = $"Student{studentNo}!";
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<StudentDto>.ErrorResponse("Kullanıcı oluşturulamadı", errors);
            }

            // Assign Student role
            await _userManager.AddToRoleAsync(user, "Student");

            // Create Student entity
            var student = _mapper.Map<Student>(dto);
            student.UserId = user.Id;
            student.StudentNo = studentNo; // Otomatik oluşturulan numara

            await _studentRepository.AddAsync(student);

            var studentDto = _mapper.Map<StudentDto>(student);
            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Öğrenci başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> UpdateAsync(StudentUpdateDto dto)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(dto.Id);
            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            // Check if student number is being changed and if it already exists
            if (!string.IsNullOrEmpty(dto.StudentNo) && dto.StudentNo != student.StudentNo)
            {
                var exists = await _studentRepository.StudentNoExistsAsync(dto.StudentNo);
                if (exists)
                {
                    return ApiResponse<StudentDto>.ErrorResponse("Bu öğrenci numarası zaten kullanılıyor");
                }
            }

            // Update student properties
            if (!string.IsNullOrEmpty(dto.StudentNo))
                student.StudentNo = dto.StudentNo;
            if (!string.IsNullOrEmpty(dto.SchoolName))
                student.SchoolName = dto.SchoolName;
            if (dto.CurrentGrade.HasValue)
                student.CurrentGrade = dto.CurrentGrade.Value;
            if (dto.Gender.HasValue)
                student.Gender = dto.Gender.Value;
            if (dto.DateOfBirth.HasValue)
                student.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.Address != null)
                student.Address = dto.Address;
            if (dto.LGSPercentile.HasValue)
                student.LGSPercentile = dto.LGSPercentile;
            if (dto.IsBilsem.HasValue)
                student.IsBilsem = dto.IsBilsem.Value;
            if (dto.BilsemField != null)
                student.BilsemField = dto.BilsemField;
            if (dto.LanguageLevel != null)
                student.LanguageLevel = dto.LanguageLevel;
            if (dto.TargetMajor != null)
                student.TargetMajor = dto.TargetMajor;
            if (dto.TargetCountry != null)
                student.TargetCountry = dto.TargetCountry;

            // Update ApplicationUser if phone number is provided
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user != null)
                {
                    user.PhoneNumber = dto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _studentRepository.UpdateAsync(student);

            var studentDto = _mapper.Map<StudentDto>(student);
            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Öğrenci başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return ApiResponse<bool>.ErrorResponse("Öğrenci bulunamadı");
            }

            // Soft delete - mark the student as deleted
            student.IsDeleted = true;

            // Also deactivate the associated user
            var user = await _userManager.FindByIdAsync(student.UserId);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
            }

            await _studentRepository.UpdateAsync(student);

            return ApiResponse<bool>.SuccessResponse(true, "Öğrenci başarıyla silindi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<StudentDto>>> SearchAsync(string searchTerm)
    {
        try
        {
            var students = await _studentRepository.SearchStudentsAsync(searchTerm);
            var studentDtos = _mapper.Map<List<StudentDto>>(students);

            return ApiResponse<List<StudentDto>>.SuccessResponse(studentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<StudentDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }
}
