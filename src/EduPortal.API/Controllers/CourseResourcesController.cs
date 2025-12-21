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
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CourseResourcesController> _logger;

    public CourseResourcesController(
        ICourseResourceService resourceService,
        IFileStorageService fileStorageService,
        IWebHostEnvironment environment,
        ILogger<CourseResourcesController> logger)
    {
        _resourceService = resourceService;
        _fileStorageService = fileStorageService;
        _environment = environment;
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
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> Create([FromForm] CourseResourceCreateFormDto formDto)
    {
        try
        {
            string? resourceUrl = formDto.ResourceUrl;
            string? filePath = null;
            string? fileName = null;
            long? fileSize = null;
            string? mimeType = null;

            // Dosya yuklenmisse
            if (formDto.File != null && formDto.File.Length > 0)
            {
                // Uploads klasorunu olustur
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "resources");
                Directory.CreateDirectory(uploadsFolder);

                // Benzersiz dosya adi olustur
                var uniqueFileName = $"{Guid.NewGuid()}_{formDto.File.FileName}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayi kaydet
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await formDto.File.CopyToAsync(stream);
                }

                filePath = uniqueFileName;
                fileName = formDto.File.FileName;
                fileSize = formDto.File.Length;
                mimeType = formDto.File.ContentType;
            }

            // Ne dosya ne de URL varsa hata ver
            if (string.IsNullOrEmpty(resourceUrl) && string.IsNullOrEmpty(filePath))
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

            var result = await _resourceService.CreateWithFileAsync(formDto.CourseId, createDto, filePath, fileName, fileSize, mimeType);

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
    /// Kaynak indirme bilgisi getir (DownloadUrl ve FileName doner)
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDownloadDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResourceDownloadDto>>> Download(int id)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _resourceService.GetDownloadInfoAsync(id, baseUrl);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resource for download {Id}", id);
            return StatusCode(500, ApiResponse<CourseResourceDownloadDto>.ErrorResponse("Kaynak getirilirken hata olustu"));
        }
    }

    /// <summary>
    /// Dosyayi binary stream olarak indir
    /// </summary>
    [HttpGet("{id}/file")]
    [HttpGet("{id}/download-file")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(int id)
    {
        try
        {
            var resource = await _resourceService.GetByIdAsync(id);
            if (!resource.Success || resource.Data == null)
            {
                return NotFound(new { success = false, message = "Kaynak bulunamadi" });
            }

            if (string.IsNullOrEmpty(resource.Data.FilePath))
            {
                return NotFound(new { success = false, message = "Bu kaynaga ait dosya yok" });
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "resources");
            var filePath = Path.Combine(uploadsFolder, resource.Data.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { success = false, message = "Dosya bulunamadi" });
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var contentType = resource.Data.MimeType ?? "application/octet-stream";
            var fileName = resource.Data.FileName ?? Path.GetFileName(resource.Data.FilePath);

            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file for resource {Id}", id);
            return StatusCode(500, new { success = false, message = "Dosya indirilirken hata olustu" });
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
