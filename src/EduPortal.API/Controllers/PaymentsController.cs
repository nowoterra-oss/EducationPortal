using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Payment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Constants;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Payment management endpoints
/// </summary>
[ApiController]
[Route("api/payments")]
[Produces("application/json")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments with pagination
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentSummaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _paymentService.GetAllPagedAsync(pageNumber, pageSize);
            var pagedResponse = new PagedResponse<PaymentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<PaymentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse("Ödemeler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(int id)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Ödeme bulunamadı"));
            }

            return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Ödeme alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create payment record
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.PaymentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] PaymentCreateDto paymentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResponse("Geçersiz veri"));
            }

            var payment = await _paymentService.CreateAsync(paymentDto);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id },
                ApiResponse<PaymentDto>.SuccessResponse(payment, "Ödeme başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Ödeme oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update payment
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission(Permissions.PaymentsProcess)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Update(int id, [FromBody] PaymentCreateDto paymentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResponse("Geçersiz veri"));
            }

            var payment = await _paymentService.UpdateAsync(id, paymentDto);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment, "Ödeme başarıyla güncellendi"));
        }
        catch (Exception ex) when (ex.Message == "Payment not found")
        {
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Ödeme bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Ödeme güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete payment
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(Permissions.PaymentsProcess)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _paymentService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme bulunamadı"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Ödeme silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student payments
    /// </summary>
    [HttpGet("student/{studentId}")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentSummaryDto>>>> GetByStudent(
        int studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _paymentService.GetByStudentPagedAsync(studentId, pageNumber, pageSize);
            var pagedResponse = new PagedResponse<PaymentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<PaymentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse("Öğrenci ödemeleri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get payments by status
    /// </summary>
    [HttpGet("status/{status}")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentSummaryDto>>>> GetByStatus(
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (!Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
            {
                return BadRequest(ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse(
                    $"Geçersiz durum. Geçerli durumlar: {string.Join(", ", Enum.GetNames<PaymentStatus>())}"));
            }

            var (items, totalCount) = await _paymentService.GetByStatusPagedAsync(parsedStatus, pageNumber, pageSize);
            var pagedResponse = new PagedResponse<PaymentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<PaymentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by status {Status}", status);
            return StatusCode(500, ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse("Ödemeler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get pending payments
    /// </summary>
    [HttpGet("pending")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentSummaryDto>>>> GetPending(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _paymentService.GetPendingPagedAsync(pageNumber, pageSize);
            var pagedResponse = new PagedResponse<PaymentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<PaymentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return StatusCode(500, ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse("Bekleyen ödemeler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get overdue payments
    /// </summary>
    [HttpGet("overdue")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PaymentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentSummaryDto>>>> GetOverdue(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _paymentService.GetOverduePagedAsync(pageNumber, pageSize);
            var pagedResponse = new PagedResponse<PaymentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<PaymentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue payments");
            return StatusCode(500, ApiResponse<PagedResponse<PaymentSummaryDto>>.ErrorResponse("Gecikmiş ödemeler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Process payment
    /// </summary>
    [HttpPost("{id}/process")]
    [RequirePermission(Permissions.PaymentsProcess)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment(int id)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(id);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment, "Ödeme başarıyla işlendi"));
        }
        catch (Exception ex) when (ex.Message == "Payment not found")
        {
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Ödeme bulunamadı"));
        }
        catch (Exception ex) when (ex.Message == "Payment already processed")
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse("Bu ödeme zaten işlenmiş"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("Ödeme işlenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Download payment receipt
    /// </summary>
    [HttpGet("{id}/receipt")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadReceipt(int id)
    {
        try
        {
            var receiptBytes = await _paymentService.GenerateReceiptAsync(id);
            if (receiptBytes == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Ödeme bulunamadı"));
            }

            return File(receiptBytes, "text/plain", $"makbuz-{id}.txt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Makbuz oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentStatisticsDto>>> GetStatistics()
    {
        try
        {
            var statistics = await _paymentService.GetStatisticsAsync();
            return Ok(ApiResponse<PaymentStatisticsDto>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics");
            return StatusCode(500, ApiResponse<PaymentStatisticsDto>.ErrorResponse("İstatistikler alınırken bir hata oluştu"));
        }
    }
}
