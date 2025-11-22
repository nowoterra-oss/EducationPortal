using EduPortal.Application.DTOs.PackagePurchase;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/student-package-purchases")]
[Authorize]
public class StudentPackagePurchasesController : ControllerBase
{
    private readonly IStudentPackagePurchaseService _service;

    public StudentPackagePurchasesController(IStudentPackagePurchaseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all package purchases
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentPackagePurchaseDto>>> GetAll()
    {
        var purchases = await _service.GetAllPurchasesAsync();
        return Ok(purchases);
    }

    /// <summary>
    /// Get active purchases
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<StudentPackagePurchaseDto>>> GetActive()
    {
        var purchases = await _service.GetActivePurchasesAsync();
        return Ok(purchases);
    }

    /// <summary>
    /// Get purchase summaries
    /// </summary>
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<PurchaseSummaryDto>>> GetSummaries()
    {
        var summaries = await _service.GetPurchaseSummariesAsync();
        return Ok(summaries);
    }

    /// <summary>
    /// Get purchase by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentPackagePurchaseDto>> GetById(int id)
    {
        var purchase = await _service.GetPurchaseByIdAsync(id);
        if (purchase == null)
            return NotFound();

        return Ok(purchase);
    }

    /// <summary>
    /// Get purchases by student
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<StudentPackagePurchaseDto>>> GetByStudent(int studentId)
    {
        var purchases = await _service.GetPurchasesByStudentAsync(studentId);
        return Ok(purchases);
    }

    /// <summary>
    /// Get purchases by package
    /// </summary>
    [HttpGet("package/{packageId}")]
    public async Task<ActionResult<IEnumerable<StudentPackagePurchaseDto>>> GetByPackage(int packageId)
    {
        var purchases = await _service.GetPurchasesByPackageAsync(packageId);
        return Ok(purchases);
    }

    /// <summary>
    /// Get statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<PurchaseStatisticsDto>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Create new purchase
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci,Muhasebe")]
    public async Task<ActionResult<StudentPackagePurchaseDto>> Create([FromBody] CreateStudentPackagePurchaseDto dto)
    {
        try
        {
            var purchase = await _service.CreatePurchaseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = purchase.Id }, purchase);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update purchase
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<StudentPackagePurchaseDto>> Update(int id, [FromBody] UpdateStudentPackagePurchaseDto dto)
    {
        try
        {
            var purchase = await _service.UpdatePurchaseAsync(id, dto);
            return Ok(purchase);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Use one session from package
    /// </summary>
    [HttpPost("{id}/use-session")]
    [Authorize(Roles = "Admin,Coach,Ogretmen")]
    public async Task<ActionResult> UseSession(int id)
    {
        var result = await _service.UseSessionAsync(id);
        if (!result)
            return BadRequest("Cannot use session");

        return Ok(new { message = "Session used successfully" });
    }

    /// <summary>
    /// Delete purchase (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeletePurchaseAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
