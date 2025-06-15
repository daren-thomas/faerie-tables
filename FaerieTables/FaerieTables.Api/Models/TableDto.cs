using System.ComponentModel.DataAnnotations;

namespace FaerieTables.Api.Models;

public class TableDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiceRange { get; set; } = string.Empty;

    // Optional: if you want to include columns/rows directly in a single DTO.
    public List<TableColumnDto> Columns { get; set; } = new();
    public List<TableRowDto> Rows { get; set; } = new();
}

// If you want a separate DTO for RowValue, create another class.
