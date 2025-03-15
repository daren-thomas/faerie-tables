using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FaerieTables.Api;
using FaerieTables.Api.Controllers;
using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FaerieTables.Api.Tests.Controllers
{
    public class SessionControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SessionControllerTests(WebApplicationFactory<Program> factory)
        {
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

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                        db.Database.EnsureCreated();
                    }
                });
            });
        }

        // Helper method to seed a test table with columns, rows, and row values.
        private void SeedTestTable(RandomTableContext context, out Table seededTable)
        {
            seededTable = new Table
            {
                Id = Guid.NewGuid(),
                Title = "Seeded Test Table",
                Source = "UnitTest",
                License = "MIT",
                Description = "A seeded table for testing session roll export",
                DiceRange = "1d6",
                Columns = new List<TableColumn>
                {
                    new TableColumn { Id = Guid.NewGuid(), Name = "Encounter", Type = "text" },
                    new TableColumn { Id = Guid.NewGuid(), Name = "Environment", Type = "text" }
                },
                Rows = new List<TableRow>()
            };

            // Seed three rows with values.
            for (int i = 0; i < 3; i++)
            {
                var row = new TableRow
                {
                    Id = Guid.NewGuid(),
                    TableId = seededTable.Id,
                    RowValues = new List<RowValue>()
                };

                foreach (var column in seededTable.Columns)
                {
                    row.RowValues.Add(new RowValue
                    {
                        Id = Guid.NewGuid(),
                        RowId = row.Id,
                        ColumnId = column.Id,
                        Value = $"{column.Name} Value {i + 1}"
                    });
                }
                seededTable.Rows.Add(row);
            }

            context.Tables.Add(seededTable);
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateSession_Returns_Created_Session()
        {
            var client = _factory.CreateClient();
            var sessionDto = new SessionDto
            {
                UserId = "TestUser",
                Name = "Test Session",
                Description = "Session for testing"
            };

            var response = await client.PostAsJsonAsync("/api/session", sessionDto);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdSession = await response.Content.ReadFromJsonAsync<Session>();
            Assert.NotNull(createdSession);
            Assert.Equal("Test Session", createdSession!.Name);
        }

        [Fact]
        public async Task Roll_Is_Associated_With_Session()
        {
            var client = _factory.CreateClient();

            // Create a session first.
            var sessionDto = new SessionDto
            {
                UserId = "TestUser",
                Name = "Session for Rolls",
                Description = "Testing roll association"
            };
            var sessionResponse = await client.PostAsJsonAsync("/api/session", sessionDto);
            sessionResponse.EnsureSuccessStatusCode();
            var session = await sessionResponse.Content.ReadFromJsonAsync<Session>();
            Assert.NotNull(session);

            // Seed a test table into the database.
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                SeedTestTable(context, out var seededTable);
            }

            // Instead of calling the TableController endpoint,
            // retrieve the seeded table from the context.
            Guid tableId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                tableId = context.Tables.FirstOrDefault(t => t.Title == "Seeded Test Table")?.Id ?? Guid.Empty;
            }
            Assert.NotEqual(Guid.Empty, tableId);

            // Perform a roll that includes the session id.
            var rollRequest = new RollRequestDto
            {
                TableId = tableId,
                SessionId = session!.SessionId,
                Mode = "row",
                Overrides = new List<OverrideDto>()
            };
            var rollResponse = await client.PostAsJsonAsync("/api/roll", rollRequest);
            rollResponse.EnsureSuccessStatusCode();

            // Retrieve the rolls for the session.
            var getRollsResponse = await client.GetAsync($"/api/session/{session.SessionId}/rolls");
            getRollsResponse.EnsureSuccessStatusCode();
            var rolls = await getRollsResponse.Content.ReadFromJsonAsync<List<Roll>>();
            Assert.NotNull(rolls);
            Assert.True(rolls!.Count > 0);
            Assert.All(rolls, r => Assert.Equal(session.SessionId, r.SessionId));
        }

        [Fact]
        public async Task MarkdownExport_Includes_Correct_Data()
        {
            var client = _factory.CreateClient();

            // Create a session.
            var sessionDto = new SessionDto
            {
                UserId = "TestUser",
                Name = "Session for Markdown Export",
                Description = "Testing markdown export"
            };
            var sessionResponse = await client.PostAsJsonAsync("/api/session", sessionDto);
            sessionResponse.EnsureSuccessStatusCode();
            var session = await sessionResponse.Content.ReadFromJsonAsync<Session>();
            Assert.NotNull(session);

            // Seed a test table.
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                SeedTestTable(context, out var seededTable);
            }
            Guid tableId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                tableId = context.Tables.FirstOrDefault(t => t.Title == "Seeded Test Table")?.Id ?? Guid.Empty;
            }
            Assert.NotEqual(Guid.Empty, tableId);

            // Perform a roll with an override.
            var rollRequest = new RollRequestDto
            {
                TableId = tableId,
                SessionId = session!.SessionId,
                Mode = "row",
                Overrides = new List<OverrideDto>
                {
                    new OverrideDto { Column = "Encounter", Value = "Test Encounter" }
                }
            };
            var rollResponse = await client.PostAsJsonAsync("/api/roll", rollRequest);
            rollResponse.EnsureSuccessStatusCode();

            // Export the session log as Markdown.
            var exportResponse = await client.GetAsync($"/api/session/{session.SessionId}/export");
            exportResponse.EnsureSuccessStatusCode();
            Assert.Equal("text/markdown", exportResponse.Content.Headers.ContentType?.MediaType);
            var markdown = await exportResponse.Content.ReadAsStringAsync();
            Assert.Contains("Seeded Test Table", markdown);
            Assert.Contains("Test Encounter", markdown);
        }

        [Fact]
        public async Task Clearing_Session_Rolls_Works_Correctly()
        {
            var client = _factory.CreateClient();

            // Create a session.
            var sessionDto = new SessionDto
            {
                UserId = "TestUser",
                Name = "Session for Clearing Rolls",
                Description = "Testing clearing rolls"
            };
            var sessionResponse = await client.PostAsJsonAsync("/api/session", sessionDto);
            sessionResponse.EnsureSuccessStatusCode();
            var session = await sessionResponse.Content.ReadFromJsonAsync<Session>();
            Assert.NotNull(session);

            // Seed a test table.
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                SeedTestTable(context, out var seededTable);
            }
            Guid tableId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                tableId = context.Tables.FirstOrDefault(t => t.Title == "Seeded Test Table")?.Id ?? Guid.Empty;
            }
            Assert.NotEqual(Guid.Empty, tableId);

            // Perform two rolls.
            var rollRequest = new RollRequestDto
            {
                TableId = tableId,
                SessionId = session!.SessionId,
                Mode = "row",
                Overrides = new List<OverrideDto>()
            };

            await client.PostAsJsonAsync("/api/roll", rollRequest);
            await client.PostAsJsonAsync("/api/roll", rollRequest);

            // Verify rolls exist.
            var getRollsResponse = await client.GetAsync($"/api/session/{session.SessionId}/rolls");
            getRollsResponse.EnsureSuccessStatusCode();
            var rollsBeforeClear = await getRollsResponse.Content.ReadFromJsonAsync<List<Roll>>();
            Assert.NotNull(rollsBeforeClear);
            Assert.True(rollsBeforeClear!.Count >= 2);

            // Clear the session's rolls.
            var clearResponse = await client.DeleteAsync($"/api/session/{session.SessionId}/rolls");
            Assert.Equal(HttpStatusCode.NoContent, clearResponse.StatusCode);

            // Verify that no rolls remain.
            var getRollsAfterClearResponse = await client.GetAsync($"/api/session/{session.SessionId}/rolls");
            getRollsAfterClearResponse.EnsureSuccessStatusCode();
            var rollsAfterClear = await getRollsAfterClearResponse.Content.ReadFromJsonAsync<List<Roll>>();
            Assert.NotNull(rollsAfterClear);
            Assert.Empty(rollsAfterClear!);
        }
    }
}
