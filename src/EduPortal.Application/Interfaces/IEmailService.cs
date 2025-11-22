using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Email;

namespace EduPortal.Application.Interfaces;

public interface IEmailService
{
    // Basic email operations
    Task<ApiResponse<bool>> SendEmailAsync(SendEmailDto dto);
    Task<ApiResponse<bool>> SendBulkEmailAsync(BulkEmailDto dto);

    // Template operations
    Task<ApiResponse<EmailTemplateDto>> GetTemplateAsync(int templateType);
    Task<ApiResponse<List<EmailTemplateDto>>> GetAllTemplatesAsync();
    Task<ApiResponse<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateDto dto);
    Task<ApiResponse<EmailTemplateDto>> UpdateTemplateAsync(int id, CreateEmailTemplateDto dto);
    Task<ApiResponse<bool>> DeleteTemplateAsync(int id);
    Task<ApiResponse<bool>> SendTemplateEmailAsync(string recipientEmail, int templateType, Dictionary<string, string> variables);

    // Specific notification emails
    Task<ApiResponse<bool>> SendVeliOdemeBilgilendirmeAsync(int studentId, int paymentId);
    Task<ApiResponse<bool>> SendOdevBildirimiAsync(int homeworkId);
    Task<ApiResponse<bool>> SendSinavSonucBildirimiAsync(int examResultId);
    Task<ApiResponse<bool>> SendDevamsizlikUyariAsync(int studentId);
    Task<ApiResponse<bool>> SendTaksitHatirlatmaAsync(int installmentId);
}
