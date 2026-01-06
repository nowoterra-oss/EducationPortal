using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Auth;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduPortal.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IPermissionService _permissionService;
    private readonly DbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMapper mapper,
        IConfiguration configuration,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        IPermissionService permissionService,
        DbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _configuration = configuration;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _permissionService = permissionService;
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Email veya şifre hatalı");
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Kullanıcı hesabı aktif değil");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Email veya şifre hatalı");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            // Kullanıcı tipini ve entity ID'lerini belirle (entity tablolarından)
            await PopulateUserTypeAndIdsAsync(userDto, user.Id, roles);

            // Kullanıcının eksik varsayılan yetkilerini senkronize et (yeni eklenen yetkiler için)
            if (!string.IsNullOrEmpty(userDto.UserType) && userDto.UserType != "Admin")
            {
                await _permissionService.SyncMissingDefaultPermissionsAsync(user.Id, userDto.UserType);
            }

            // Kullanıcının permission'larını al
            userDto.Permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id);

            var token = GenerateJwtToken(userDto);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"));

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Giriş başarılı");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Bu email adresi zaten kullanılıyor");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<LoginResponseDto>.ErrorResponse("Kullanıcı oluşturulamadı", errors);
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, registerDto.Role);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = new List<string> { registerDto.Role };

            var token = GenerateJwtToken(userDto);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"));

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Kayıt başarılı");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.ErrorResponse("Şifre değiştirilemedi", errors);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Şifre başarıyla değiştirildi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResponse("Kullanıcı bulunamadı");
            }

            var userDto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            // Kullanıcı tipini ve entity ID'lerini belirle (entity tablolarından)
            await PopulateUserTypeAndIdsAsync(userDto, userId, roles);

            // Kullanıcının permission'larını al
            userDto.Permissions = await _permissionService.GetEffectivePermissionsAsync(userId);

            return ApiResponse<UserDto>.SuccessResponse(userDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    private async Task PopulateUserTypeAndIdsAsync(UserDto userDto, string userId, IList<string> roles)
    {
        // Admin kontrolü (role bazlı - sadece Admin rolü kaldı)
        if (roles.Contains("Admin"))
        {
            userDto.UserType = "Admin";
            return;
        }

        // Öğrenci kontrolü (entity tablosundan)
        var student = await _studentRepository.GetByUserIdAsync(userId);
        if (student != null)
        {
            userDto.StudentId = student.Id;
            userDto.UserType = "Student";
            return;
        }

        // Öğretmen kontrolü (entity tablosundan)
        var teacher = await _teacherRepository.GetByUserIdAsync(userId);
        if (teacher != null)
        {
            userDto.TeacherId = teacher.Id;
            userDto.UserType = "Teacher";
            return;
        }

        // Danışman kontrolü (entity tablosundan)
        var counselor = await _dbContext.Set<Counselor>()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
        if (counselor != null)
        {
            userDto.CounselorId = counselor.Id;
            userDto.UserType = "Counselor";
            return;
        }

        // Veli kontrolü (entity tablosundan)
        var parent = await _dbContext.Set<Parent>()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (parent != null)
        {
            userDto.ParentId = parent.Id;
            userDto.UserType = "Parent";
            return;
        }

        // Hiçbir entity'ye ait değilse UserType boş kalır
        userDto.UserType = "Unknown";
    }

    public string GenerateJwtToken(UserDto user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles to claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add UserType to claims
        if (!string.IsNullOrEmpty(user.UserType))
        {
            claims.Add(new Claim("UserType", user.UserType));
        }

        // Add entity IDs to claims
        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
        }
        if (user.TeacherId.HasValue)
        {
            claims.Add(new Claim("TeacherId", user.TeacherId.Value.ToString()));
        }
        if (user.ParentId.HasValue)
        {
            claims.Add(new Claim("ParentId", user.ParentId.Value.ToString()));
        }
        if (user.CounselorId.HasValue)
        {
            claims.Add(new Claim("CounselorId", user.CounselorId.Value.ToString()));
        }

        // Add permissions to claims
        if (user.Permissions != null)
        {
            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
