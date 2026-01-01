using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/homework-drafts")]
[Authorize]
public class HomeworkDraftsController : ControllerBase
{
    private readonly IHomeworkDraftService _service;
    private readonly ILogger<HomeworkDraftsController> _logger;

    public HomeworkDraftsController(IHomeworkDraftService service, ILogger<HomeworkDraftsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Öğretmenin tüm taslaklarını getir
    /// </summary>
    /// <param name="teacherId">Öğretmen ID (opsiyonel, belirtilmezse token'dan alınır)</param>
    /// <param name="isSent">Gönderilmiş mi? (null=tümü, true=gönderilenler, false=bekleyenler)</param>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<HomeworkDraftDto>>>> GetDrafts(
        [FromQuery] int? teacherId = null,
        [FromQuery] bool? isSent = null)
    {
        var effectiveTeacherId = teacherId ?? GetCurrentTeacherId();
        if (effectiveTeacherId <= 0)
            return BadRequest(ApiResponse<List<HomeworkDraftDto>>.ErrorResponse("Öğretmen ID gerekli"));

        var result = await _service.GetDraftsByTeacherAsync(effectiveTeacherId, isSent);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Belirli bir taslağı getir (ID ile)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HomeworkDraftDto>>> GetDraftById(int id)
    {
        var result = await _service.GetDraftByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Belirli bir taslağı getir (LessonId ile)
    /// </summary>
    /// <param name="lessonId">Frontend'deki lesson.id (schedule_1234 formatında)</param>
    /// <param name="teacherId">Öğretmen ID (opsiyonel)</param>
    [HttpGet("by-lesson/{lessonId}")]
    public async Task<ActionResult<ApiResponse<HomeworkDraftDto>>> GetDraftByLessonId(
        string lessonId,
        [FromQuery] int? teacherId = null)
    {
        var effectiveTeacherId = teacherId ?? GetCurrentTeacherId();
        if (effectiveTeacherId <= 0)
            return BadRequest(ApiResponse<HomeworkDraftDto>.ErrorResponse("Öğretmen ID gerekli"));

        var result = await _service.GetDraftByLessonIdAsync(effectiveTeacherId, lessonId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Yeni taslak oluştur veya mevcut taslağı güncelle (upsert)
    /// LessonId + TeacherId kombinasyonu unique olduğundan, aynı ders için çağrılırsa güncelleme yapılır
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<HomeworkDraftDto>>> CreateOrUpdateDraft(
        [FromBody] CreateHomeworkDraftDto dto,
        [FromQuery] int? teacherId = null)
    {
        var effectiveTeacherId = teacherId ?? GetCurrentTeacherId();
        if (effectiveTeacherId <= 0)
            return BadRequest(ApiResponse<HomeworkDraftDto>.ErrorResponse("Öğretmen ID gerekli"));

        var result = await _service.CreateOrUpdateDraftAsync(effectiveTeacherId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslağı güncelle (partial update)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<HomeworkDraftDto>>> UpdateDraft(
        int id,
        [FromBody] UpdateHomeworkDraftDto dto)
    {
        var result = await _service.UpdateDraftAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslağa ders içeriği dosyası yükle
    /// </summary>
    [HttpPost("{id}/upload-content")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
    public async Task<ActionResult<ApiResponse<DraftFileDto>>> UploadContentFile(
        int id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<DraftFileDto>.ErrorResponse("Dosya seçilmedi"));

        using var stream = file.OpenReadStream();
        var result = await _service.UploadContentFileAsync(id, stream, file.FileName, file.ContentType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslağa test dosyası yükle
    /// </summary>
    [HttpPost("{id}/upload-test")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
    public async Task<ActionResult<ApiResponse<DraftFileDto>>> UploadTestFile(
        int id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<DraftFileDto>.ErrorResponse("Dosya seçilmedi"));

        using var stream = file.OpenReadStream();
        var result = await _service.UploadTestFileAsync(id, stream, file.FileName, file.ContentType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslaktan dosya sil
    /// </summary>
    /// <param name="id">Taslak ID</param>
    /// <param name="downloadUrl">Silinecek dosyanın URL'si</param>
    /// <param name="fileType">Dosya tipi: content veya test</param>
    [HttpDelete("{id}/files")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFile(
        int id,
        [FromQuery] string downloadUrl,
        [FromQuery] string fileType)
    {
        if (string.IsNullOrEmpty(downloadUrl))
            return BadRequest(ApiResponse<bool>.ErrorResponse("Dosya URL'si gerekli"));

        if (fileType != "content" && fileType != "test")
            return BadRequest(ApiResponse<bool>.ErrorResponse("Geçersiz dosya tipi (content veya test olmalı)"));

        var result = await _service.RemoveFileAsync(id, downloadUrl, fileType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslağı öğrencilere gönder (HomeworkAssignment'lar oluşturulur)
    /// </summary>
    [HttpPost("{id}/send")]
    public async Task<ActionResult<ApiResponse<SendDraftResultDto>>> SendDraft(int id)
    {
        var result = await _service.SendDraftAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Taslağı sil (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDraft(int id)
    {
        var result = await _service.DeleteDraftAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private int GetCurrentTeacherId()
    {
        var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(teacherIdClaim, out var teacherId) ? teacherId : 0;
    }
}
