using System.Security.Claims;
using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Certificate;
using EduPortal.Application.DTOs.Student;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Constants;
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
    private readonly IStudentCertificateService _certificateService;
    private readonly IStudentExtendedInfoService _extendedInfoService;
    private readonly IStudentRepository _studentRepository;
    private readonly IAdvisorAccessService _advisorAccessService;
    private readonly IParentAccessService _parentAccessService;
    private readonly ILogger<StudentsController> _logger;
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public StudentsController(
        IStudentService studentService,
        IStudentCertificateService certificateService,
        IStudentExtendedInfoService extendedInfoService,
        IStudentRepository studentRepository,
        IAdvisorAccessService advisorAccessService,
        IParentAccessService parentAccessService,
        ILogger<StudentsController> logger,
        IWebHostEnvironment environment)
    {
        _studentService = studentService;
        _certificateService = certificateService;
        _extendedInfoService = extendedInfoService;
        _studentRepository = studentRepository;
        _advisorAccessService = advisorAccessService;
        _parentAccessService = parentAccessService;
        _logger = logger;
        _environment = environment;
    }

    private async Task<(string? url, string? fileName, string? error)> SaveCertificateFileAsync(IFormFile? file, int studentId, string folder)
    {
        if (file == null || file.Length == 0)
            return (null, null, null);

        if (file.Length > MaxFileSize)
            return (null, null, "Dosya boyutu 10 MB'ı aşamaz");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (null, null, "Desteklenmeyen dosya formatı. Desteklenen formatlar: PDF, JPG, JPEG, PNG, DOC, DOCX");

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "files", folder);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{studentId}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/files/{folder}/{uniqueFileName}", file.FileName, null);
    }

    /// <summary>
    /// Get all students with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="includeInactive">Include inactive students (default: false)</param>
    /// <returns>Paginated list of students</returns>
    /// <response code="200">Students retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpGet]
    [RequirePermission(Permissions.StudentsView, Permissions.UsersView, Permissions.AgpView, Permissions.AgpCreate, Permissions.AgpEdit, Permissions.SchedulingView, Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var result = await _studentService.GetAllAsync(pageNumber, pageSize, includeInactive);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all students");
            return StatusCode(500, ApiResponse<PagedResponse<StudentDto>>.ErrorResponse("Öğrenciler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get current student's own profile (no permission required)
    /// </summary>
    /// <returns>Current student details</returns>
    /// <remarks>
    /// Öğrenciler bu endpoint ile kendi profillerini yetki kontrolü olmadan görüntüleyebilir.
    /// </remarks>
    /// <response code="200">Student profile retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Student not found</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<StudentDto>.ErrorResponse("Kullanıcı kimliği bulunamadı"));
            }

            // Kullanıcının öğrenci kaydını bul
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
            {
                return NotFound(ApiResponse<StudentDto>.ErrorResponse("Öğrenci kaydı bulunamadı"));
            }

            // Öğrenci bilgilerini getir
            var result = await _studentService.GetByIdAsync(student.Id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current student profile");
            return StatusCode(500, ApiResponse<StudentDto>.ErrorResponse("Profil bilgisi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student details</returns>
    /// <remarks>
    /// Danışman öğretmenler sadece kendilerine atanmış öğrencilerin bilgilerini görüntüleyebilir.
    /// Veliler sadece kendilerine bağlı öğrencilerin bilgilerini görüntüleyebilir.
    /// </remarks>
    /// <response code="200">Student retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Advisor can only access assigned students</response>
    /// <response code="404">Student not found</response>
    [HttpGet("{id}")]
    [RequirePermission(Permissions.StudentsView, Permissions.UsersView, Permissions.AgpView, Permissions.AgpCreate, Permissions.AgpEdit, Permissions.SchedulingView, Permissions.SchedulingCreate, Permissions.ParentStudentView, Permissions.AdvisorStudentView)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Tam erişim yetkisi olanlar (admin, StudentsView vb.) tüm öğrencilere erişebilir
                var hasFullAccess = User.HasClaim("permission", Permissions.StudentsView) ||
                                    User.HasClaim("permission", Permissions.UsersView) ||
                                    User.HasClaim("permission", Permissions.AgpView) ||
                                    User.HasClaim("permission", Permissions.AgpCreate) ||
                                    User.HasClaim("permission", Permissions.AgpEdit) ||
                                    User.HasClaim("permission", Permissions.SchedulingView) ||
                                    User.HasClaim("permission", Permissions.SchedulingCreate);

                if (!hasFullAccess)
                {
                    // Veli erişim kontrolü - veli sadece kendi öğrencisine erişebilir
                    var isParent = await _parentAccessService.IsParentAsync(userId);
                    if (isParent)
                    {
                        var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, id);
                        if (!canAccessAsParent)
                        {
                            return StatusCode(StatusCodes.Status403Forbidden,
                                ApiResponse<StudentDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                        }
                    }
                    else
                    {
                        // Danışman öğretmen erişim kontrolü
                        var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, id);
                        if (!canAccess)
                        {
                            return StatusCode(StatusCodes.Status403Forbidden,
                                ApiResponse<StudentDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                        }
                    }
                }
            }

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
    /// <param name="includeInactive">Include inactive students (default: false)</param>
    /// <returns>Matching students</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("search")]
    [RequirePermission(Permissions.StudentsView, Permissions.UsersView, Permissions.AgpView, Permissions.AgpCreate, Permissions.AgpEdit, Permissions.SchedulingView, Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentDto>>>> Search(
        [FromQuery] string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var result = await _studentService.SearchAsync(term, includeInactive);
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
    [RequirePermission(Permissions.StudentsView, Permissions.UsersView, Permissions.AgpView, Permissions.AgpCreate, Permissions.AgpEdit, Permissions.SchedulingView, Permissions.SchedulingCreate)]
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
    /// Oluşturulacak bir sonraki öğrenci numarasını önizle
    /// </summary>
    /// <returns>Sonraki öğrenci numarası</returns>
    /// <response code="200">Öğrenci numarası başarıyla alındı</response>
    [HttpGet("next-student-no")]
    [RequirePermission(Permissions.StudentsCreate, Permissions.UsersCreate)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetNextStudentNo()
    {
        try
        {
            var result = await _studentService.GetNextStudentNoPreviewAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next student number preview");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Öğrenci numarası önizlemesi alınırken hata oluştu"));
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
    [RequirePermission(Permissions.StudentsCreate, Permissions.UsersCreate)]
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
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(int id, [FromBody] StudentUpdateDto studentUpdateDto)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Forbid();

            // Öğrenci rolü için kısıtlama: sadece kendi kaydını güncelleyebilir
            if (User.IsInRole("Ogrenci"))
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null || student.Id != id)
                    return Forbid();

                // Öğrenci sadece email ve address alanlarını güncelleyebilir
                studentUpdateDto = new StudentUpdateDto
                {
                    Id = id,
                    Email = studentUpdateDto.Email,
                    Address = studentUpdateDto.Address
                };
            }
            else
            {
                // Admin her zaman yetkili, diğerleri için students.edit veya users.edit yetkisi gerekli
                if (!User.IsInRole("Admin"))
                {
                    var permissionService = HttpContext.RequestServices.GetRequiredService<IPermissionService>();
                    var hasStudentsEdit = await permissionService.HasPermissionAsync(currentUserId, Permissions.StudentsEdit);
                    var hasUsersEdit = await permissionService.HasPermissionAsync(currentUserId, Permissions.UsersEdit);
                    if (!hasStudentsEdit && !hasUsersEdit)
                    {
                        return Forbid();
                    }
                }
                studentUpdateDto.Id = id;
            }

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
    [RequirePermission(Permissions.StudentsDelete, Permissions.UsersDelete)]
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

    // ===============================================
    // STUDENT CERTIFICATES
    // ===============================================

    /// <summary>
    /// Get student certificates
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of certificates</returns>
    [HttpGet("{studentId}/certificates")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentCertificateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentCertificateDto>>>> GetCertificates(int studentId)
    {
        var result = await _certificateService.GetByStudentIdAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Upload a new certificate
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="file">Certificate file (PDF, JPG, PNG - max 5MB)</param>
    /// <param name="name">Certificate name</param>
    /// <param name="description">Description (optional)</param>
    /// <param name="issueDate">Issue date (optional)</param>
    /// <param name="issuingOrganization">Issuing organization (optional)</param>
    [HttpPost("{studentId}/certificates")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<StudentCertificateUploadResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentCertificateUploadResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<StudentCertificateUploadResultDto>>> UploadCertificate(
        int studentId,
        IFormFile file,
        [FromForm] string name,
        [FromForm] string? description = null,
        [FromForm] DateTime? issueDate = null,
        [FromForm] string? issuingOrganization = null)
    {
        // Öğrenci sadece kendi sertifikasını yükleyebilir
        if (User.IsInRole("Ogrenci"))
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null || student.Id != studentId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<StudentCertificateUploadResultDto>.ErrorResponse("Sadece kendi sertifikanızı yükleyebilirsiniz."));
        }

        var dto = new StudentCertificateCreateDto
        {
            Name = name,
            Description = description,
            IssueDate = issueDate,
            IssuingOrganization = issuingOrganization
        };

        // Determine if added by admin based on user role
        var isAddedByAdmin = User.IsInRole("Admin") || User.IsInRole("Danışman");

        var result = await _certificateService.UploadAsync(studentId, file, dto, isAddedByAdmin);
        if (result.Success)
            return CreatedAtAction(nameof(GetCertificates), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Delete a certificate
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="certificateId">Certificate ID</param>
    /// <remarks>
    /// Admin can delete any certificate. Students can only delete certificates they added themselves (IsAddedByAdmin = false).
    /// </remarks>
    [HttpDelete("{studentId}/certificates/{certificateId}")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCertificate(int studentId, int certificateId)
    {
        // If student role, check if they can delete this certificate
        if (User.IsInRole("Ogrenci"))
        {
            var certificate = await _certificateService.GetByIdAsync(studentId, certificateId);
            if (!certificate.Success)
                return NotFound(ApiResponse<bool>.ErrorResponse("Sertifika bulunamadı."));

            // Student can only delete certificates they added themselves
            if (certificate.Data?.IsAddedByAdmin == true)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<bool>.ErrorResponse("Admin tarafından eklenen sertifikaları silemezsiniz."));
        }

        var result = await _certificateService.DeleteAsync(studentId, certificateId);
        return Ok(result);
    }

    /// <summary>
    /// Download/view a certificate file
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="certificateId">Certificate ID</param>
    [HttpGet("{studentId}/certificates/{certificateId}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCertificate(int studentId, int certificateId)
    {
        var (fileBytes, contentType, fileName) = await _certificateService.DownloadAsync(studentId, certificateId);

        if (fileBytes == null || contentType == null || fileName == null)
            return NotFound(ApiResponse<bool>.ErrorResponse("Sertifika dosyası bulunamadı."));

        return File(fileBytes, contentType, fileName);
    }

    // ===============================================
    // FOREIGN LANGUAGES
    // ===============================================

    /// <summary>
    /// Get student foreign languages
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of foreign languages</returns>
    [HttpGet("{studentId}/foreign-languages")]
    [ProducesResponseType(typeof(ApiResponse<List<ForeignLanguageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ForeignLanguageDto>>>> GetForeignLanguages(int studentId)
    {
        var result = await _extendedInfoService.GetForeignLanguagesAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Add a foreign language
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Foreign language data</param>
    [HttpPost("{studentId}/foreign-languages")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<ForeignLanguageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ForeignLanguageDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ForeignLanguageDto>>> AddForeignLanguage(
        int studentId,
        [FromBody] ForeignLanguageCreateDto dto)
    {
        var result = await _extendedInfoService.AddForeignLanguageAsync(studentId, dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetForeignLanguages), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Delete a foreign language
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="id">Foreign language ID</param>
    [HttpDelete("{studentId}/foreign-languages/{id}")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteForeignLanguage(int studentId, int id)
    {
        var result = await _extendedInfoService.DeleteForeignLanguageAsync(studentId, id);
        return Ok(result);
    }

    // ===============================================
    // HOBBIES
    // ===============================================

    /// <summary>
    /// Get student hobbies
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of hobbies</returns>
    [HttpGet("{studentId}/hobbies")]
    [ProducesResponseType(typeof(ApiResponse<List<HobbyDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HobbyDto>>>> GetHobbies(int studentId)
    {
        var result = await _extendedInfoService.GetHobbiesAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Add a hobby (JSON)
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Hobby data</param>
    [HttpPost("{studentId}/hobbies")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HobbyDto>>> AddHobby(
        int studentId,
        [FromBody] HobbyCreateDto dto)
    {
        var result = await _extendedInfoService.AddHobbyAsync(studentId, dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetHobbies), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Add a hobby with file upload (multipart/form-data)
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Hobby data with file</param>
    [HttpPost("{studentId}/hobbies/upload")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HobbyDto>>> AddHobbyWithFile(
        int studentId,
        [FromForm] HobbyCreateDto dto)
    {
        // Dosya yükleme
        var (certificateUrl, certificateFileName, error) = await SaveCertificateFileAsync(dto.CertificateFile, studentId, "hobbies");
        if (error != null)
            return BadRequest(ApiResponse<HobbyDto>.ErrorResponse(error));

        var result = await _extendedInfoService.AddHobbyAsync(studentId, dto, certificateUrl, certificateFileName);
        if (result.Success)
            return CreatedAtAction(nameof(GetHobbies), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Delete a hobby
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="id">Hobby ID</param>
    [HttpDelete("{studentId}/hobbies/{id}")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteHobby(int studentId, int id)
    {
        var result = await _extendedInfoService.DeleteHobbyAsync(studentId, id);
        return Ok(result);
    }

    // ===============================================
    // ACTIVITIES
    // ===============================================

    /// <summary>
    /// Get student activities
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of activities</returns>
    [HttpGet("{studentId}/activities")]
    [ProducesResponseType(typeof(ApiResponse<List<ActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ActivityDto>>>> GetActivities(int studentId)
    {
        var result = await _extendedInfoService.GetActivitiesAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Add an activity (JSON)
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Activity data</param>
    [HttpPost("{studentId}/activities")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<ActivityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ActivityDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ActivityDto>>> AddActivity(
        int studentId,
        [FromBody] ActivityCreateDto dto)
    {
        var result = await _extendedInfoService.AddActivityAsync(studentId, dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetActivities), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Add an activity with file upload (multipart/form-data)
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Activity data with file</param>
    [HttpPost("{studentId}/activities/upload")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ActivityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ActivityDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ActivityDto>>> AddActivityWithFile(
        int studentId,
        [FromForm] ActivityCreateDto dto)
    {
        // Dosya yükleme
        var (certificateUrl, certificateFileName, error) = await SaveCertificateFileAsync(dto.CertificateFile, studentId, "activities");
        if (error != null)
            return BadRequest(ApiResponse<ActivityDto>.ErrorResponse(error));

        var result = await _extendedInfoService.AddActivityAsync(studentId, dto, certificateUrl, certificateFileName);
        if (result.Success)
            return CreatedAtAction(nameof(GetActivities), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="id">Activity ID</param>
    [HttpDelete("{studentId}/activities/{id}")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteActivity(int studentId, int id)
    {
        var result = await _extendedInfoService.DeleteActivityAsync(studentId, id);
        return Ok(result);
    }

    // ===============================================
    // READINESS EXAMS
    // ===============================================

    /// <summary>
    /// Get student readiness exams
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of readiness exams</returns>
    [HttpGet("{studentId}/readiness-exams")]
    [ProducesResponseType(typeof(ApiResponse<List<ReadinessExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ReadinessExamDto>>>> GetReadinessExams(int studentId)
    {
        var result = await _extendedInfoService.GetReadinessExamsAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Add a readiness exam
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Readiness exam data</param>
    [HttpPost("{studentId}/readiness-exams")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<ReadinessExamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ReadinessExamDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReadinessExamDto>>> AddReadinessExam(
        int studentId,
        [FromBody] ReadinessExamCreateDto dto)
    {
        var result = await _extendedInfoService.AddReadinessExamAsync(studentId, dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetReadinessExams), new { studentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Delete a readiness exam
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="id">Readiness exam ID</param>
    [HttpDelete("{studentId}/readiness-exams/{id}")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReadinessExam(int studentId, int id)
    {
        var result = await _extendedInfoService.DeleteReadinessExamAsync(studentId, id);
        return Ok(result);
    }

    // ===============================================
    // PARENTS
    // ===============================================

    /// <summary>
    /// Get parents of a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of parents</returns>
    [HttpGet("{studentId}/parents")]
    [ProducesResponseType(typeof(ApiResponse<List<Application.DTOs.Parent.ParentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Application.DTOs.Parent.ParentDto>>>> GetParents(int studentId)
    {
        try
        {
            var parentService = HttpContext.RequestServices.GetRequiredService<IParentService>();
            var parents = await parentService.GetParentsByStudentIdAsync(studentId);
            return Ok(ApiResponse<IEnumerable<Application.DTOs.Parent.ParentDto>>.SuccessResponse(parents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parents for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<Application.DTOs.Parent.ParentDto>>.ErrorResponse("Veliler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create a new parent for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="dto">Parent creation data</param>
    /// <returns>Created parent</returns>
    [HttpPost("{studentId}/parents")]
    [RequirePermission(Permissions.StudentsEdit, Permissions.UsersEdit, Permissions.StudentsCreate, Permissions.UsersCreate)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Parent.ParentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Parent.ParentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Application.DTOs.Parent.ParentDto>>> CreateParent(
        int studentId,
        [FromBody] Application.DTOs.Parent.CreateParentForStudentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Application.DTOs.Parent.ParentDto>.ErrorResponse("Geçersiz veri"));
            }

            var parentService = HttpContext.RequestServices.GetRequiredService<IParentService>();
            var parent = await parentService.CreateParentForStudentAsync(studentId, dto);
            return CreatedAtAction(nameof(GetParents), new { studentId },
                ApiResponse<Application.DTOs.Parent.ParentDto>.SuccessResponse(parent, "Veli başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent for student {StudentId}", studentId);
            return BadRequest(ApiResponse<Application.DTOs.Parent.ParentDto>.ErrorResponse(ex.Message));
        }
    }
}
