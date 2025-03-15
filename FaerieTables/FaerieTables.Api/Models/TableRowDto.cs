namespace FaerieTables.Api.Models;

public class TableRowDto
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }

    // Optionally include RowValues in the row, etc.
    // public List<RowValueDto> RowValues { get; set; } = new();
}