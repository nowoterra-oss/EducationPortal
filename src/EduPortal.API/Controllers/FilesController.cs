using EduPortal.Application.Common;
using EduPortal.Application.DTOs.File;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// File upload and management endpoints
/// </summary>
[ApiController]
[Route("api/files")]
[Produces("application/json")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="category">File category (documents, images, etc.)</param>
    /// <returns>Uploaded file information</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResultDto>), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> Upload(
        IFormFile file,
        [FromQuery] string category = "general")
    {
        var result = await _fileStorageService.UploadFileAsync(file, category);

        if (result.Success)
            return CreatedAtAction(nameof(Upload), result);

        return BadRequest(result);
    }

    /// <summary>
    /// Upload profile photo
    /// </summary>
    /// <param name="file">Photo file (JPG, PNG, etc.)</param>
    /// <returns>Photo URL</returns>
    [HttpPost("upload/profile-photo")]
    [ProducesResponseType(typeof(ApiResponse<ProfilePhotoUploadResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProfilePhotoUploadResultDto>), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2 MB
    public async Task<ActionResult<ApiResponse<ProfilePhotoUploadResultDto>>> UploadProfilePhoto(
        IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse("Kullanici kimligi bulunamadi"));

        var result = await _fileStorageService.UploadProfilePhotoAsync(file, userId);

        if (result.Success)
            return CreatedAtAction(nameof(UploadProfilePhoto), result);

        return BadRequest(result);
    }

    /// <summary>
    /// Upload student profile photo (for registration)
    /// </summary>
    /// <param name="file">Photo file</param>
    /// <param name="tempId">Temporary ID for new student</param>
    [HttpPost("upload/student-photo")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<ProfilePhotoUploadResultDto>), StatusCodes.Status201Created)]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2 MB
    public async Task<ActionResult<ApiResponse<ProfilePhotoUploadResultDto>>> UploadStudentPhoto(
        IFormFile file,
        [FromQuery] string? tempId = null)
    {
        var identifier = tempId ?? Guid.NewGuid().ToString("N");
        var result = await _fileStorageService.UploadProfilePhotoAsync(file, $"student_{identifier}");

        if (result.Success)
            return CreatedAtAction(nameof(UploadStudentPhoto), result);

        return BadRequest(result);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="fileUrl">File URL to delete</param>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete([FromQuery] string fileUrl)
    {
        var result = await _fileStorageService.DeleteFileAsync(fileUrl);

        if (result.Success)
            return Ok(result);

        return NotFound(result);
    }

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="fileUrl">File URL to check</param>
    [HttpGet("exists")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> FileExists([FromQuery] string fileUrl)
    {
        var exists = await _fileStorageService.FileExistsAsync(fileUrl);
        return Ok(ApiResponse<bool>.SuccessResponse(exists));
    }
}
