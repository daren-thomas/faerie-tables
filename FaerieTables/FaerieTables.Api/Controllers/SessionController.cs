using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FaerieTables.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly RandomTableContext _context;

        public SessionController(RandomTableContext context)
        {
            _context = context;
        }

        // POST /api/session
        [HttpPost]
        public async Task<ActionResult<Session>> CreateSession([FromBody] SessionDto sessionDto)
        {
            if (string.IsNullOrWhiteSpace(sessionDto.Name) || string.IsNullOrWhiteSpace(sessionDto.Description))
            {
                return BadRequest("Name and Description are required.");
            }

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = sessionDto.UserId ?? "Anonymous",
                Name = sessionDto.Name,
                Description = sessionDto.Description
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }

        // GET /api/session/{sessionId}
        [HttpGet("{sessionId:guid}")]
        public async Task<ActionResult<Session>> GetSession(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Rolls)
                .ThenInclude(r => r.RollResults)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null)
                return NotFound();

            return Ok(session);
        }

        // GET /api/session/{sessionId}/rolls
        [HttpGet("{sessionId:guid}/rolls")]
        public async Task<ActionResult<IEnumerable<Roll>>> GetSessionRolls(Guid sessionId)
        {
            var exists = await _context.Sessions.AnyAsync(s => s.SessionId == sessionId);
            if (!exists)
                return NotFound($"Session with ID {sessionId} not found.");

            var rolls = await _context.Rolls
                .Where(r => r.SessionId == sessionId)
                .Include(r => r.RollResults)
                .ToListAsync();

            var okObjectResult = Ok(rolls);
            return okObjectResult;
        }

        // GET /api/session/{sessionId}/export
        [HttpGet("{sessionId:guid}/export")]
        public async Task<IActionResult> ExportSessionRolls(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Rolls)
                    .ThenInclude(r => r.RollResults)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null)
                return NotFound($"Session with ID {sessionId} not found.");

            // Create a simple Markdown export. Each roll is output as a bullet
            var sb = new StringBuilder();
            sb.AppendLine($"# Session {session.Name} ({session.SessionId}) Roll Log");
            sb.AppendLine();
            foreach (var roll in session.Rolls.OrderBy(r => r.Timestamp))
            {
                // For each roll, list the table title and the roll results.
                // In this basic example, we output the TableColumnId (or name if available) along with its value.
                var resultParts = roll.RollResults.Select(rr => $"{_context.TableColumns.FirstOrDefault(tc => tc.Id == rr.TableColumnId)?.Name ?? rr.TableColumnId.ToString()}: {rr.Value}");
                sb.AppendLine($"- [{roll.TableTitle}](# {roll.TableId}) - {roll.Timestamp:yyyy-MM-dd HH:mm:ss}: {string.Join(", ", resultParts)}");
            }

            return Content(sb.ToString(), "text/markdown");
        }

        // DELETE /api/session/{sessionId}/rolls
        [HttpDelete("{sessionId:guid}/rolls")]
        public async Task<IActionResult> ClearSessionRolls(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Rolls)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null)
                return NotFound($"Session with ID {sessionId} not found.");

            _context.Rolls.RemoveRange(session.Rolls);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class SessionDto
    {
        public string? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
