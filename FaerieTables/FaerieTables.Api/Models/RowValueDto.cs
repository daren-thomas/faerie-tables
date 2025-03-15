namespace FaerieTables.Api.Models;

public class RowValueDto
{
    public Guid Id { get; set; }
    public Guid RowId { get; set; }
    public Guid ColumnId { get; set; }
    public string Value { get; set; } = string.Empty;
}