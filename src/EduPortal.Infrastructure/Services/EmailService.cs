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
    public async Task<ApiResponse<bool>> SendVeliOdemeBilgilendirmeAsync(int studentId, int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Student)
                .ThenInclude(s => s.User)
                .Include(p => p.Student)
                .ThenInclude(s => s.Parents)
                .ThenInclude(sp => sp.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

            if (payment == null)
                return ApiResponse<bool>.ErrorResponse("Ödeme bulunamadı");

            var parent = payment.Student.Parents.FirstOrDefault()?.Parent;
            if (parent == null || string.IsNullOrEmpty(parent.User.Email))
                return ApiResponse<bool>.ErrorResponse("Veli email adresi bulunamadı");

            var variables = new Dictionary<string, string>
            {
                { "VeliAdi", $"{parent.User.FirstName} {parent.User.LastName}" },
                { "OgrenciAdi", $"{payment.Student.User.FirstName} {payment.Student.User.LastName}" },
                { "OgrenciNo", payment.Student.StudentNo },
                { "OdemeTutari", payment.Amount.ToString("C") },
                { "OdemeTarihi", payment.PaymentDate.ToString("dd/MM/yyyy") },
                { "OdemeDurumu", payment.Status.ToString() },
                { "Aciklama", payment.Description ?? "" }
            };

            return await SendTemplateEmailAsync(
                parent.User.Email,
                (int)EmailTemplateType.VeliOdemeBilgilendirme,
                variables
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending payment notification for student {studentId}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> SendOdevBildirimiAsync(int homeworkId)
    {
        try
        {
            var homework = await _context.Homeworks
                .Include(h => h.Course)
                .Include(h => h.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(h => h.Id == homeworkId && !h.IsDeleted);

            if (homework == null)
                return ApiResponse<bool>.ErrorResponse("Ödev bulunamadı");

            // Get all students in this course
            var studentEmails = await _context.StudentTeacherAssignments
                .Where(sta => sta.CourseId == homework.CourseId &&
                             sta.TeacherId == homework.TeacherId &&
                             !sta.IsDeleted &&
                             sta.IsActive)
                .Include(sta => sta.Student)
                .ThenInclude(s => s.User)
                .Where(sta => !string.IsNullOrEmpty(sta.Student.User.Email))
                .Select(sta => sta.Student.User.Email!)
                .ToListAsync();

            if (!studentEmails.Any())
                return ApiResponse<bool>.ErrorResponse("Öğrenci bulunamadı");

            var variables = new Dictionary<string, string>
            {
                { "DersAdi", homework.Course.CourseName },
                { "OdevBaslik", homework.Title },
                { "OdevAciklama", homework.Description ?? "" },
                { "SonTeslimTarihi", homework.DueDate.ToString("dd/MM/yyyy HH:mm") },
                { "OgretmenAdi", $"{homework.Teacher.User.FirstName} {homework.Teacher.User.LastName}" }
            };

            var bulkDto = new BulkEmailDto
            {
                Recipients = studentEmails,
                Subject = $"Yeni Ödev: {homework.Title}",
                Body = ReplaceVariables(
                    "Merhaba,<br/><br/>" +
                    "<strong>{DersAdi}</strong> dersi için yeni bir ödev verildi.<br/><br/>" +
                    "<strong>Ödev:</strong> {OdevBaslik}<br/>" +
                    "<strong>Açıklama:</strong> {OdevAciklama}<br/>" +
                    "<strong>Son Teslim:</strong> {SonTeslimTarihi}<br/>" +
                    "<strong>Öğretmen:</strong> {OgretmenAdi}<br/><br/>" +
                    "İyi çalışmalar!",
                    variables
                ),
                IsHtml = true
            };

            return await SendBulkEmailAsync(bulkDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending homework notification {homeworkId}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> SendSinavSonucBildirimiAsync(int examResultId)
    {
        try
        {
            var examResult = await _context.ExamResults
                .Include(er => er.Student)
                .ThenInclude(s => s.User)
                .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(er => er.Id == examResultId && !er.IsDeleted);

            if (examResult == null)
                return ApiResponse<bool>.ErrorResponse("Sınav sonucu bulunamadı");

            if (string.IsNullOrEmpty(examResult.Student.User.Email))
                return ApiResponse<bool>.ErrorResponse("Öğrenci email adresi bulunamadı");

            var percentage = (examResult.Score / examResult.Exam.TotalPoints) * 100;

            var variables = new Dictionary<string, string>
            {
                { "OgrenciAdi", $"{examResult.Student.User.FirstName} {examResult.Student.User.LastName}" },
                { "DersAdi", examResult.Exam.Course.CourseName },
                { "SinavAdi", examResult.Exam.ExamName },
                { "Puan", examResult.Score.ToString("F2") },
                { "ToplamPuan", examResult.Exam.TotalPoints.ToString("F2") },
                { "Yuzde", percentage.ToString("F1") },
                { "SinavTarihi", examResult.Exam.ExamDate.ToString("dd/MM/yyyy") }
            };

            return await SendTemplateEmailAsync(
                examResult.Student.User.Email,
                (int)EmailTemplateType.SinavSonucBildirimi,
                variables
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending exam result notification {examResultId}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> SendDevamsizlikUyariAsync(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Parents)
                .ThenInclude(sp => sp.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

            if (student == null)
                return ApiResponse<bool>.ErrorResponse("Öğrenci bulunamadı");

            var totalAttendance = await _context.Attendances
                .CountAsync(a => a.StudentId == studentId && !a.IsDeleted);

            var absentCount = await _context.Attendances
                .CountAsync(a => a.StudentId == studentId &&
                               !a.IsDeleted &&
                               a.Status == AttendanceStatus.Gelmedi_Mazeretsiz);

            var absentRate = totalAttendance > 0 ? (decimal)absentCount / totalAttendance * 100 : 0;

            var variables = new Dictionary<string, string>
            {
                { "OgrenciAdi", $"{student.User.FirstName} {student.User.LastName}" },
                { "OgrenciNo", student.StudentNo },
                { "DevamsizlikSayisi", absentCount.ToString() },
                { "ToplamDers", totalAttendance.ToString() },
                { "DevamsizlikOrani", absentRate.ToString("F1") }
            };

            var parent = student.Parents.FirstOrDefault()?.Parent;
            var recipientEmail = parent?.User.Email ?? student.User.Email;

            if (string.IsNullOrEmpty(recipientEmail))
                return ApiResponse<bool>.ErrorResponse("Email adresi bulunamadı");

            return await SendTemplateEmailAsync(
                recipientEmail,
                (int)EmailTemplateType.DevamsizlikUyari,
                variables
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending absence warning for student {studentId}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> SendTaksitHatirlatmaAsync(int installmentId)
    {
        try
        {
            var installment = await _context.PaymentInstallments
                .Include(i => i.StudentPaymentPlan)
                .ThenInclude(spp => spp.Student)
                .ThenInclude(s => s.User)
                .Include(i => i.StudentPaymentPlan)
                .ThenInclude(spp => spp.Student)
                .ThenInclude(s => s.Parents)
                .ThenInclude(sp => sp.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(i => i.Id == installmentId && !i.IsDeleted);

            if (installment == null)
                return ApiResponse<bool>.ErrorResponse("Taksit bulunamadı");

            var student = installment.StudentPaymentPlan.Student;
            var parent = student.Parents.FirstOrDefault()?.Parent;

            var recipientEmail = parent?.User.Email ?? student.User.Email;
            if (string.IsNullOrEmpty(recipientEmail))
                return ApiResponse<bool>.ErrorResponse("Email adresi bulunamadı");

            var daysUntilDue = (installment.DueDate - DateTime.UtcNow).Days;

            var variables = new Dictionary<string, string>
            {
                { "OgrenciAdi", $"{student.User.FirstName} {student.User.LastName}" },
                { "OgrenciNo", student.StudentNo },
                { "TaksitNo", installment.InstallmentNumber.ToString() },
                { "TaksitTutari", installment.Amount.ToString("C") },
                { "SonOdemeTarihi", installment.DueDate.ToString("dd/MM/yyyy") },
                { "KalanGun", daysUntilDue.ToString() },
                { "Durum", installment.Status.ToString() }
            };

            return await SendTemplateEmailAsync(
                recipientEmail,
                (int)EmailTemplateType.TaksitHatirlatma,
                variables
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending installment reminder {installmentId}");
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }
}
