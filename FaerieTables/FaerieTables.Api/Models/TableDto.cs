using System.ComponentModel.DataAnnotations;

namespace FaerieTables.Api.Models;

public class TableDto
{
    public Guid Id { get; set; }

    [Required] public string Title { get; set; } = null!;

    public string Source { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiceRange { get; set; } = string.Empty;
}