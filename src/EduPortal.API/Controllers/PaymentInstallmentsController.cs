using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/payment-installments")]
[Authorize]
public class PaymentInstallmentsController : ControllerBase
{
    private readonly IPaymentInstallmentService _service;

    public PaymentInstallmentsController(IPaymentInstallmentService service)
    {
        _service = service;
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
}
