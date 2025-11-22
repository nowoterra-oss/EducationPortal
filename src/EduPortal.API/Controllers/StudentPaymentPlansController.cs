using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/student-payment-plans")]
[Authorize]
public class StudentPaymentPlansController : ControllerBase
{
    private readonly IStudentPaymentPlanService _service;

    public StudentPaymentPlansController(IStudentPaymentPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Muhasebe,Kayitci")]
    public async Task<ActionResult<IEnumerable<StudentPaymentPlanDto>>> GetAll()
    {
        var plans = await _service.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentPaymentPlanDto>> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        if (plan == null)
            return NotFound($"Student payment plan with ID {id} not found");

        return Ok(plan);
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<StudentPaymentPlanDto>>> GetByStudent(int studentId)
    {
        var plans = await _service.GetByStudentIdAsync(studentId);
        return Ok(plans);
    }

    [HttpGet("student/{studentId}/active")]
    public async Task<ActionResult<StudentPaymentPlanDto>> GetActiveByStudent(int studentId)
    {
        var plan = await _service.GetActiveByStudentIdAsync(studentId);
        if (plan == null)
            return NotFound($"No active payment plan found for student {studentId}");

        return Ok(plan);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Muhasebe,Kayitci")]
    public async Task<ActionResult<StudentPaymentPlanDto>> Create([FromBody] CreateStudentPaymentPlanDto dto)
    {
        try
        {
            var plan = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult> Cancel(int id, [FromBody] string reason)
    {
        var result = await _service.CancelAsync(id, reason);
        if (!result)
            return NotFound();

        return Ok(new { message = "Payment plan cancelled" });
    }

    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult> Complete(int id)
    {
        var result = await _service.CompleteAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Payment plan completed" });
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<object>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<IEnumerable<StudentPaymentPlanDto>>> GetOverdue()
    {
        var plans = await _service.GetOverduePlansAsync();
        return Ok(plans);
    }
}
