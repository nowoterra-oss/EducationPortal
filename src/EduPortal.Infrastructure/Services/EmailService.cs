using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Email;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Configuration;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace EduPortal.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ApplicationDbContext context,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> SendEmailAsync(SendEmailDto dto)
    {
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = dto.Subject,
                Body = dto.Body,
                IsBodyHtml = dto.IsHtml
            };

            mailMessage.To.Add(new MailAddress(dto.To, dto.ToName ?? dto.To));

            // CC ekle
            if (!string.IsNullOrEmpty(dto.Cc))
            {
                var ccAddresses = dto.Cc.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var cc in ccAddresses)
                {
                    mailMessage.CC.Add(cc.Trim());
                }
            }

            // BCC ekle
            if (!string.IsNullOrEmpty(dto.Bcc))
            {
                var bccAddresses = dto.Bcc.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var bcc in bccAddresses)
                {
                    mailMessage.Bcc.Add(bcc.Trim());
                }
            }

            // Attachments ekle
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var attachment in dto.Attachments)
                {
                    if (attachment.FileContent != null)
                    {
                        var stream = new MemoryStream(attachment.FileContent);
                        mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName));
                    }
                    else if (!string.IsNullOrEmpty(attachment.FilePath) && File.Exists(attachment.FilePath))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachment.FilePath));
                    }
                }
            }

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation($"Email sent successfully to {dto.To}");
            return ApiResponse<bool>.SuccessResponse(true, "Email başarıyla gönderildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {dto.To}");
            return ApiResponse<bool>.ErrorResponse($"Email gönderilemedi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> SendBulkEmailAsync(BulkEmailDto dto)
    {
        try
        {
            var tasks = new List<Task<ApiResponse<bool>>>();

            foreach (var recipient in dto.Recipients)
            {
                var emailDto = new SendEmailDto
                {
                    To = recipient,
                    Subject = dto.Subject,
                    Body = dto.Body,
                    IsHtml = dto.IsHtml
                };

                tasks.Add(SendEmailAsync(emailDto));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.Success);

            _logger.LogInformation($"Bulk email sent: {successCount}/{dto.Recipients.Count} successful");

            return ApiResponse<bool>.SuccessResponse(
                true,
                $"{successCount}/{dto.Recipients.Count} email başarıyla gönderildi"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return ApiResponse<bool>.ErrorResponse($"Toplu email gönderilemedi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EmailTemplateDto>> GetTemplateAsync(int templateType)
    {
        try
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t =>
                    t.TemplateType == (EmailTemplateType)templateType &&
                    !t.IsDeleted &&
                    t.IsActive);

            if (template == null)
                return ApiResponse<EmailTemplateDto>.ErrorResponse("Template bulunamadı");

            var dto = MapToDto(template);
            return ApiResponse<EmailTemplateDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting template {templateType}");
            return ApiResponse<EmailTemplateDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<EmailTemplateDto>>> GetAllTemplatesAsync()
    {
        try
        {
            var templates = await _context.EmailTemplates
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.TemplateName)
                .ToListAsync();

            var dtos = templates.Select(MapToDto).ToList();
            return ApiResponse<List<EmailTemplateDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all templates");
            return ApiResponse<List<EmailTemplateDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateDto dto)
    {
        try
        {
            var template = new EmailTemplate
            {
                TemplateName = dto.TemplateName,
                TemplateType = (EmailTemplateType)dto.TemplateType,
                Subject = dto.Subject,
                Body = dto.Body,
                VariablesJson = dto.VariablesJson,
                IsActive = dto.IsActive
            };

            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();

            var result = MapToDto(template);
            return ApiResponse<EmailTemplateDto>.SuccessResponse(result, "Template başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return ApiResponse<EmailTemplateDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<EmailTemplateDto>> UpdateTemplateAsync(int id, CreateEmailTemplateDto dto)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return ApiResponse<EmailTemplateDto>.ErrorResponse("Template bulunamadı");

            template.TemplateName = dto.TemplateName;
            template.TemplateType = (EmailTemplateType)dto.TemplateType;
            template.Subject = dto.Subject;
            template.Body = dto.Body;
            template.VariablesJson = dto.VariablesJson;
            template.IsActive = dto.IsActive;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = MapToDto(template);
            return ApiResponse<EmailTemplateDto>.SuccessResponse(result, "Template başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating template {id}");
            return ApiResponse<EmailTemplateDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteTemplateAsync(int id)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return ApiResponse<bool>.ErrorResponse("Template bulunamadı");

            template.IsDeleted = true;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Template başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting template {id}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> SendTemplateEmailAsync(
        string recipientEmail,
        int templateType,
        Dictionary<string, string> variables)
    {
        try
        {
            var templateResult = await GetTemplateAsync(templateType);
            if (!templateResult.Success || templateResult.Data == null)
                return ApiResponse<bool>.ErrorResponse("Template bulunamadı");

            var template = templateResult.Data;
            var subject = ReplaceVariables(template.Subject, variables);
            var body = ReplaceVariables(template.Body, variables);

            var emailDto = new SendEmailDto
            {
                To = recipientEmail,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(emailDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Timeout = _emailSettings.TimeoutSeconds * 1000,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
        };

        return smtpClient;
    }

    private EmailTemplateDto MapToDto(EmailTemplate template)
    {
        var variables = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(template.VariablesJson))
        {
            try
            {
                variables = JsonSerializer.Deserialize<Dictionary<string, string>>(template.VariablesJson)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                // JSON parse hatası, boş dictionary dön
            }
        }

        return new EmailTemplateDto
        {
            Id = template.Id,
            TemplateName = template.TemplateName,
            TemplateType = template.TemplateType.ToString(),
            Subject = template.Subject,
            Body = template.Body,
            Variables = variables,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }

    private string ReplaceVariables(string text, Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            text = text.Replace($"{{{variable.Key}}}", variable.Value);
        }
        return text;
    }

    // Specific notification emails
    public Task<ApiResponse<bool>> SendVeliOdemeBilgilendirmeAsync(int studentId, int paymentId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> SendOdevBildirimiAsync(int homeworkId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> SendSinavSonucBildirimiAsync(int examResultId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> SendDevamsizlikUyariAsync(int studentId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> SendTaksitHatirlatmaAsync(int installmentId)
    {
        throw new NotImplementedException();
    }
}
