﻿using Microsoft.AspNetCore.Mvc;

namespace FaerieTables.Api.Controllers;

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