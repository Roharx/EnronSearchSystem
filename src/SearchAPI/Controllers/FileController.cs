using Microsoft.AspNetCore.Mvc;
using SearchAPI.Interfaces;

namespace SearchAPI.Controllers;

[ApiController]
[Route("file")]
public class FileController : ControllerBase
{
    private readonly IDatabaseService _databaseService;

    public FileController(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFile(int id)
    {
        var file = await _databaseService.GetFileAsync(id);
        if (file == null) return NotFound("File not found");

        return Ok(file);
    }
}