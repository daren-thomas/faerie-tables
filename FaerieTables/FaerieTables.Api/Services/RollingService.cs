using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaerieTables.Api.Services
{
    public interface IRollingService
    {
        /// <summary>
        /// Performs a roll on the table using the specified mode and applies any manual overrides.
        /// Returns a dictionary keyed by the TableColumn Id with the rolled (or overridden) value.
        /// </summary>
        Task<Dictionary<Guid, string>> RollTableAsync(Guid tableId, string mode, List<OverrideDto>? overrides = null);
    }

    public class RollingService : IRollingService
    {
        private readonly RandomTableContext _context;
        private readonly Random _random = new Random();

        public RollingService(RandomTableContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<Guid, string>> RollTableAsync(Guid tableId, string mode, List<OverrideDto>? overrides = null)
        {
            // Load table including Columns and Rows with their RowValues
            var table = await _context.Tables
                .Include(t => t.Columns)
                .Include(t => t.Rows)
                    .ThenInclude(r => r.RowValues)
                .FirstOrDefaultAsync(t => t.Id == tableId);

            if (table == null)
                throw new ArgumentException($"Table with ID {tableId} not found.");

            if (table.Columns == null || !table.Columns.Any())
                throw new InvalidOperationException("The table has no columns defined.");

            if (table.Rows == null || !table.Rows.Any())
                throw new InvalidOperationException("The table has no rows defined.");

            var results = new Dictionary<Guid, string>();

            if (mode.Equals("row", StringComparison.OrdinalIgnoreCase))
            {
                // Pick one row at random.
                int randomRowIndex = _random.Next(table.Rows.Count);
                var selectedRow = table.Rows.ElementAt(randomRowIndex);

                // For each column, look for a row value in the selected row.
                foreach (var column in table.Columns)
                {
                    var valueEntry = selectedRow.RowValues.FirstOrDefault(rv => rv.ColumnId == column.Id);
                    results[column.Id] = valueEntry?.Value ?? string.Empty;
                }
            }
            else if (mode.Equals("column", StringComparison.OrdinalIgnoreCase))
            {
                // For each column, choose a random row value.
                foreach (var column in table.Columns)
                {
                    // Gather all row values for the column across all rows.
                    var valuesForColumn = table.Rows
                        .SelectMany(r => r.RowValues)
                        .Where(rv => rv.ColumnId == column.Id)
                        .ToList();

                    if (valuesForColumn.Count == 0)
                        results[column.Id] = string.Empty;
                    else
                    {
                        int index = _random.Next(valuesForColumn.Count);
                        results[column.Id] = valuesForColumn[index].Value;
                    }
                }
            }
            else
            {
                throw new ArgumentException("Mode must be either 'row' or 'column'.");
            }

            // Apply any overrides (lookup column by name)
            if (overrides != null && overrides.Any())
            {
                foreach (var ovrd in overrides)
                {
                    // Find the column with matching name (case-insensitive)
                    var column = table.Columns.FirstOrDefault(c => c.Name.Equals(ovrd.Column, StringComparison.OrdinalIgnoreCase));
                    if (column != null)
                    {
                        results[column.Id] = ovrd.Value;
                    }
                }
            }

            return results;
        }
    }

    public class OverrideDto
    {
        public string Column { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
