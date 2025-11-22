using EduPortal.Application.DTOs.StudentTeacherAssignment;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/student-teacher-assignments")]
[Authorize]
public class StudentTeacherAssignmentsController : ControllerBase
{
    private readonly IStudentTeacherAssignmentService _service;

    public StudentTeacherAssignmentsController(IStudentTeacherAssignmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all student-teacher assignments
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<IEnumerable<StudentTeacherAssignmentDto>>> GetAll()
    {
        var assignments = await _service.GetAllAsync();
        return Ok(assignments);
    }

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentTeacherAssignmentDto>> GetById(int id)
    {
        var assignment = await _service.GetByIdAsync(id);
        if (assignment == null)
            return NotFound($"Assignment with ID {id} not found");

        return Ok(assignment);
    }

    /// <summary>
    /// Get assignments by student ID
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<StudentTeacherAssignmentDto>>> GetByStudent(int studentId)
    {
        var assignments = await _service.GetByStudentIdAsync(studentId);
        return Ok(assignments);
    }

    /// <summary>
    /// Get assignments by teacher ID
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<IEnumerable<StudentTeacherAssignmentDto>>> GetByTeacher(int teacherId)
    {
        var assignments = await _service.GetByTeacherIdAsync(teacherId);
        return Ok(assignments);
    }

    /// <summary>
    /// Get assignments by course ID
    /// </summary>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Ogretmen,Kayitci")]
    public async Task<ActionResult<IEnumerable<StudentTeacherAssignmentDto>>> GetByCourse(int courseId)
    {
        var assignments = await _service.GetByCourseIdAsync(courseId);
        return Ok(assignments);
    }

    /// <summary>
    /// Create new assignment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<StudentTeacherAssignmentDto>> Create([FromBody] CreateStudentTeacherAssignmentDto dto)
    {
        var assignment = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
    }

    /// <summary>
    /// Create bulk assignments
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<IEnumerable<StudentTeacherAssignmentDto>>> CreateBulk([FromBody] List<CreateStudentTeacherAssignmentDto> dtos)
    {
        var assignments = await _service.CreateBulkAsync(dtos);
        return Ok(assignments);
    }

    /// <summary>
    /// Update assignment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult<StudentTeacherAssignmentDto>> Update(int id, [FromBody] UpdateStudentTeacherAssignmentDto dto)
    {
        try
        {
            var assignment = await _service.UpdateAsync(id, dto);
            return Ok(assignment);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete assignment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Deactivate assignment
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin,Kayitci")]
    public async Task<ActionResult> Deactivate(int id)
    {
        var result = await _service.DeactivateAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Assignment deactivated successfully" });
    }
}
