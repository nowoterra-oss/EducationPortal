using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Ders kaynaklari yonetimi
/// </summary>
[ApiController]
[Route("api/course-resources")]
[Produces("application/json")]
[Authorize]
public class CourseResourcesController : ControllerBase
{
    private readonly ICourseResourceService _resourceService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CourseResourcesController> _logger;

    public CourseResourcesController(
        ICourseResourceService resourceService,
        IFileStorageService fileStorageService,
        ILogger<CourseResourcesController> logger)
    {
        _resourceService = resourceService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Tum ders kaynaklarini listele
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResourceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CourseResourceDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _resourceService.GetAllAsync(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all course resources");
            return StatusCode(500, ApiResponse<PagedResponse<CourseResourceDto>>.ErrorResponse("Kaynaklar getirilirken hata olustu"));
        }
    }

    /// <summary>
    /// Belirli bir derse ait kaynaklari listele
    /// </summary>
    [HttpGet("course/{courseId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResourceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CourseResourceDto>>>> GetByCourse(
        int courseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _resourceService.GetByCourseAsync(courseId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resources for course {CourseId}", courseId);
            return StatusCode(500, ApiResponse<PagedResponse<CourseResourceDto>>.ErrorResponse("Ders kaynaklari getirilirken hata olustu"));
        }
    }

    /// <summary>
    /// Kaynak detayi getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> GetById(int id)
    {
        try
        {
            var result = await _resourceService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resource {Id}", id);
            return StatusCode(500, ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak getirilirken hata olustu"));
        }
    }

    /// <summary>
    /// Yeni kaynak olustur (dosya yukleme destekli)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> Create([FromForm] CourseResourceCreateFormDto formDto)
    {
        try
        {
            string resourceUrl = formDto.ResourceUrl ?? string.Empty;

            // Dosya yuklenmisse, once dosyayi yukle
            if (formDto.File != null && formDto.File.Length > 0)
            {
                var uploadResult = await _fileStorageService.UploadFileAsync(formDto.File, "course-resources", formDto.CourseId.ToString());
                if (!uploadResult.Success)
                {
                    return BadRequest(ApiResponse<CourseResourceDto>.ErrorResponse(uploadResult.Message ?? "Dosya yuklenemedi"));
                }
                resourceUrl = uploadResult.Data?.FileUrl ?? string.Empty;
            }

            if (string.IsNullOrEmpty(resourceUrl))
            {
                return BadRequest(ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak URL veya dosya belirtilmelidir"));
            }

            var createDto = new CreateCourseResourceDto
            {
                CurriculumId = formDto.CurriculumId,
                Title = formDto.Title,
                ResourceType = formDto.ResourceType,
                ResourceUrl = resourceUrl,
                Description = formDto.Description,
                IsVisible = formDto.IsVisible
            };

            var result = await _resourceService.CreateAsync(formDto.CourseId, createDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course resource");
            return StatusCode(500, ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak olusturulurken hata olustu"));
        }
    }

    /// <summary>
    /// Kaynak guncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> Update(int id, [FromBody] CreateCourseResourceDto dto)
    {
        try
        {
            var result = await _resourceService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message?.Contains("bulunamadi") == true ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course resource {Id}", id);
            return StatusCode(500, ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak guncellenirken hata olustu"));
        }
    }

    /// <summary>
    /// Kaynak sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _resourceService.DeleteAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course resource {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Kaynak silinirken hata olustu"));
        }
    }

    /// <summary>
    /// Kaynak indirme bilgisi getir
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> Download(int id)
    {
        try
        {
            var result = await _resourceService.GetForDownloadAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resource for download {Id}", id);
            return StatusCode(500, ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak getirilirken hata olustu"));
        }
    }
}

/// <summary>
/// Form data ile kaynak olusturmak icin DTO
/// </summary>
public class CourseResourceCreateFormDto
{
    public int CourseId { get; set; }
    public int? CurriculumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceUrl { get; set; }
    public string? Description { get; set; }
    public bool IsVisible { get; set; } = true;
    public IFormFile? File { get; set; }
}
