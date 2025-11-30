using EduPortal.Application.Common;
using EduPortal.Application.DTOs.File;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EduPortal.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;

    // Izin verilen dosya tipleri
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

    // Maksimum dosya boyutları (byte)
    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB
    private const long MaxDocumentSize = 25 * 1024 * 1024; // 25 MB
    private const long MaxProfilePhotoSize = 2 * 1024 * 1024; // 2 MB

    // Profil fotoğrafı boyutları
    private const int ProfilePhotoMaxWidth = 400;
    private const int ProfilePhotoMaxHeight = 400;
    private const int ThumbnailSize = 100;

    public FileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<FileUploadResultDto>> UploadFileAsync(
        IFormFile file,
        string category,
        string? subFolder = null)
    {
        try
        {
            // Validasyon
            if (file == null || file.Length == 0)
                return ApiResponse<FileUploadResultDto>.ErrorResponse("Dosya secilmedi");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isImage = AllowedImageExtensions.Contains(extension);
            var isDocument = AllowedDocumentExtensions.Contains(extension);

            if (!isImage && !isDocument)
                return ApiResponse<FileUploadResultDto>.ErrorResponse(
                    $"Desteklenmeyen dosya formati. Izin verilenler: {string.Join(", ", AllowedImageExtensions.Concat(AllowedDocumentExtensions))}");

            var maxSize = isImage ? MaxImageSize : MaxDocumentSize;
            if (file.Length > maxSize)
                return ApiResponse<FileUploadResultDto>.ErrorResponse(
                    $"Dosya boyutu cok buyuk. Maksimum: {maxSize / 1024 / 1024} MB");

            // Dosya adi olustur (guvenli)
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

            // Klasor yolu olustur
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
            var categoryFolder = Path.Combine(uploadsFolder, category);

            if (!string.IsNullOrEmpty(subFolder))
                categoryFolder = Path.Combine(categoryFolder, subFolder);

            Directory.CreateDirectory(categoryFolder);

            // Dosyayı kaydet
            var filePath = Path.Combine(categoryFolder, uniqueFileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // URL olustur
            var relativePath = string.IsNullOrEmpty(subFolder)
                ? $"/uploads/{category}/{uniqueFileName}"
                : $"/uploads/{category}/{subFolder}/{uniqueFileName}";

            var result = new FileUploadResultDto
            {
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                FileUrl = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Category = category,
                UploadedAt = DateTime.UtcNow
            };

            _logger.LogInformation("File uploaded: {FileName} to {Path}", file.FileName, relativePath);

            return ApiResponse<FileUploadResultDto>.SuccessResponse(result, "Dosya basariyla yuklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return ApiResponse<FileUploadResultDto>.ErrorResponse($"Dosya yuklenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProfilePhotoUploadResultDto>> UploadProfilePhotoAsync(
        IFormFile file,
        string userId)
    {
        try
        {
            // Validasyon
            if (file == null || file.Length == 0)
                return ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse("Fotograf secilmedi");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
                return ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse(
                    $"Desteklenmeyen format. Izin verilenler: {string.Join(", ", AllowedImageExtensions)}");

            if (file.Length > MaxProfilePhotoSize)
                return ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse(
                    $"Fotograf boyutu cok buyuk. Maksimum: {MaxProfilePhotoSize / 1024 / 1024} MB");

            // Content-Type kontrolu
            if (!file.ContentType.StartsWith("image/"))
                return ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse("Sadece resim dosyalari yuklenebilir");

            // Dosya adi olustur
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var mainFileName = $"{userId}_{timestamp}.jpg"; // Her zaman jpg olarak kaydet
            var thumbnailFileName = $"{userId}_{timestamp}_thumb.jpg";

            // Klasor yolu
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath ?? _environment.ContentRootPath,
                "uploads",
                "profile-photos");

            Directory.CreateDirectory(uploadsFolder);

            // Resmi yukle ve boyutlandir
            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            // Ana resmi boyutlandir (max 400x400, en-boy oranini koru)
            var mainFilePath = Path.Combine(uploadsFolder, mainFileName);
            await ResizeAndSaveImageAsync(image, mainFilePath, ProfilePhotoMaxWidth, ProfilePhotoMaxHeight);

            // Thumbnail olustur (100x100)
            var thumbnailFilePath = Path.Combine(uploadsFolder, thumbnailFileName);
            await ResizeAndSaveImageAsync(image, thumbnailFilePath, ThumbnailSize, ThumbnailSize);

            var photoUrl = $"/uploads/profile-photos/{mainFileName}";
            var thumbnailUrl = $"/uploads/profile-photos/{thumbnailFileName}";

            var result = new ProfilePhotoUploadResultDto
            {
                PhotoUrl = photoUrl,
                ThumbnailUrl = thumbnailUrl,
                UploadedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Profile photo uploaded for user: {UserId}, Size: {Width}x{Height}",
                userId, image.Width, image.Height);

            return ApiResponse<ProfilePhotoUploadResultDto>.SuccessResponse(result, "Fotograf basariyla yuklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for user: {UserId}", userId);
            return ApiResponse<ProfilePhotoUploadResultDto>.ErrorResponse($"Fotograf yuklenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return ApiResponse<bool>.ErrorResponse("Dosya URL'i belirtilmedi");

            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine(
                _environment.WebRootPath ?? _environment.ContentRootPath,
                relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {Path}", fileUrl);

                // Eger profil fotografiysa thumbnail'i de sil
                if (fileUrl.Contains("profile-photos") && !fileUrl.Contains("_thumb"))
                {
                    var thumbPath = fullPath.Replace(".jpg", "_thumb.jpg");
                    if (File.Exists(thumbPath))
                    {
                        File.Delete(thumbPath);
                        _logger.LogInformation("Thumbnail deleted: {Path}", thumbPath);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, "Dosya silindi");
            }

            return ApiResponse<bool>.ErrorResponse("Dosya bulunamadi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return ApiResponse<bool>.ErrorResponse($"Dosya silinirken hata olustu: {ex.Message}");
        }
    }

    public Task<bool> FileExistsAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return Task.FromResult(false);

        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(
            _environment.WebRootPath ?? _environment.ContentRootPath,
            relativePath);

        return Task.FromResult(File.Exists(fullPath));
    }

    /// <summary>
    /// Resmi boyutlandirir ve kaydeder (en-boy oranini korur)
    /// </summary>
    private static async Task ResizeAndSaveImageAsync(Image image, string outputPath, int maxWidth, int maxHeight)
    {
        // Mevcut boyutlari al
        var originalWidth = image.Width;
        var originalHeight = image.Height;

        // Yeni boyutlari hesapla (en-boy oranini koru)
        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        // Eger resim zaten kucukse buyutme
        if (ratio >= 1)
        {
            ratio = 1;
        }

        var newWidth = (int)(originalWidth * ratio);
        var newHeight = (int)(originalHeight * ratio);

        // Kopyasini olustur ve boyutlandir
        using var resizedImage = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(newWidth, newHeight),
            Mode = ResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));

        // JPEG olarak kaydet (kalite: 85)
        var encoder = new JpegEncoder
        {
            Quality = 85
        };

        await resizedImage.SaveAsync(outputPath, encoder);
    }
}
