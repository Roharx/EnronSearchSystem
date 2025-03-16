using Microsoft.AspNetCore.Mvc;
using SearchAPI.Interfaces;

namespace SearchAPI.Controllers;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly IDatabaseService _databaseService;

    public SearchController(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q))
            return BadRequest("Search query cannot be empty");

        var results = await _databaseService.SearchFilesAsync(q);
        return Ok(results);
    }
}