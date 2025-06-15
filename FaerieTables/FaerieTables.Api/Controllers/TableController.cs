using FaerieTables.Api.Entities;
using FaerieTables.Api.Models;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FaerieTables.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController(ITableService tableService) : ControllerBase
{
    // GET /api/table?search=...
    [HttpGet]
    public async Task<ActionResult<List<TableDto>>> GetAll([FromQuery] string? search)
    {
        var tables = await tableService.GetAllAsync(search);

        // Map entities to DTOs
        var dtos = tables.Select(t => new TableDto
        {
            Id = t.Id,
            Title = t.Title,
            Source = t.Source,
            License = t.License,
            Description = t.Description,
            DiceRange = t.DiceRange
            // If you want to populate columns/rows in the same response, you can do so:
            // Columns = t.Columns.Select(c => new TableColumnDto { ... }).ToList(),
            // Rows = t.Rows.Select(r => new TableRowDto { ... }).ToList()
        }).ToList();

        return Ok(dtos);
    }

    // GET /api/table/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TableDto>> GetById(Guid id)
    {
        var table = await tableService.GetByIdAsync(id);
        if (table == null)
            return NotFound();

        var dto = new TableDto
        {
            Id = table.Id,
            Title = table.Title,
            Source = table.Source,
            License = table.License,
            Description = table.Description,
            DiceRange = table.DiceRange,
            Columns = table.Columns.Select(c => new TableColumnDto
            {
                Id = c.Id,
                TableId = c.TableId,
                Name = c.Name,
                Type = c.Type
            }).ToList(),
            Rows = table.Rows.Select(r => new TableRowDto
            {
                Id = r.Id,
                TableId = r.TableId
            }).ToList()
        };
        return Ok(dto);
    }

    // POST /api/table
    [HttpPost]
    public async Task<ActionResult<TableDto>> Create([FromBody] TableDto dto)
    {
        // Validate the DTO
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new Table
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Source = dto.Source,
            License = dto.License,
            Description = dto.Description,
            DiceRange = dto.DiceRange
        };

        // Persist via service
        var created = await tableService.CreateAsync(entity);

        // Return the created DTO
        dto.Id = created.Id;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
    }

    // PUT /api/table/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TableDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch between route and body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await tableService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Title = dto.Title;
        existing.Source = dto.Source;
        existing.License = dto.License;
        existing.Description = dto.Description;
        existing.DiceRange = dto.DiceRange;

        await tableService.UpdateAsync(existing);
        return NoContent();
    }

    // DELETE /api/table/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await tableService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await tableService.DeleteAsync(existing);
        return NoContent();
    }
}