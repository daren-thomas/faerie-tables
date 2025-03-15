using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FaerieTables.Api;
using FaerieTables.Api.Controllers;
using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FaerieTables.Api.Tests.Controllers
{
    public class RollControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RollControllerTests(WebApplicationFactory<Program> factory)
        {
            // Configure an in-memory SQLite for integration testing.
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<RandomTableContext>));
                    services.RemoveAll(typeof(RandomTableContext));
                    
                    var connection = new SqliteConnection("Data Source=:memory:");
                    connection.Open();

                    services.AddDbContext<RandomTableContext>(options =>
                        options.UseSqlite(connection));

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                        db.Database.EnsureCreated();

                        // Seed table and session data.
                        SeedTestData(db);
                    }
                });
            });
        }

        private void SeedTestTable(RandomTableContext context)
        {
            var tableId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var table = new Table
            {
                Id = tableId,
                Title = "Seeded Table",
                DiceRange = "1d6",
                Columns = new List<TableColumn>
                {
                    new TableColumn { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), TableId = tableId, Name = "Encounter", Type = "text" },
                    new TableColumn { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), TableId = tableId, Name = "Environment", Type = "text" }
                },
                Rows = new List<TableRow>()
            };

            // Create 3 rows.
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
        public async Task PostRoll_Returns_Ok_With_RollResponse()
        {
            var client = _factory.CreateClient();

            // Use the seeded table and a test session ID.
            var rollRequest = new RollRequestDto
            {
                TableId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SessionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Mode = "row",
                Overrides = new List<OverrideDto>
                {
                    new OverrideDto { Column = "Encounter", Value = "Manual Override Encounter" }
                }
            };

            var response = await client.PostAsJsonAsync("/api/roll", rollRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RollResponseDto>();
            Assert.NotNull(result);
            Assert.Equal("Seeded Table", result!.TableTitle);
            Assert.Equal("row", result.Mode, ignoreCase: true);
            // Check that override was applied.
            Assert.Contains("Encounter", result.Results.Keys);
            Assert.Equal("Manual Override Encounter", result.Results["Encounter"]);
        }
        
        private void SeedTestData(RandomTableContext context)
        {
            // Seed table (as before)
            SeedTestTable(context);

            // Create and add a session.
            var sessionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            context.Sessions.Add(new Session
            {
                SessionId = sessionId,
                UserId = "TestUser",
                Name = "Test Session",
                Description = "Session for integration tests"
            });
            context.SaveChanges();
        }
    }
}
