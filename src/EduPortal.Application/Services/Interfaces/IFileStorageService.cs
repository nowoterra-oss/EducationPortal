using EduPortal.Application.Common;
using EduPortal.Application.DTOs.File;
using Microsoft.AspNetCore.Http;

namespace EduPortal.Application.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Dosya yukler ve URL doner
    /// </summary>
    Task<ApiResponse<FileUploadResultDto>> UploadFileAsync(
        IFormFile file,
        string category,
        string? subFolder = null);

    /// <summary>
    /// Profil fotografı yukler (resize + thumbnail olusturur)
    /// </summary>
    Task<ApiResponse<ProfilePhotoUploadResultDto>> UploadProfilePhotoAsync(
        IFormFile file,
        string userId);

    /// <summary>
    /// Dosyayı siler
    /// </summary>
    Task<ApiResponse<bool>> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Dosya var mı kontrol eder
    /// </summary>
    Task<bool> FileExistsAsync(string fileUrl);
}
