using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Certificate;
using Microsoft.AspNetCore.Http;

namespace EduPortal.Application.Services.Interfaces;

public interface IStudentCertificateService
{
    Task<ApiResponse<List<StudentCertificateDto>>> GetByStudentIdAsync(int studentId);
    Task<ApiResponse<StudentCertificateDto>> GetByIdAsync(int studentId, int certificateId);
    Task<ApiResponse<StudentCertificateUploadResultDto>> UploadAsync(int studentId, IFormFile file, StudentCertificateCreateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int studentId, int certificateId);
    Task<(byte[]? fileBytes, string? contentType, string? fileName)> DownloadAsync(int studentId, int certificateId);
}
