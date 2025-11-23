using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Email;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Email yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IEmailService emailService,
        ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Email gönder
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendEmail([FromBody] SendEmailDto dto)
    {
        try
        {
            var result = await _emailService.SendEmailAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Toplu email gönder
    /// </summary>
    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendBulkEmail([FromBody] BulkEmailDto dto)
    {
        try
        {
            var result = await _emailService.SendBulkEmailAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Tüm email template'lerini getir
    /// </summary>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<EmailTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<EmailTemplateDto>>>> GetAllTemplates()
    {
        try
        {
            var result = await _emailService.GetAllTemplatesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            return StatusCode(500, ApiResponse<List<EmailTemplateDto>>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Template ID'ye göre getir
    /// </summary>
    [HttpGet("templates/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> GetTemplate(int id)
    {
        try
        {
            var result = await _emailService.GetTemplateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting template {id}");
            return StatusCode(500, ApiResponse<EmailTemplateDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Yeni email template oluştur
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> CreateTemplate([FromBody] CreateEmailTemplateDto dto)
    {
        try
        {
            var result = await _emailService.CreateTemplateAsync(dto);
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(nameof(GetTemplate), new { id = result.Data.Id }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, ApiResponse<EmailTemplateDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Email template güncelle
    /// </summary>
    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> UpdateTemplate(int id, [FromBody] CreateEmailTemplateDto dto)
    {
        try
        {
            var result = await _emailService.UpdateTemplateAsync(id, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating template {id}");
            return StatusCode(500, ApiResponse<EmailTemplateDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Email template sil
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(int id)
    {
        try
        {
            var result = await _emailService.DeleteTemplateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting template {id}");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Veli ödeme bilgilendirme maili gönder
    /// </summary>
    [HttpPost("notifications/veli-odeme/{studentId}/{paymentId}")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendVeliOdemeBilgilendirme(int studentId, int paymentId)
    {
        try
        {
            var result = await _emailService.SendVeliOdemeBilgilendirmeAsync(studentId, paymentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment notification");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Ödev bildirimi gönder
    /// </summary>
    [HttpPost("notifications/odev/{homeworkId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendOdevBildirimi(int homeworkId)
    {
        try
        {
            var result = await _emailService.SendOdevBildirimiAsync(homeworkId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending homework notification");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Sınav sonucu bildirimi gönder
    /// </summary>
    [HttpPost("notifications/sinav-sonuc/{examResultId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendSinavSonucBildirimi(int examResultId)
    {
        try
        {
            var result = await _emailService.SendSinavSonucBildirimiAsync(examResultId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending exam result notification");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Devamsızlık uyarısı gönder
    /// </summary>
    [HttpPost("notifications/devamsizlik/{studentId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendDevamsizlikUyari(int studentId)
    {
        try
        {
            var result = await _emailService.SendDevamsizlikUyariAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending absence warning");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Taksit hatırlatması gönder
    /// </summary>
    [HttpPost("notifications/taksit/{installmentId}")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendTaksitHatirlatma(int installmentId)
    {
        try
        {
            var result = await _emailService.SendTaksitHatirlatmaAsync(installmentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending installment reminder");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sunucu hatası"));
        }
    }
}
