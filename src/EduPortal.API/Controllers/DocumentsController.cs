using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Document;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Document management endpoints
/// </summary>
[ApiController]
[Route("api/documents")]
[Produces("application/json")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<DocumentDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _documentService.GetAllPagedAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<DocumentDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<DocumentDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belgeler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<DocumentDto>>.ErrorResponse("Belgeler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GetById(int id)
    {
        try
        {
            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
                return NotFound(ApiResponse<DocumentDto>.ErrorResponse("Belge bulunamadı"));

            return Ok(ApiResponse<DocumentDto>.SuccessResponse(document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge getirilirken hata oluştu. ID: {DocumentId}", id);
            return StatusCode(500, ApiResponse<DocumentDto>.ErrorResponse("Belge getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Upload document
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Upload([FromBody] CreateDocumentDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<DocumentDto>.ErrorResponse("Geçersiz veri"));

            var document = await _documentService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = document.Id },
                ApiResponse<DocumentDto>.SuccessResponse(document, "Belge başarıyla yüklendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge yüklenirken hata oluştu");
            return StatusCode(500, ApiResponse<DocumentDto>.ErrorResponse("Belge yüklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Update(int id, [FromBody] UpdateDocumentDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<DocumentDto>.ErrorResponse("Geçersiz veri"));

            var document = await _documentService.UpdateAsync(id, updateDto);

            return Ok(ApiResponse<DocumentDto>.SuccessResponse(document, "Belge başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<DocumentDto>.ErrorResponse("Belge bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge güncellenirken hata oluştu. ID: {DocumentId}", id);
            return StatusCode(500, ApiResponse<DocumentDto>.ErrorResponse("Belge güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _documentService.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Belge bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Belge başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge silinirken hata oluştu. ID: {DocumentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Belge silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get documents for a student
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var documents = await _documentService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.SuccessResponse(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğrenci belgeleri getirilirken hata oluştu. StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.ErrorResponse("Belgeler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get documents by type
    /// </summary>
    [HttpGet("type/{documentType}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<DocumentDto>>>> GetByType(
        DocumentType documentType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _documentService.GetByTypeAsync(documentType, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<DocumentDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<DocumentDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge türüne göre belgeler getirilirken hata oluştu. Type: {DocumentType}", documentType);
            return StatusCode(500, ApiResponse<PagedResponse<DocumentDto>>.ErrorResponse("Belgeler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Download document
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var result = await _documentService.DownloadAsync(id);

            if (result == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Belge bulunamadı"));

            var (fileContent, fileName, contentType) = result.Value;

            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge indirilirken hata oluştu. ID: {DocumentId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Belge indirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Share document with user
    /// </summary>
    [HttpPost("{id}/share")]
    [Authorize(Roles = "Admin,Danışman,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<DocumentShareResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DocumentShareResultDto>>> Share(int id, [FromBody] ShareDocumentDto shareDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<DocumentShareResultDto>.ErrorResponse("Geçersiz veri"));

            var result = await _documentService.ShareAsync(id, shareDto);

            return Ok(ApiResponse<DocumentShareResultDto>.SuccessResponse(result, "Belge başarıyla paylaşıldı"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentShareResultDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge paylaşılırken hata oluştu. DocumentId: {DocumentId}", id);
            return StatusCode(500, ApiResponse<DocumentShareResultDto>.ErrorResponse("Belge paylaşılırken bir hata oluştu"));
        }
    }
}
