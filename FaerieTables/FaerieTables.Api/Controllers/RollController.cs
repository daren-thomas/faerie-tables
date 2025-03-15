﻿using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FaerieTables.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RollController : ControllerBase
    {
        private readonly RandomTableContext _context;
        private readonly IRollingService _rollingService;

        public RollController(RandomTableContext context, IRollingService rollingService)
        {
            _context = context;
            _rollingService = rollingService;
        }

        // POST /api/roll
        [HttpPost]
        public async Task<ActionResult<RollResponseDto>> Roll([FromBody] RollRequestDto request)
        {
            if (request == null ||
                request.TableId == Guid.Empty ||
                request.SessionId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.Mode))
            {
                return BadRequest("Missing required parameters.");
            }
            
            // Retrieve the session to ensure it exists.
            var session = await _context.Sessions.FindAsync(request.SessionId);
            if (session == null)
                return NotFound($"Session with ID {request.SessionId} not found.");

            try
            {
                // Get the roll results (a dictionary mapping TableColumnId to value)
                var results = await _rollingService.RollTableAsync(request.TableId, request.Mode, request.Overrides);

                // Retrieve the table to get additional info (like title)
                var table = await _context.Tables.FindAsync(request.TableId);
                if (table == null)
                    return NotFound($"Table with ID {request.TableId} not found.");

                // Create a new Roll entity
                var roll = new Roll
                {
                    RollId = Guid.NewGuid(),
                    SessionId = request.SessionId,
                    TableId = request.TableId,
                    TableTitle = table.Title,
                    Mode = request.Mode,
                    Timestamp = DateTime.UtcNow,
                };

                // Create RollResult records from the results dictionary
                foreach (var result in results)
                {
                    roll.RollResults.Add(new RollResult
                    {
                        Id = Guid.NewGuid(),
                        RollId = roll.RollId,
                        TableColumnId = result.Key,
                        Value = result.Value
                    });
                }

                _context.Rolls.Add(roll);
                await _context.SaveChangesAsync();

                // Prepare a response DTO that includes the roll id and results.
                var responseDto = new RollResponseDto
                {
                    RollId = roll.RollId,
                    TableId = roll.TableId,
                    TableTitle = roll.TableTitle,
                    Mode = roll.Mode,
                    Timestamp = roll.Timestamp,
                    Results = results.ToDictionary(kvp => 
                        // Optionally, lookup the column name from the table columns.
                        _context.TableColumns.FirstOrDefault(tc => tc.Id == kvp.Key)?.Name ?? kvp.Key.ToString(),
                        kvp => kvp.Value)
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class RollRequestDto
    {
        public Guid TableId { get; set; }
        public Guid SessionId { get; set; }
        public string Mode { get; set; } = string.Empty;
        public List<OverrideDto>? Overrides { get; set; }
    }

    public class RollResponseDto
    {
        public Guid RollId { get; set; }
        public Guid TableId { get; set; }
        public string TableTitle { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Results { get; set; } = new Dictionary<string, string>();
    }
}
