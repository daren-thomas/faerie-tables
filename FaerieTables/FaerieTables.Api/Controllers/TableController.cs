using FaerieTables.Api.Entities;
using FaerieTables.Api.Models;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FaerieTables.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }

    // GET /tables?search=...
    [HttpGet]
    public async Task<ActionResult<List<TableDto>>> GetAll([FromQuery] string? search)
    {
        var tables = await _tableService.GetAllAsync(search);
        var dtos = tables.Select(t => new TableDto
        {
            Id = t.Id,
            Title = t.Title,
            Source = t.Source,
            License = t.License,
            Description = t.Description,
            DiceRange = t.DiceRange
        }).ToList();

        return Ok(dtos);
    }

    // GET /tables/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TableDto>> GetById(Guid id)
    {
        var table = await _tableService.GetByIdAsync(id);
        if (table == null)
            return NotFound();

        var dto = new TableDto
        {
            Id = table.Id,
            Title = table.Title,
            Source = table.Source,
            License = table.License,
            Description = table.Description,
            DiceRange = table.DiceRange
        };
        return Ok(dto);
    }

    // POST /tables
    [HttpPost]
    public async Task<ActionResult<TableDto>> Create([FromBody] TableDto dto)
    {
        // Minimal check
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newTable = new Table
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Source = dto.Source ?? string.Empty,
            License = dto.License ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            DiceRange = dto.DiceRange ?? string.Empty
        };

        newTable = await _tableService.CreateAsync(newTable);

        dto.Id = newTable.Id;
        return CreatedAtAction(nameof(GetById), new { id = newTable.Id }, dto);
    }

    // PUT /tables/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TableDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch between route and body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _tableService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Title = dto.Title;
        existing.Source = dto.Source ?? string.Empty;
        existing.License = dto.License ?? string.Empty;
        existing.Description = dto.Description ?? string.Empty;
        existing.DiceRange = dto.DiceRange ?? string.Empty;

        await _tableService.UpdateAsync(existing);
        return NoContent();
    }

    // DELETE /tables/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _tableService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _tableService.DeleteAsync(existing);
        return NoContent();
    }
}