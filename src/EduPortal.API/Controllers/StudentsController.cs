using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Student;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Student management endpoints
/// </summary>
[ApiController]
[Route("api/students")]
[Produces("application/json")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all students with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of students</returns>
    /// <response code="200">Students retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _studentService.GetAllAsync(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all students");
            return StatusCode(500, ApiResponse<PagedResponse<StudentDto>>.ErrorResponse("Öğrenciler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student details</returns>
    /// <response code="200">Student retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Student not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        try
        {
            var result = await _studentService.GetByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting student by ID: {StudentId}", id);
            return StatusCode(500, ApiResponse<StudentDto>.ErrorResponse("Öğrenci bilgisi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Search students by name, email, or student number
    /// </summary>
    /// <param name="term">Search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Matching students</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("search")]
    [Authorize(Roles = "Admin,Danışman,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentDto>>>> Search(
        [FromQuery] string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _studentService.SearchAsync(term);
            var pagedResponse = new PagedResponse<StudentDto>(
                result.Data ?? new List<StudentDto>(),
                result.Data?.Count ?? 0,
                pageNumber,
                pageSize
            );
            return Ok(ApiResponse<PagedResponse<StudentDto>>.SuccessResponse(pagedResponse, "Arama tamamlandı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching students with term: {SearchTerm}", term);
            return StatusCode(500, ApiResponse<PagedResponse<StudentDto>>.ErrorResponse("Öğrenci arama sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student by student number
    /// </summary>
    /// <param name="studentNo">Student number</param>
    /// <returns>Student details</returns>
    /// <response code="200">Student retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Student not found</response>
    [HttpGet("student-no/{studentNo}")]
    [Authorize(Roles = "Admin,Danışman,Öğretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetByStudentNo(string studentNo)
    {
        try
        {
            var result = await _studentService.GetByStudentNoAsync(studentNo);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting student by student number: {StudentNo}", studentNo);
            return StatusCode(500, ApiResponse<StudentDto>.ErrorResponse("Öğrenci bilgisi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="studentCreateDto">Student creation data</param>
    /// <returns>Created student</returns>
    /// <response code="201">Student created successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayıtçı")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] StudentCreateDto studentCreateDto)
    {
        try
        {
            var result = await _studentService.CreateAsync(studentCreateDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating student");
            return StatusCode(500, ApiResponse<StudentDto>.ErrorResponse("Öğrenci oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update student information
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="studentUpdateDto">Updated student data</param>
    /// <returns>Updated student</returns>
    /// <response code="200">Student updated successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Student not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(int id, [FromBody] StudentUpdateDto studentUpdateDto)
    {
        try
        {
            studentUpdateDto.Id = id;
            var result = await _studentService.UpdateAsync(studentUpdateDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message.Contains("bulunamadı") ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating student: {StudentId}", id);
            return StatusCode(500, ApiResponse<StudentDto>.ErrorResponse("Öğrenci güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete student
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Student deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Student not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _studentService.DeleteAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting student: {StudentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Öğrenci silinirken bir hata oluştu"));
        }
    }

    // TODO: Implement GetProfileAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get detailed student profile with all information
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Detailed student profile</returns>
    // /// <response code="200">Profile retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/profile")]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<object>>> GetProfile(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetProfileAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting student profile: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse("Öğrenci profili getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetAcademicHistoryAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student academic history
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Academic history records</returns>
    // /// <response code="200">Academic history retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/academic-history")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetAcademicHistory(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetAcademicHistoryAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting academic history for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Akademik geçmiş getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetHobbiesAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student hobbies
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of student hobbies</returns>
    // /// <response code="200">Hobbies retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/hobbies")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetHobbies(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetHobbiesAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting hobbies for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Hobiler getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetClubsAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student club memberships
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of club memberships</returns>
    // /// <response code="200">Clubs retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/clubs")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetClubs(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetClubsAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting clubs for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Kulüpler getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetExamsAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student exam results
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of exam results</returns>
    // /// <response code="200">Exam results retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/exams")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetExams(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetExamsAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting exams for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Sınavlar getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetInternationalExamsAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student international exam results
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of international exam results</returns>
    // /// <response code="200">International exams retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/international-exams")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetInternationalExams(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetInternationalExamsAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting international exams for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Uluslararası sınavlar getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetCompetitionsAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student competitions and awards
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of competitions and awards</returns>
    // /// <response code="200">Competitions retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/competitions")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetCompetitions(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetCompetitionsAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting competitions for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Yarışmalar getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetDocumentsAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student documents
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of student documents</returns>
    // /// <response code="200">Documents retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/documents")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetDocuments(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetDocumentsAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting documents for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Belgeler getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetWeeklyScheduleAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student weekly schedule
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Weekly schedule</returns>
    // /// <response code="200">Schedule retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/weekly-schedule")]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<object>>> GetWeeklySchedule(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetWeeklyScheduleAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting weekly schedule for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse("Haftalık program getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetTeachersAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student's teachers
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>List of teachers</returns>
    // /// <response code="200">Teachers retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/teachers")]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<List<object>>>> GetTeachers(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetTeachersAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting teachers for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Öğretmenler getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetPerformanceSummaryAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student performance summary
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Performance summary</returns>
    // /// <response code="200">Performance summary retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/performance")]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<object>>> GetPerformanceSummary(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetPerformanceSummaryAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting performance summary for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse("Performans özeti getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetAttendanceSummaryAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student attendance summary
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Attendance summary</returns>
    // /// <response code="200">Attendance summary retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/attendance-summary")]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<object>>> GetAttendanceSummary(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetAttendanceSummaryAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting attendance summary for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse("Devamsızlık özeti getirilirken bir hata oluştu"));
    //     }
    // }

    // TODO: Implement GetHomeworkPerformanceAsync in IStudentService and StudentService before uncommenting
    // /// <summary>
    // /// Get student homework performance
    // /// </summary>
    // /// <param name="id">Student ID</param>
    // /// <returns>Homework performance data</returns>
    // /// <response code="200">Homework performance retrieved successfully</response>
    // /// <response code="401">Unauthorized</response>
    // /// <response code="404">Student not found</response>
    // [HttpGet("{id}/homework-performance")]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<ApiResponse<object>>> GetHomeworkPerformance(int id)
    // {
    //     try
    //     {
    //         var result = await _studentService.GetHomeworkPerformanceAsync(id);
    //
    //         if (result.Success)
    //         {
    //             return Ok(result);
    //         }
    //
    //         return NotFound(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while getting homework performance for student: {StudentId}", id);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse("Ödev performansı getirilirken bir hata oluştu"));
    //     }
    // }
}
