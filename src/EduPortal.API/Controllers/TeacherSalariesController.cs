using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Finance;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Constants;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/teacher-salaries")]
[Produces("application/json")]
[Authorize]
public class TeacherSalariesController : ControllerBase
{
    private readonly ITeacherSalaryService _salaryService;
    private readonly ILogger<TeacherSalariesController> _logger;

    public TeacherSalariesController(ITeacherSalaryService salaryService, ILogger<TeacherSalariesController> logger)
    {
        _salaryService = salaryService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TeacherSalaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TeacherSalaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] SalaryStatus? status = null)
    {
        try
        {
            var result = await _salaryService.GetAllPagedAsync(pageNumber, pageSize, year, month, status);
            return Ok(ApiResponse<PagedResult<TeacherSalaryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaşlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResult<TeacherSalaryDto>>.ErrorResponse("Maaşlar alınırken bir hata oluştu"));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TeacherSalaryDto>>> GetById(int id)
    {
        try
        {
            var salary = await _salaryService.GetByIdAsync(id);
            if (salary == null)
                return NotFound(ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı bulunamadı"));

            return Ok(ApiResponse<TeacherSalaryDto>.SuccessResponse(salary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş kaydı getirilirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı alınırken bir hata oluştu"));
        }
    }

    [HttpGet("teacher/{teacherId}")]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherSalaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TeacherSalaryDto>>>> GetByTeacher(int teacherId)
    {
        try
        {
            var salaries = await _salaryService.GetByTeacherAsync(teacherId);
            return Ok(ApiResponse<List<TeacherSalaryDto>>.SuccessResponse(salaries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğretmen maaşları getirilirken hata oluştu. TeacherId: {TeacherId}", teacherId);
            return StatusCode(500, ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Öğretmen maaşları alınırken bir hata oluştu"));
        }
    }

    [HttpGet("my-salaries")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherSalaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TeacherSalaryDto>>>> GetMySalaries()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Kullanıcı kimliği bulunamadı"));

            var salaries = await _salaryService.GetByTeacherAsync(int.Parse(userId));
            return Ok(ApiResponse<List<TeacherSalaryDto>>.SuccessResponse(salaries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kendi maaşlarını getirirken hata oluştu");
            return StatusCode(500, ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Maaşlarınız alınırken bir hata oluştu"));
        }
    }

    [HttpPost]
    [RequirePermission(Permissions.SalariesCreate)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TeacherSalaryDto>>> Create([FromBody] TeacherSalaryCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TeacherSalaryDto>.ErrorResponse("Geçersiz veri"));

            var salary = await _salaryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = salary.Id },
                ApiResponse<TeacherSalaryDto>.SuccessResponse(salary, "Maaş kaydı başarıyla oluşturuldu"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TeacherSalaryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş kaydı oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı oluşturulurken bir hata oluştu"));
        }
    }

    [HttpPost("bulk")]
    [RequirePermission(Permissions.SalariesCreate)]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherSalaryDto>>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<List<TeacherSalaryDto>>>> CreateBulk([FromBody] TeacherSalaryBulkCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Geçersiz veri"));

            var salaries = await _salaryService.CreateBulkAsync(dto);
            return CreatedAtAction(nameof(GetAll), null,
                ApiResponse<List<TeacherSalaryDto>>.SuccessResponse(salaries, $"{salaries.Count} adet maaş kaydı başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu maaş kaydı oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Toplu maaş kaydı oluşturulurken bir hata oluştu"));
        }
    }

    [HttpPut("{id}")]
    [RequirePermission(Permissions.SalariesProcess)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TeacherSalaryDto>>> Update(int id, [FromBody] TeacherSalaryCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TeacherSalaryDto>.ErrorResponse("Geçersiz veri"));

            var salary = await _salaryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<TeacherSalaryDto>.SuccessResponse(salary, "Maaş kaydı başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı bulunamadı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TeacherSalaryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş kaydı güncellenirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı güncellenirken bir hata oluştu"));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.SalariesProcess)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _salaryService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Maaş kaydı bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Maaş kaydı başarıyla silindi"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş kaydı silinirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Maaş kaydı silinirken bir hata oluştu"));
        }
    }

    [HttpPost("{id}/pay")]
    [RequirePermission(Permissions.SalariesProcess)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeacherSalaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TeacherSalaryDto>>> PaySalary(int id, [FromBody] TeacherSalaryPayDto dto)
    {
        try
        {
            var salary = await _salaryService.PaySalaryAsync(id, dto);
            return Ok(ApiResponse<TeacherSalaryDto>.SuccessResponse(salary, "Maaş başarıyla ödendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş kaydı bulunamadı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TeacherSalaryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş ödenirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<TeacherSalaryDto>.ErrorResponse("Maaş ödenirken bir hata oluştu"));
        }
    }

    [HttpGet("pending")]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherSalaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TeacherSalaryDto>>>> GetPending()
    {
        try
        {
            var salaries = await _salaryService.GetPendingAsync();
            return Ok(ApiResponse<List<TeacherSalaryDto>>.SuccessResponse(salaries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bekleyen maaşlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Bekleyen maaşlar alınırken bir hata oluştu"));
        }
    }

    [HttpGet("overdue")]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherSalaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TeacherSalaryDto>>>> GetOverdue()
    {
        try
        {
            var salaries = await _salaryService.GetOverdueAsync();
            return Ok(ApiResponse<List<TeacherSalaryDto>>.SuccessResponse(salaries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gecikmiş maaşlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<List<TeacherSalaryDto>>.ErrorResponse("Gecikmiş maaşlar alınırken bir hata oluştu"));
        }
    }

    [HttpGet("statistics")]
    [RequirePermission(Permissions.SalariesView)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetStatistics()
    {
        try
        {
            var totalPending = await _salaryService.GetTotalPendingAsync();
            var currentMonth = DateTime.Now;
            var paidThisMonth = await _salaryService.GetTotalPaidForMonthAsync(currentMonth.Year, currentMonth.Month);

            var stats = new
            {
                TotalPending = totalPending,
                PaidThisMonth = paidThisMonth,
                CurrentYear = currentMonth.Year,
                CurrentMonth = currentMonth.Month
            };

            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş istatistikleri getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Maaş istatistikleri alınırken bir hata oluştu"));
        }
    }
}
