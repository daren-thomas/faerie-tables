using System.ComponentModel.DataAnnotations;

namespace FaerieTables.Api.Models;

public class TableColumnDto
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }

    [Required(ErrorMessage = "Column Name is required.")]
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "text";
}