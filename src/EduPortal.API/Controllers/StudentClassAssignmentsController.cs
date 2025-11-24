using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentClassAssignment;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Öğrenci-sınıf atama yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class StudentClassAssignmentsController : ControllerBase
{
    private readonly IStudentClassAssignmentService _assignmentService;
    private readonly ILogger<StudentClassAssignmentsController> _logger;

    public StudentClassAssignmentsController(
        IStudentClassAssignmentService assignmentService,
        ILogger<StudentClassAssignmentsController> logger)
    {
        _assignmentService = assignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm öğrenci-sınıf atamalarını listele
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <param name="isActive">Aktiflik durumu filtresi (opsiyonel)</param>
    /// <returns>Sayfalanmış atama listesi</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentClassAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentClassAssignmentDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var (items, totalCount) = await _assignmentService.GetAllPagedAsync(pageNumber, pageSize, isActive);
            var response = new PagedResponse<StudentClassAssignmentDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<StudentClassAssignmentDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student class assignments");
            return StatusCode(500, ApiResponse<PagedResponse<StudentClassAssignmentDto>>.ErrorResponse("Atamalar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile atama detayı getir
    /// </summary>
    /// <param name="id">Atama ID</param>
    /// <returns>Atama detayları</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentClassAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentClassAssignmentDto>>> GetById(int id)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync(id);
            if (assignment == null)
                return NotFound(ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Atama bulunamadı"));

            return Ok(ApiResponse<StudentClassAssignmentDto>.SuccessResponse(assignment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
            return StatusCode(500, ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Atama alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğrencinin sınıf atamalarını getir
    /// </summary>
    /// <param name="studentId">Öğrenci ID</param>
    /// <returns>Öğrencinin sınıf geçmişi</returns>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentClassAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentClassAssignmentDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var assignments = await _assignmentService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<List<StudentClassAssignmentDto>>.SuccessResponse(assignments.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<StudentClassAssignmentDto>>.ErrorResponse("Öğrenci atamaları alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıfın öğrenci atamalarını getir
    /// </summary>
    /// <param name="classId">Sınıf ID</param>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <returns>Sınıfın öğrencileri</returns>
    [HttpGet("class/{classId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentClassAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentClassAssignmentDto>>>> GetByClass(
        int classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _assignmentService.GetByClassAsync(classId, pageNumber, pageSize);
            var response = new PagedResponse<StudentClassAssignmentDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<StudentClassAssignmentDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for class {ClassId}", classId);
            return StatusCode(500, ApiResponse<PagedResponse<StudentClassAssignmentDto>>.ErrorResponse("Sınıf atamaları alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni öğrenci-sınıf ataması oluştur (Admin/Kayıtçı only)
    /// </summary>
    /// <param name="dto">Atama bilgileri</param>
    /// <returns>Oluşturulan atama</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<StudentClassAssignmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentClassAssignmentDto>>> Create([FromBody] CreateStudentClassAssignmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Geçersiz veri"));

            var assignment = await _assignmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id },
                ApiResponse<StudentClassAssignmentDto>.SuccessResponse(assignment, "Atama başarıyla oluşturuldu"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StudentClassAssignmentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student class assignment");
            return StatusCode(500, ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Atama oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Toplu öğrenci-sınıf ataması oluştur (Admin/Kayıtçı only)
    /// </summary>
    /// <param name="dtos">Atama listesi</param>
    /// <returns>Oluşturulan atamalar</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentClassAssignmentDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<StudentClassAssignmentDto>>>> CreateBulk([FromBody] List<CreateStudentClassAssignmentDto> dtos)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<StudentClassAssignmentDto>>.ErrorResponse("Geçersiz veri"));

            if (dtos == null || dtos.Count == 0)
                return BadRequest(ApiResponse<List<StudentClassAssignmentDto>>.ErrorResponse("Atama listesi boş olamaz"));

            var assignments = await _assignmentService.CreateBulkAsync(dtos);
            return CreatedAtAction(nameof(GetAll), null,
                ApiResponse<List<StudentClassAssignmentDto>>.SuccessResponse(assignments.ToList(), $"{assignments.Count()} atama başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk student class assignments");
            return StatusCode(500, ApiResponse<List<StudentClassAssignmentDto>>.ErrorResponse("Toplu atama oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Atama bilgilerini güncelle (Admin/Kayıtçı only)
    /// </summary>
    /// <param name="id">Atama ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş atama</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<StudentClassAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentClassAssignmentDto>>> Update(int id, [FromBody] UpdateStudentClassAssignmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Geçersiz veri"));

            var assignment = await _assignmentService.UpdateAsync(id, dto);
            return Ok(ApiResponse<StudentClassAssignmentDto>.SuccessResponse(assignment, "Atama başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Atama bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment {AssignmentId}", id);
            return StatusCode(500, ApiResponse<StudentClassAssignmentDto>.ErrorResponse("Atama güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Atamayı sil (Admin only)
    /// </summary>
    /// <param name="id">Atama ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _assignmentService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Atama bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Atama başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment {AssignmentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Atama silinirken bir hata oluştu"));
        }
    }
}
