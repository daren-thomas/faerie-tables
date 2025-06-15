using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                string.IsNullOrWhiteSpace(request.Mode))
            {
                return BadRequest("Missing required parameters.");
            }

            try
            {
                // Perform the roll using the rolling service
                var results = await _rollingService.RollTableAsync(request.TableId, request.Mode, request.Overrides);

                // Retrieve the table for additional details
                var table = await _context.Tables.FindAsync(request.TableId);
                if (table == null)
                    return NotFound($"Table with ID {request.TableId} not found.");

                var responseDto = new RollResponseDto
                {
                    TableId = request.TableId,
                    TableTitle = table.Title,
                    Mode = request.Mode,
                    Results = results.ToDictionary(
                        kvp => _context.TableColumns.FirstOrDefault(tc => tc.Id == kvp.Key)?.Name ?? kvp.Key.ToString(),
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
        public string Mode { get; set; } = string.Empty;
        public List<OverrideDto>? Overrides { get; set; }
    }

    public class RollResponseDto
    {
        public Guid TableId { get; set; }
        public string TableTitle { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public Dictionary<string, string> Results { get; set; } = new Dictionary<string, string>();
    }
}
