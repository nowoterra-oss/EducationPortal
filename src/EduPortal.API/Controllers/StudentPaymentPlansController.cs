using EduPortal.Application.Common;
using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/student-payment-plans")]
[Produces("application/json")]
[Authorize]
public class StudentPaymentPlansController : ControllerBase
{
    private readonly IStudentPaymentPlanService _service;
    private readonly ILogger<StudentPaymentPlansController> _logger;

    public StudentPaymentPlansController(IStudentPaymentPlanService service, ILogger<StudentPaymentPlansController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Muhasebe,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentPaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentPlanDto>>>> GetAll()
    {
        var plans = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StudentPaymentPlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentPaymentPlanDto>>> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        if (plan == null)
            return NotFound(ApiResponse<StudentPaymentPlanDto>.ErrorResponse($"ID {id} olan öğrenci ödeme planı bulunamadı"));

        return Ok(ApiResponse<StudentPaymentPlanDto>.SuccessResponse(plan));
    }

    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentPaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentPlanDto>>>> GetByStudent(int studentId)
    {
        var plans = await _service.GetByStudentIdAsync(studentId);
        return Ok(ApiResponse<IEnumerable<StudentPaymentPlanDto>>.SuccessResponse(plans));
    }

    [HttpGet("student/{studentId}/active")]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentPaymentPlanDto>>> GetActiveByStudent(int studentId)
    {
        var plan = await _service.GetActiveByStudentIdAsync(studentId);
        if (plan == null)
            return NotFound(ApiResponse<StudentPaymentPlanDto>.ErrorResponse($"Öğrenci {studentId} için aktif ödeme planı bulunamadı"));

        return Ok(ApiResponse<StudentPaymentPlanDto>.SuccessResponse(plan));
    }

    /// <summary>
    /// Velinin çocuklarının ödeme planlarını getirir
    /// </summary>
    [HttpGet("parent/{parentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentPaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentPlanDto>>>> GetByParent(int parentId)
    {
        var plans = await _service.GetByParentIdAsync(parentId);
        return Ok(ApiResponse<IEnumerable<StudentPaymentPlanDto>>.SuccessResponse(plans));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Muhasebe,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentPaymentPlanDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentPaymentPlanDto>>> Create([FromBody] CreateStudentPaymentPlanDto dto)
    {
        try
        {
            var plan = await _service.CreateAsync(dto);
            return Ok(ApiResponse<StudentPaymentPlanDto>.SuccessResponse(plan, "Öğrenci ödeme planı başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğrenci ödeme planı oluşturulurken hata oluştu. StudentId: {StudentId}", dto.StudentId);
            return BadRequest(ApiResponse<StudentPaymentPlanDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Cancel(int id, [FromBody] string reason)
    {
        var result = await _service.CancelAsync(id, reason);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme planı bulunamadı"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme planı iptal edildi"));
    }

    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Complete(int id)
    {
        var result = await _service.CompleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Ödeme planı bulunamadı"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ödeme planı tamamlandı"));
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Muhasebe")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentPaymentPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentPlanDto>>>> GetOverdue()
    {
        var plans = await _service.GetOverduePlansAsync();
        return Ok(ApiResponse<IEnumerable<StudentPaymentPlanDto>>.SuccessResponse(plans));
    }
}
