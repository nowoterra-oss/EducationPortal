using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentGroup;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/student-groups")]
[Authorize]
public class StudentGroupsController : ControllerBase
{
    private readonly IStudentGroupService _service;

    public StudentGroupsController(IStudentGroupService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tum gruplari getir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentGroupDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, includeInactive);
        return Ok(result);
    }

    /// <summary>
    /// Grup detayi getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StudentGroupDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Yeni grup olustur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci,Danisman")]
    public async Task<ActionResult<ApiResponse<StudentGroupDto>>> Create([FromBody] CreateStudentGroupDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Grup guncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci,Danisman")]
    public async Task<ActionResult<ApiResponse<StudentGroupDto>>> Update(int id, [FromBody] UpdateStudentGroupDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Grup sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Gruba ogrenci ekle
    /// </summary>
    [HttpPost("{id}/students")]
    [Authorize(Roles = "Admin,Kayitci,Danisman")]
    public async Task<ActionResult<ApiResponse<StudentGroupDto>>> AddStudents(int id, [FromBody] AddStudentsToGroupDto dto)
    {
        var result = await _service.AddStudentsAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Gruptan ogrenci cikar
    /// </summary>
    [HttpDelete("{id}/students")]
    [Authorize(Roles = "Admin,Kayitci,Danisman")]
    public async Task<ActionResult<ApiResponse<StudentGroupDto>>> RemoveStudents(int id, [FromBody] RemoveStudentsFromGroupDto dto)
    {
        var result = await _service.RemoveStudentsAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Ogrencinin gruplarini getir
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<ApiResponse<List<StudentGroupDto>>>> GetStudentGroups(int studentId)
    {
        var result = await _service.GetStudentGroupsAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Grup dersi olustur (cakisma kontrolu ile)
    /// </summary>
    [HttpPost("{id}/lessons")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<ApiResponse<GroupLessonScheduleDto>>> CreateLesson(int id, [FromBody] CreateGroupLessonDto dto)
    {
        dto.GroupId = id;
        var result = await _service.CreateGroupLessonAsync(dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetGroupLessons), new { id }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Grup derslerini getir
    /// </summary>
    [HttpGet("{id}/lessons")]
    public async Task<ActionResult<ApiResponse<List<GroupLessonScheduleDto>>>> GetGroupLessons(int id)
    {
        var result = await _service.GetGroupLessonsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Grup dersi cakisma kontrolu (ders olusturmadan once kontrol)
    /// </summary>
    [HttpPost("{id}/lessons/check-conflicts")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<ApiResponse<GroupLessonConflictCheckResult>>> CheckConflicts(int id, [FromBody] CreateGroupLessonDto dto)
    {
        dto.GroupId = id;
        var result = await _service.CheckGroupLessonConflictsAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Grup dersini iptal et
    /// </summary>
    [HttpPatch("lessons/{lessonId}/cancel")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelLesson(
        int lessonId,
        [FromQuery] bool cancelAll = true,
        [FromQuery] DateTime? cancelDate = null)
    {
        var result = await _service.CancelGroupLessonAsync(lessonId, cancelAll, cancelDate);
        return Ok(result);
    }

    /// <summary>
    /// Ogretmenin grup derslerini getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/lessons")]
    public async Task<ActionResult<ApiResponse<List<GroupLessonScheduleDto>>>> GetTeacherGroupLessons(int teacherId)
    {
        var result = await _service.GetTeacherGroupLessonsAsync(teacherId);
        return Ok(result);
    }
}
