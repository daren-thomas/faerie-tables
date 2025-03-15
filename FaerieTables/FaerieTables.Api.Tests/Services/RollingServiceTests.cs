using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FaerieTables.Api.Tests.Services
{
    public class RollingServiceTests
    {
        private RandomTableContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RandomTableContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new RandomTableContext(options);
        }

        private void SeedTable(RandomTableContext context, Guid tableId)
        {
            // Create a table with 2 columns and 3 rows.
            var table = new Table
            {
                Id = tableId,
                Title = "Test Table",
                DiceRange = "1d6",
                Columns = new List<TableColumn>
                {
                    new TableColumn { Id = Guid.NewGuid(), TableId = tableId, Name = "Encounter", Type = "text" },
                    new TableColumn { Id = Guid.NewGuid(), TableId = tableId, Name = "Environment", Type = "text" }
                },
                Rows = new List<TableRow>()
            };

            // For each row, add row values for each column.
            for (int i = 0; i < 3; i++)
            {
                var row = new TableRow
                {
                    Id = Guid.NewGuid(),
                    TableId = tableId,
                    RowValues = new List<RowValue>()
                };

                foreach (var column in table.Columns)
                {
                    row.RowValues.Add(new RowValue
                    {
                        Id = Guid.NewGuid(),
                        RowId = row.Id,
                        ColumnId = column.Id,
                        Value = $"{column.Name} Value {i + 1}"
                    });
                }
                table.Rows.Add(row);
            }

            context.Tables.Add(table);
            context.SaveChanges();
        }

        [Fact]
        public async Task RollTableAsync_RowMode_Returns_All_Columns_From_Single_Row()
        {
            var context = GetInMemoryContext();
            var tableId = Guid.NewGuid();
            SeedTable(context, tableId);

            var service = new RollingService(context);
            var results = await service.RollTableAsync(tableId, "row");

            // Expect as many results as there are columns.
            var table = await context.Tables.Include(t => t.Columns).Include(t => t.Rows).ThenInclude(r => r.RowValues)
                .FirstOrDefaultAsync(t => t.Id == tableId);
            Assert.NotNull(table);
            Assert.Equal(table.Columns.Count, results.Count);

            // All values should be non-empty.
            Assert.All(results.Values, value => Assert.False(string.IsNullOrEmpty(value)));
        }

        [Fact]
        public async Task RollTableAsync_ColumnMode_Returns_Values_For_Each_Column()
        {
            var context = GetInMemoryContext();
            var tableId = Guid.NewGuid();
            SeedTable(context, tableId);

            var service = new RollingService(context);
            var results = await service.RollTableAsync(tableId, "column");

            var table = await context.Tables.Include(t => t.Columns).Include(t => t.Rows).ThenInclude(r => r.RowValues)
                .FirstOrDefaultAsync(t => t.Id == tableId);
            Assert.NotNull(table);
            Assert.Equal(table.Columns.Count, results.Count);
            Assert.All(results.Values, value => Assert.False(string.IsNullOrEmpty(value)));
        }

        [Fact]
        public async Task RollTableAsync_Overrides_Are_Applied()
        {
            var context = GetInMemoryContext();
            var tableId = Guid.NewGuid();
            SeedTable(context, tableId);

            var service = new RollingService(context);
            var overrides = new List<OverrideDto>
            {
                new OverrideDto { Column = "Encounter", Value = "Custom Encounter" }
            };

            var results = await service.RollTableAsync(tableId, "row", overrides);

            // Find the column with name "Encounter"
            var table = await context.Tables.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == tableId);
            var encounterColumn = table?.Columns.FirstOrDefault(c => c.Name.Equals("Encounter", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(encounterColumn);
            Assert.Equal("Custom Encounter", results[encounterColumn!.Id]);
        }
    }
}
