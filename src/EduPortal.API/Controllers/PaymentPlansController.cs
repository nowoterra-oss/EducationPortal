using EduPortal.Application.Common;
using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/payment-plans")]
[Produces("application/json")]
[Authorize]
public class PaymentPlansController : ControllerBase
{
    private readonly IPaymentPlanService _service;
    private readonly ILogger<PaymentPlansController> _logger;

    public PaymentPlansController(IPaymentPlanService service, ILogger<PaymentPlansController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentPlanDto>>>> GetAll()
    {
        var plans = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PaymentPlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentPlanDto>>>> GetActive()
    {
        var plans = await _service.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<PaymentPlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentPlanDto>>> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        if (plan == null)
            return NotFound(ApiResponse<PaymentPlanDto>.ErrorResponse($"ID {id} olan ödeme planı bulunamadı"));

        return Ok(ApiResponse<PaymentPlanDto>.SuccessResponse(plan));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentPlanDto>>> Create([FromBody] CreatePaymentPlanDto dto)
    {
        try
        {
            var plan = await _service.CreateAsync(dto);
            return Ok(ApiResponse<PaymentPlanDto>.SuccessResponse(plan, "Ödeme planı başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme planı oluşturulurken hata oluştu");
            return BadRequest(ApiResponse<PaymentPlanDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentPlanDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentPlanDto>>> Update(int id, [FromBody] CreatePaymentPlanDto dto)
    {
        try
        {
            var plan = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<PaymentPlanDto>.SuccessResponse(plan, "Ödeme planı başarıyla güncellendi"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentPlanDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme planı güncellenirken hata oluştu. Id: {Id}", id);
            return BadRequest(ApiResponse<PaymentPlanDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme planı bulunamadı"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme planı başarıyla silindi"));
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Activate(int id)
    {
        var result = await _service.ActivateAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme planı bulunamadı"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme planı aktif edildi"));
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Deactivate(int id)
    {
        var result = await _service.DeactivateAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme planı bulunamadı"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme planı pasif edildi"));
    }
}
