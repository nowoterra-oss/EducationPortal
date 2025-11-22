using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/payment-plans")]
[Authorize]
public class PaymentPlansController : ControllerBase
{
    private readonly IPaymentPlanService _service;

    public PaymentPlansController(IPaymentPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentPlanDto>>> GetAll()
    {
        var plans = await _service.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PaymentPlanDto>>> GetActive()
    {
        var plans = await _service.GetActiveAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentPlanDto>> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        if (plan == null)
            return NotFound($"Payment plan with ID {id} not found");

        return Ok(plan);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<PaymentPlanDto>> Create([FromBody] CreatePaymentPlanDto dto)
    {
        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<PaymentPlanDto>> Update(int id, [FromBody] CreatePaymentPlanDto dto)
    {
        try
        {
            var plan = await _service.UpdateAsync(id, dto);
            return Ok(plan);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult> Activate(int id)
    {
        var result = await _service.ActivateAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Payment plan activated" });
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult> Deactivate(int id)
    {
        var result = await _service.DeactivateAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Payment plan deactivated" });
    }
}
