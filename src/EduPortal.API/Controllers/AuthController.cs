using System.Security.Claims;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Auth;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Authentication and user management endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">Registration information</param>
    /// <returns>User information with token</returns>
    /// <response code="201">User registered successfully</response>
    /// <response code="400">Invalid registration data or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse("Kayıt işlemi sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid credentials</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login");
            return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse("Giriş işlemi sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New JWT token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid refresh token</response>
    /// <remarks>Commented out: RefreshTokenAsync method doesn't exist in IAuthService</remarks>
    /*
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse("Token yenileme sırasında bir hata oluştu"));
        }
    }
    */

    /// <summary>
    /// User logout
    /// </summary>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="401">Unauthorized</response>
    /// <remarks>Commented out: LogoutAsync method doesn't exist in IAuthService</remarks>
    /*
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        try
        {
            var result = await _authService.LogoutAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Çıkış işlemi sırasında bir hata oluştu"));
        }
    }
    */

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="changePasswordDto">Password change information</param>
    /// <returns>Password change confirmation</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Invalid current password or validation error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password change");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Şifre değiştirme sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    /// <param name="email">User email address</param>
    /// <returns>Password reset email sent confirmation</returns>
    /// <response code="200">Password reset email sent</response>
    /// <response code="400">Invalid email or user not found</response>
    /// <remarks>Commented out: ForgotPasswordAsync method doesn't exist in IAuthService</remarks>
    /*
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] string email)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Şifre sıfırlama isteği sırasında bir hata oluştu"));
        }
    }
    */

    /// <summary>
    /// Reset password with reset token
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Reset token</param>
    /// <param name="newPassword">New password</param>
    /// <returns>Password reset confirmation</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid token or password</response>
    /// <remarks>Commented out: ResetPasswordAsync method doesn't exist in IAuthService</remarks>
    /*
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(
        [FromQuery] string email,
        [FromQuery] string token,
        [FromBody] string newPassword)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(email, token, newPassword);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Şifre sıfırlama sırasında bir hata oluştu"));
        }
    }
    */

    /// <summary>
    /// Verify user email
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Verification token</param>
    /// <returns>Email verification confirmation</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid token or email</response>
    /// <remarks>Commented out: VerifyEmailAsync method doesn't exist in IAuthService</remarks>
    /*
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyEmail(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(email, token);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email verification");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Email doğrulama sırasında bir hata oluştu"));
        }
    }
    */

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">User not found</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.GetCurrentUserAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Kullanıcı bilgileri alınırken bir hata oluştu"));
        }
    }
}
