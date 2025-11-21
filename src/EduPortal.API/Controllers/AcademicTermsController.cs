using EduPortal.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Akademik dönem yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AcademicTermsController : ControllerBase
{
    // TODO: Implement IAcademicTermService
    private readonly ILogger<AcademicTermsController> _logger;

    public AcademicTermsController(ILogger<AcademicTermsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tüm akademik dönemleri listele
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <returns>Sayfalanmış dönem listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<object>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.GetAll called but service not implemented yet");
        return Ok(ApiResponse<PagedResponse<object>>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// ID ile dönem detayı getir
    /// </summary>
    /// <param name="id">Dönem ID</param>
    /// <returns>Dönem detayları</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> GetById(int id)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.GetById called but service not implemented yet");
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Aktif akademik dönemi getir
    /// </summary>
    /// <returns>Aktif dönem bilgileri</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetActive()
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.GetActive called but service not implemented yet");
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Yeni akademik dönem oluştur (Admin only)
    /// </summary>
    /// <param name="createDto">Dönem bilgileri</param>
    /// <returns>Oluşturulan dönem</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] object createDto)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.Create called but service not implemented yet");
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Akademik dönem bilgilerini güncelle (Admin only)
    /// </summary>
    /// <param name="id">Dönem ID</param>
    /// <param name="updateDto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş dönem</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] object updateDto)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.Update called but service not implemented yet");
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Akademik dönemi aktif et (Admin only)
    /// </summary>
    /// <param name="id">Dönem ID</param>
    /// <returns>Aktif edilen dönem</returns>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.Activate called but service not implemented yet");
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Akademik dönemi sil (Admin only)
    /// </summary>
    /// <param name="id">Dönem ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        // TODO: Implement service
        _logger.LogWarning("AcademicTermsController.Delete called but service not implemented yet");
        return Ok(ApiResponse<bool>.ErrorResponse("Servis henüz implement edilmedi"));
    }
}
