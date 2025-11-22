using EduPortal.Application.DTOs.Document;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/application-documents")]
[Authorize]
public class ApplicationDocumentsController : ControllerBase
{
    private readonly IApplicationDocumentService _documentService;

    public ApplicationDocumentsController(IApplicationDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<ApplicationDocumentDto>>> GetAllDocuments()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        return Ok(documents);
    }

    [HttpGet("program/{programId}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<IEnumerable<ApplicationDocumentDto>>> GetDocumentsByProgram(int programId)
    {
        var documents = await _documentService.GetDocumentsByProgramAsync(programId);
        return Ok(documents);
    }

    [HttpGet("program/{programId}/checklist")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<DocumentChecklistDto>> GetDocumentChecklist(int programId)
    {
        try
        {
            var checklist = await _documentService.GetDocumentChecklistAsync(programId);
            return Ok(checklist);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("expiring")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<ApplicationDocumentDto>>> GetExpiringDocuments([FromQuery] int days = 30)
    {
        var documents = await _documentService.GetExpiringDocumentsAsync(days);
        return Ok(documents);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<DocumentStatisticsDto>> GetStatistics()
    {
        var statistics = await _documentService.GetStatisticsAsync();
        return Ok(statistics);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<ApplicationDocumentDto>> GetDocumentById(int id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null)
            return NotFound($"Application document with ID {id} not found");

        return Ok(document);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<ApplicationDocumentDto>> CreateDocument([FromBody] CreateApplicationDocumentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var document = await _documentService.CreateDocumentAsync(dto);
        return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<ApplicationDocumentDto>> UpdateDocument(int id, [FromBody] UpdateApplicationDocumentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var document = await _documentService.UpdateDocumentAsync(id, dto);
            return Ok(document);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteDocument(int id)
    {
        var success = await _documentService.DeleteDocumentAsync(id);
        if (!success)
            return NotFound($"Application document with ID {id} not found");

        return NoContent();
    }
}
