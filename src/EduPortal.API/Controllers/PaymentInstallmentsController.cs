using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/payment-installments")]
[Produces("application/json")]
[Authorize]
public class PaymentInstallmentsController : ControllerBase
{
    private readonly IPaymentInstallmentService _service;
    private readonly ILogger<PaymentInstallmentsController> _logger;

    public PaymentInstallmentsController(IPaymentInstallmentService service, ILogger<PaymentInstallmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentInstallmentDto>> GetById(int id)
    {
        var installment = await _service.GetByIdAsync(id);
        if (installment == null)
            return NotFound($"Installment with ID {id} not found");

        return Ok(installment);
    }

    [HttpGet("plan/{planId}")]
    public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByPlan(int planId)
    {
        var installments = await _service.GetByStudentPaymentPlanAsync(planId);
        return Ok(installments);
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByStudent(int studentId)
    {
        var installments = await _service.GetByStudentIdAsync(studentId);
        return Ok(installments);
    }

    /// <summary>
    /// Velinin çocuklarının taksitlerini getirir
    /// </summary>
    [HttpGet("parent/{parentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentInstallmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentInstallmentDto>>>> GetByParent(int parentId)
    {
        var installments = await _service.GetByParentIdAsync(parentId);
        return Ok(ApiResponse<IEnumerable<PaymentInstallmentDto>>.SuccessResponse(installments));
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetOverdue()
    {
        var installments = await _service.GetOverdueAsync();
        return Ok(installments);
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetUpcoming([FromQuery] int days = 7)
    {
        var installments = await _service.GetUpcomingAsync(days);
        return Ok(installments);
    }

    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<PaymentInstallmentDto>> Pay(int id, [FromBody] PayInstallmentDto dto)
    {
        try
        {
            var installment = await _service.PayInstallmentAsync(id, dto);
            return Ok(installment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<object>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    #region Dekont İşlemleri

    /// <summary>
    /// Taksit için dekont yükler (Veli)
    /// </summary>
    [HttpPost("{id}/upload-receipt")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentDto>>> UploadReceipt(
        int id,
    IFormFile file,
        [FromForm] string? notes)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<PaymentInstallmentDto>.ErrorResponse("Kullanıcı kimliği bulunamadı"));

            // Veli için studentId'yi al
            // Not: Gerçek senaryoda veli-öğrenci ilişkisinden studentId alınmalı
            // Şimdilik userId'yi kullanıyoruz
            var installment = await _service.GetByIdAsync(id);
            if (installment == null)
                return NotFound(ApiResponse<PaymentInstallmentDto>.ErrorResponse("Taksit bulunamadı"));

            var result = await _service.UploadReceiptAsync(id, file, notes, installment.StudentId);
            return Ok(ApiResponse<PaymentInstallmentDto>.SuccessResponse(result, "Dekont başarıyla yüklendi"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dekont yüklenirken hata oluştu. InstallmentId: {InstallmentId}", id);
            return StatusCode(500, ApiResponse<PaymentInstallmentDto>.ErrorResponse("Dekont yüklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Dekont dosyasını getirir
    /// </summary>
    [HttpGet("{id}/receipt")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceipt(int id)
    {
        try
        {
            var result = await _service.GetReceiptAsync(id);
            if (!result.HasValue)
                return NotFound(ApiResponse<object>.ErrorResponse("Dekont bulunamadı"));

            var (fileContent, contentType, fileName) = result.Value;
            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dekont getirilirken hata oluştu. InstallmentId: {InstallmentId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Dekont getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Onay bekleyen taksitleri listeler (Admin)
    /// </summary>
    [HttpGet("pending-approval")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentInstallmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentInstallmentDto>>>> GetPendingApproval(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _service.GetPendingApprovalAsync(page, pageSize);
            return Ok(ApiResponse<PagedResult<PaymentInstallmentDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onay bekleyen taksitler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResult<PaymentInstallmentDto>>.ErrorResponse("Onay bekleyen taksitler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Taksiti onaylar (Admin)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentDto>>> Approve(
        int id,
        [FromBody] ApproveInstallmentDto? dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<PaymentInstallmentDto>.ErrorResponse("Kullanıcı kimliği bulunamadı"));

            var result = await _service.ApproveInstallmentAsync(id, userId, dto?.Notes);
            return Ok(ApiResponse<PaymentInstallmentDto>.SuccessResponse(result, "Ödeme başarıyla onaylandı"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taksit onaylanırken hata oluştu. InstallmentId: {InstallmentId}", id);
            return StatusCode(500, ApiResponse<PaymentInstallmentDto>.ErrorResponse("Taksit onaylanırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Taksiti reddeder (Admin)
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentDto>>> Reject(
        int id,
        [FromBody] RejectInstallmentDto dto)
    {
        try
        {
            var result = await _service.RejectInstallmentAsync(id, dto.Reason);
            return Ok(ApiResponse<PaymentInstallmentDto>.SuccessResponse(result, "Ödeme reddedildi"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taksit reddedilirken hata oluştu. InstallmentId: {InstallmentId}", id);
            return StatusCode(500, ApiResponse<PaymentInstallmentDto>.ErrorResponse("Taksit reddedilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Debug: Tüm taksitlerin durumlarını kontrol eder
    /// </summary>
    [HttpGet("debug-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DebugStatus()
    {
        var all = await _service.GetDebugStatusAsync();
        return Ok(all);
    }

    /// <summary>
    /// Elden ödeme kaydeder (Admin) - Nakit, POS veya banka transferi ile yapılan ödemeler için
    /// </summary>
    [HttpPost("{id}/cash-payment")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInstallmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentDto>>> CashPayment(
        int id,
        [FromBody] CashPaymentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<PaymentInstallmentDto>.ErrorResponse("Kullanıcı kimliği bulunamadı"));

            var result = await _service.MarkAsCashPaymentAsync(id, userId, dto.Notes, dto.PaymentMethod);
            return Ok(ApiResponse<PaymentInstallmentDto>.SuccessResponse(result, "Elden ödeme başarıyla kaydedildi"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentInstallmentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elden ödeme kaydedilirken hata oluştu. InstallmentId: {InstallmentId}", id);
            return StatusCode(500, ApiResponse<PaymentInstallmentDto>.ErrorResponse("Elden ödeme kaydedilirken bir hata oluştu"));
        }
    }

    #endregion
}
