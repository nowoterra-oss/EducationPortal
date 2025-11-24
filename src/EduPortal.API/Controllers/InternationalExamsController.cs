using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// International exam (SAT, TOEFL, IELTS, etc.) management endpoints
/// </summary>
[ApiController]
[Route("api/exams/international")]
[Produces("application/json")]
[Authorize]
public class InternationalExamsController : ControllerBase
{
    private readonly IInternationalExamService _examService;
    private readonly ILogger<InternationalExamsController> _logger;

    public InternationalExamsController(
        IInternationalExamService examService,
        ILogger<InternationalExamsController> logger)
    {
        _examService = examService;
        _logger = logger;
    }

    /// <summary>
    /// Get all international exam records with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InternationalExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<InternationalExamDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _examService.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get international exam record by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InternationalExamDto>>> GetById(int id)
    {
        var result = await _examService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create international exam record
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InternationalExamDto>>> Create([FromBody] InternationalExamCreateDto examDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<InternationalExamDto>.ErrorResponse("Geçersiz veri"));
        }

        var result = await _examService.CreateAsync(examDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update international exam record
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InternationalExamDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InternationalExamDto>>> Update(int id, [FromBody] InternationalExamUpdateDto examDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<InternationalExamDto>.ErrorResponse("Geçersiz veri"));
        }

        var result = await _examService.UpdateAsync(id, examDto);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete international exam record
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _examService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get international exam records for a student
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<InternationalExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<InternationalExamDto>>>> GetByStudent(int studentId)
    {
        var result = await _examService.GetByStudentAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Get exam records by type (SAT, TOEFL, etc.)
    /// </summary>
    [HttpGet("type/{examType}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InternationalExamDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InternationalExamDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResponse<InternationalExamDto>>>> GetByType(
        string examType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!Enum.TryParse<ExamType>(examType, true, out var parsedExamType))
        {
            return BadRequest(ApiResponse<PagedResponse<InternationalExamDto>>.ErrorResponse(
                $"Geçersiz sınav tipi. Geçerli tipler: {string.Join(", ", Enum.GetNames<ExamType>())}"));
        }

        var result = await _examService.GetByExamTypeAsync(parsedExamType, pageNumber, pageSize);
        return Ok(result);
    }
}
