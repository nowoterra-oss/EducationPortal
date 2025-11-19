using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Auth;

namespace EduPortal.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId);
    string GenerateJwtToken(UserDto user);
}
