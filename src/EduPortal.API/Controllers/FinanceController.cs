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
[Route("api/finance")]
[Produces("application/json")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;
    private readonly ILogger<FinanceController> _logger;

    public FinanceController(IFinanceService financeService, ILogger<FinanceController> logger)
    {
        _financeService = financeService;
        _logger = logger;
    }

    #region Finance Records

    [HttpGet("records")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FinanceRecordDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FinanceRecordDto>>>> GetAllRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] FinanceType? type = null,
        [FromQuery] FinanceCategory? category = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _financeService.GetAllPagedAsync(pageNumber, pageSize, type, category, startDate, endDate);
            return Ok(ApiResponse<PagedResult<FinanceRecordDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans kayıtları getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResult<FinanceRecordDto>>.ErrorResponse("Finans kayıtları alınırken bir hata oluştu"));
        }
    }

    [HttpGet("records/{id}")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FinanceRecordDto>>> GetRecordById(int id)
    {
        try
        {
            var record = await _financeService.GetByIdAsync(id);
            if (record == null)
                return NotFound(ApiResponse<FinanceRecordDto>.ErrorResponse("Kayıt bulunamadı"));

            return Ok(ApiResponse<FinanceRecordDto>.SuccessResponse(record));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans kaydı getirilirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<FinanceRecordDto>.ErrorResponse("Finans kaydı alınırken bir hata oluştu"));
        }
    }

    [HttpPost("records")]
    [RequirePermission(Permissions.FinanceCreate)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FinanceRecordDto>>> CreateRecord([FromBody] FinanceRecordCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<FinanceRecordDto>.ErrorResponse("Geçersiz veri"));

            var record = await _financeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetRecordById), new { id = record.Id },
                ApiResponse<FinanceRecordDto>.SuccessResponse(record, "Kayıt başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans kaydı oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<FinanceRecordDto>.ErrorResponse("Finans kaydı oluşturulurken bir hata oluştu"));
        }
    }

    [HttpPut("records/{id}")]
    [RequirePermission(Permissions.FinanceEdit)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinanceRecordDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FinanceRecordDto>>> UpdateRecord(int id, [FromBody] FinanceRecordCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<FinanceRecordDto>.ErrorResponse("Geçersiz veri"));

            var record = await _financeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<FinanceRecordDto>.SuccessResponse(record, "Kayıt başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<FinanceRecordDto>.ErrorResponse("Kayıt bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans kaydı güncellenirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<FinanceRecordDto>.ErrorResponse("Finans kaydı güncellenirken bir hata oluştu"));
        }
    }

    [HttpDelete("records/{id}")]
    [RequirePermission(Permissions.FinanceDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRecord(int id)
    {
        try
        {
            var result = await _financeService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Kayıt bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Kayıt başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans kaydı silinirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Finans kaydı silinirken bir hata oluştu"));
        }
    }

    [HttpGet("records/range")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<List<FinanceRecordDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FinanceRecordDto>>>> GetRecordsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] FinanceType? type = null)
    {
        try
        {
            var records = await _financeService.GetByDateRangeAsync(startDate, endDate, type);
            return Ok(ApiResponse<List<FinanceRecordDto>>.SuccessResponse(records));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarih aralığına göre finans kayıtları getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<List<FinanceRecordDto>>.ErrorResponse("Finans kayıtları alınırken bir hata oluştu"));
        }
    }

    #endregion

    #region Recurring Expenses

    [HttpGet("recurring-expenses")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RecurringExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RecurringExpenseDto>>>> GetAllRecurringExpenses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var result = await _financeService.GetRecurringExpensesPagedAsync(pageNumber, pageSize, isActive);
            return Ok(ApiResponse<PagedResult<RecurringExpenseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli giderler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResult<RecurringExpenseDto>>.ErrorResponse("Düzenli giderler alınırken bir hata oluştu"));
        }
    }

    [HttpGet("recurring-expenses/{id}")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RecurringExpenseDto>>> GetRecurringExpenseById(int id)
    {
        try
        {
            var record = await _financeService.GetRecurringExpenseByIdAsync(id);
            if (record == null)
                return NotFound(ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider bulunamadı"));

            return Ok(ApiResponse<RecurringExpenseDto>.SuccessResponse(record));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli gider getirilirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider alınırken bir hata oluştu"));
        }
    }

    [HttpPost("recurring-expenses")]
    [RequirePermission(Permissions.FinanceCreate)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RecurringExpenseDto>>> CreateRecurringExpense([FromBody] RecurringExpenseCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<RecurringExpenseDto>.ErrorResponse("Geçersiz veri"));

            var record = await _financeService.CreateRecurringExpenseAsync(dto);
            return CreatedAtAction(nameof(GetRecurringExpenseById), new { id = record.Id },
                ApiResponse<RecurringExpenseDto>.SuccessResponse(record, "Düzenli gider başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli gider oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider oluşturulurken bir hata oluştu"));
        }
    }

    [HttpPut("recurring-expenses/{id}")]
    [RequirePermission(Permissions.FinanceEdit)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RecurringExpenseDto>>> UpdateRecurringExpense(int id, [FromBody] RecurringExpenseCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<RecurringExpenseDto>.ErrorResponse("Geçersiz veri"));

            var record = await _financeService.UpdateRecurringExpenseAsync(id, dto);
            return Ok(ApiResponse<RecurringExpenseDto>.SuccessResponse(record, "Düzenli gider başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli gider güncellenirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider güncellenirken bir hata oluştu"));
        }
    }

    [HttpDelete("recurring-expenses/{id}")]
    [RequirePermission(Permissions.FinanceDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRecurringExpense(int id)
    {
        try
        {
            var result = await _financeService.DeleteRecurringExpenseAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Düzenli gider bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Düzenli gider başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli gider silinirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Düzenli gider silinirken bir hata oluştu"));
        }
    }

    [HttpPost("recurring-expenses/{id}/toggle")]
    [RequirePermission(Permissions.FinanceEdit)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RecurringExpenseDto>>> ToggleRecurringExpense(int id)
    {
        try
        {
            var record = await _financeService.ToggleRecurringExpenseAsync(id);
            var message = record.IsActive ? "Düzenli gider aktif edildi" : "Düzenli gider pasif edildi";
            return Ok(ApiResponse<RecurringExpenseDto>.SuccessResponse(record, message));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Düzenli gider durumu değiştirilirken hata oluştu. Id: {Id}", id);
            return StatusCode(500, ApiResponse<RecurringExpenseDto>.ErrorResponse("Düzenli gider durumu değiştirilirken bir hata oluştu"));
        }
    }

    #endregion

    #region Statistics

    [HttpGet("statistics")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<FinanceStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FinanceStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var statistics = await _financeService.GetStatisticsAsync(startDate, endDate);
            return Ok(ApiResponse<FinanceStatisticsDto>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finans istatistikleri getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<FinanceStatisticsDto>.ErrorResponse("Finans istatistikleri alınırken bir hata oluştu"));
        }
    }

    [HttpGet("categories")]
    [RequirePermission(Permissions.FinanceView)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetCategories()
    {
        try
        {
            var incomeCategories = new[] { "OgrenciOdemesi", "BagisHibe", "DigerGelir" };
            var expenseCategories = new[] { "Maas", "Kira", "Fatura", "Malzeme", "Bakim", "DigerGider" };

            var result = new
            {
                FinanceTypes = Enum.GetNames<FinanceType>(),
                IncomeCategories = incomeCategories,
                ExpenseCategories = expenseCategories,
                AllCategories = Enum.GetNames<FinanceCategory>(),
                RecurrenceTypes = Enum.GetNames<RecurrenceType>(),
                PaymentMethods = Enum.GetNames<PaymentMethod>()
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Kategoriler alınırken bir hata oluştu"));
        }
    }

    #endregion
}
