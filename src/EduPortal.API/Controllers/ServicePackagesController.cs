using EduPortal.Application.DTOs.Package;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/service-packages")]
[Authorize]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _service;

    public ServicePackagesController(IServicePackageService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all service packages
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicePackageDto>>> GetAll()
    {
        var packages = await _service.GetAllPackagesAsync();
        return Ok(packages);
    }

    /// <summary>
    /// Get active packages
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ServicePackageDto>>> GetActive()
    {
        var packages = await _service.GetActivePackagesAsync();
        return Ok(packages);
    }

    /// <summary>
    /// Get package summaries
    /// </summary>
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<ServicePackageSummaryDto>>> GetSummaries()
    {
        var summaries = await _service.GetPackageSummariesAsync();
        return Ok(summaries);
    }

    /// <summary>
    /// Get package by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ServicePackageDto>> GetById(int id)
    {
        var package = await _service.GetPackageByIdAsync(id);
        if (package == null)
            return NotFound();

        return Ok(package);
    }

    /// <summary>
    /// Get statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Muhasebe")]
    public async Task<ActionResult<PackageStatisticsDto>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Create new package
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServicePackageDto>> Create([FromBody] CreateServicePackageDto dto)
    {
        var package = await _service.CreatePackageAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = package.Id }, package);
    }

    /// <summary>
    /// Update package
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServicePackageDto>> Update(int id, [FromBody] UpdateServicePackageDto dto)
    {
        try
        {
            var package = await _service.UpdatePackageAsync(id, dto);
            return Ok(package);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete package (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeletePackageAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
