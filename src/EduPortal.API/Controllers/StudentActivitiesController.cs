using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentActivity;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Ogrenci aktiviteleri yonetimi (Yaz Aktiviteleri, Stajlar, Sosyal Sorumluluk Projeleri)
/// </summary>
[ApiController]
[Route("api/student-activities")]
[Produces("application/json")]
[Authorize]
public class StudentActivitiesController : ControllerBase
{
    private readonly IStudentActivityService _activityService;
    private readonly ILogger<StudentActivitiesController> _logger;

    public StudentActivitiesController(
        IStudentActivityService activityService,
        ILogger<StudentActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    #region Summer Activities

    /// <summary>
    /// Ogrencinin yaz aktivitelerini getirir
    /// </summary>
    [HttpGet("summer/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentSummerActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentSummerActivityDto>>>> GetSummerActivities(int studentId)
    {
        var result = await _activityService.GetSummerActivitiesByStudentAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Yaz aktivitesi detayini getirir
    /// </summary>
    [HttpGet("summer/detail/{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentSummerActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentSummerActivityDto>>> GetSummerActivityById(int id)
    {
        var result = await _activityService.GetSummerActivityByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Yeni yaz aktivitesi ekler
    /// </summary>
    [HttpPost("summer")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentSummerActivityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentSummerActivityDto>>> CreateSummerActivity(
        [FromBody] CreateStudentSummerActivityDto dto)
    {
        var result = await _activityService.CreateSummerActivityAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetSummerActivityById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Yaz aktivitesini gunceller
    /// </summary>
    [HttpPut("summer")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentSummerActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentSummerActivityDto>>> UpdateSummerActivity(
        [FromBody] UpdateStudentSummerActivityDto dto)
    {
        var result = await _activityService.UpdateSummerActivityAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Yaz aktivitesini siler
    /// </summary>
    [HttpDelete("summer/{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSummerActivity(int id)
    {
        var result = await _activityService.DeleteSummerActivityAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    #endregion

    #region Internships

    /// <summary>
    /// Ogrencinin stajlarini getirir
    /// </summary>
    [HttpGet("internships/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentInternshipDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentInternshipDto>>>> GetInternships(int studentId)
    {
        var result = await _activityService.GetInternshipsByStudentAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Staj detayini getirir
    /// </summary>
    [HttpGet("internships/detail/{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentInternshipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentInternshipDto>>> GetInternshipById(int id)
    {
        var result = await _activityService.GetInternshipByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Yeni staj ekler
    /// </summary>
    [HttpPost("internships")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentInternshipDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentInternshipDto>>> CreateInternship(
        [FromBody] CreateStudentInternshipDto dto)
    {
        var result = await _activityService.CreateInternshipAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetInternshipById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Staji gunceller
    /// </summary>
    [HttpPut("internships")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentInternshipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentInternshipDto>>> UpdateInternship(
        [FromBody] UpdateStudentInternshipDto dto)
    {
        var result = await _activityService.UpdateInternshipAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Staji siler
    /// </summary>
    [HttpDelete("internships/{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteInternship(int id)
    {
        var result = await _activityService.DeleteInternshipAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    #endregion

    #region Social Projects

    /// <summary>
    /// Ogrencinin sosyal sorumluluk projelerini getirir
    /// </summary>
    [HttpGet("social-projects/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentSocialProjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentSocialProjectDto>>>> GetSocialProjects(int studentId)
    {
        var result = await _activityService.GetSocialProjectsByStudentAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Sosyal sorumluluk projesi detayini getirir
    /// </summary>
    [HttpGet("social-projects/detail/{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentSocialProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentSocialProjectDto>>> GetSocialProjectById(int id)
    {
        var result = await _activityService.GetSocialProjectByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Yeni sosyal sorumluluk projesi ekler
    /// </summary>
    [HttpPost("social-projects")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentSocialProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentSocialProjectDto>>> CreateSocialProject(
        [FromBody] CreateStudentSocialProjectDto dto)
    {
        var result = await _activityService.CreateSocialProjectAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetSocialProjectById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Sosyal sorumluluk projesini gunceller
    /// </summary>
    [HttpPut("social-projects")]
    [Authorize(Roles = "Admin,Kayitci,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<StudentSocialProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentSocialProjectDto>>> UpdateSocialProject(
        [FromBody] UpdateStudentSocialProjectDto dto)
    {
        var result = await _activityService.UpdateSocialProjectAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sosyal sorumluluk projesini siler
    /// </summary>
    [HttpDelete("social-projects/{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSocialProject(int id)
    {
        var result = await _activityService.DeleteSocialProjectAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    #endregion
}
