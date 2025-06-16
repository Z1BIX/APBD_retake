using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services;

namespace Project.Controllers;

[ApiController]
[Route("api")]
public class ArtifactsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ArtifactsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpGet("projects/{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        var result = await _dbService.GetProjectByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost("artifacts")]
    public async Task<IActionResult> AddArtifactWithProject([FromBody] CreateArtifactWithProjectDto dto)
    {
        try
        {
            await _dbService.AddArtifactWithProjectAsync(dto);
            return Ok(new { message = "Artifact and project added successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
