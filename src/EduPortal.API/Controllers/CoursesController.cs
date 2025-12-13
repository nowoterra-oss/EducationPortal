using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Course management endpoints
/// </summary>
[ApiController]
[Route("api/courses")]
[Produces("application/json")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all courses with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CourseDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _courseService.GetAllPagedAsync(pageNumber, pageSize);
            var response = new PagedResponse<CourseDto>(items.ToList(), totalCount, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<CourseDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return StatusCode(500, ApiResponse<PagedResponse<CourseDto>>.ErrorResponse("Dersler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get course by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseDto>>> GetById(int id)
    {
        try
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
                return NotFound(ApiResponse<CourseDto>.ErrorResponse("Ders bulunamadı"));

            return Ok(ApiResponse<CourseDto>.SuccessResponse(course));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course {CourseId}", id);
            return StatusCode(500, ApiResponse<CourseDto>.ErrorResponse("Ders alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create new course
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Create([FromBody] CreateCourseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CourseDto>.ErrorResponse("Geçersiz veri"));

            var course = await _courseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id },
                ApiResponse<CourseDto>.SuccessResponse(course, "Ders başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(500, ApiResponse<CourseDto>.ErrorResponse("Ders oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update course
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Update(int id, [FromBody] UpdateCourseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CourseDto>.ErrorResponse("Geçersiz veri"));

            var course = await _courseService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CourseDto>.SuccessResponse(course, "Ders başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<CourseDto>.ErrorResponse("Ders bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", id);
            return StatusCode(500, ApiResponse<CourseDto>.ErrorResponse("Ders güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete course
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _courseService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Ders bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Ders başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Ders silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get course curriculum
    /// </summary>
    [HttpGet("{id}/curriculum")]
    [ProducesResponseType(typeof(ApiResponse<List<CurriculumDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CurriculumDto>>>> GetCurriculum(int id)
    {
        try
        {
            var curriculum = await _courseService.GetCurriculumAsync(id);
            return Ok(ApiResponse<List<CurriculumDto>>.SuccessResponse(curriculum.ToList()));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<List<CurriculumDto>>.ErrorResponse("Ders bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving curriculum for course {CourseId}", id);
            return StatusCode(500, ApiResponse<List<CurriculumDto>>.ErrorResponse("Müfredat alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update course curriculum
    /// </summary>
    [HttpPut("{id}/curriculum")]
    [Authorize(Roles = "Admin,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<List<CurriculumDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<CurriculumDto>>>> UpdateCurriculum(int id, [FromBody] UpdateCurriculumDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<CurriculumDto>>.ErrorResponse("Geçersiz veri"));

            var curriculum = await _courseService.UpdateCurriculumAsync(id, dto);
            return Ok(ApiResponse<List<CurriculumDto>>.SuccessResponse(curriculum.ToList(), "Müfredat başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<List<CurriculumDto>>.ErrorResponse("Ders bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating curriculum for course {CourseId}", id);
            return StatusCode(500, ApiResponse<List<CurriculumDto>>.ErrorResponse("Müfredat güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get course resources
    /// </summary>
    [HttpGet("{id}/resources")]
    [ProducesResponseType(typeof(ApiResponse<List<CourseResourceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CourseResourceDto>>>> GetResources(int id)
    {
        try
        {
            var resources = await _courseService.GetResourcesAsync(id);
            return Ok(ApiResponse<List<CourseResourceDto>>.SuccessResponse(resources.ToList()));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<List<CourseResourceDto>>.ErrorResponse("Ders bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resources for course {CourseId}", id);
            return StatusCode(500, ApiResponse<List<CourseResourceDto>>.ErrorResponse("Kaynaklar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Add course resource
    /// </summary>
    [HttpPost("{id}/resources")]
    [Authorize(Roles = "Admin,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<CourseResourceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResourceDto>>> AddResource(int id, [FromBody] CreateCourseResourceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CourseResourceDto>.ErrorResponse("Geçersiz veri"));

            var resource = await _courseService.AddResourceAsync(id, dto);
            return CreatedAtAction(nameof(GetResources), new { id = id },
                ApiResponse<CourseResourceDto>.SuccessResponse(resource, "Kaynak başarıyla eklendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<CourseResourceDto>.ErrorResponse("Ders bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding resource to course {CourseId}", id);
            return StatusCode(500, ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak eklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete course resource
    /// </summary>
    [HttpDelete("{id}/resources/{resourceId}")]
    [Authorize(Roles = "Admin,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteResource(int id, int resourceId)
    {
        try
        {
            var result = await _courseService.DeleteResourceAsync(id, resourceId);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Kaynak bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Kaynak başarıyla silindi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Ders veya kaynak bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource {ResourceId} from course {CourseId}", resourceId, id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Kaynak silinirken bir hata oluştu"));
        }
    }
}
