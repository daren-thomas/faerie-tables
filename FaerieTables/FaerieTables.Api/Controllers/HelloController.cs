using Microsoft.AspNetCore.Mvc;

namespace FaerieTables.Api.Controllers;

/// <summary>
/// Provides a simple greeting endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello World from the API!");
    }
}