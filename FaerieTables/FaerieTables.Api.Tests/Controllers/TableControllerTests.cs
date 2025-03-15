using System.Net;
using System.Net.Http.Json;
using FaerieTables.Api;
using FaerieTables.Api.Data;
using FaerieTables.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FaerieTables.Api.Tests.Controllers
{
    public class TableControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        
        public TableControllerTests(WebApplicationFactory<Program> factory)
        {
            // We override the normal DB with in-memory for testing
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing registrations for RandomTableContext
                    services.RemoveAll(typeof(DbContextOptions<RandomTableContext>));
                    services.RemoveAll(typeof(RandomTableContext));

                    // Create an in-memory SQLite connection and keep it open
                    var connection = new SqliteConnection("Data Source=:memory:");
                    connection.Open();

                    // Register the DbContext using the SQLite connection
                    services.AddDbContext<RandomTableContext>(options =>
                        options.UseSqlite(connection));

                    // Optionally, store the connection somewhere to dispose it later.
                    
                    // Build a temporary provider and ensure the schema is created.
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
                        db.Database.EnsureCreated();
                    }
                });
            });
        }

        [Fact]
        public async Task POST_Create_ReturnsCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newTable = new TableDto
            {
                Title = "Integration Test Table"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/table", newTable);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<TableDto>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);
            Assert.Equal("Integration Test Table", created.Title);
        }

        [Fact]
        public async Task GET_All_Should_Include_Created_Table()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Create one table
            var table = new TableDto { Title = "First One" };
            var postResponse = await client.PostAsJsonAsync("/api/table", table);
            postResponse.EnsureSuccessStatusCode();

            // Act
            var getResponse = await client.GetAsync("/api/table");
            getResponse.EnsureSuccessStatusCode();

            // Assert
            var list = await getResponse.Content.ReadFromJsonAsync<List<TableDto>>();
            Assert.NotNull(list);
            Assert.Single(list!);
            Assert.Equal("First One", list[0].Title);
        }

        [Fact]
        public async Task PUT_Update_Should_Return_NoContent()
        {
            var client = _factory.CreateClient();

            // Create table
            var table = new TableDto { Title = "Old Title" };
            var postResponse = await client.PostAsJsonAsync("/api/table", table);
            postResponse.EnsureSuccessStatusCode();

            var created = await postResponse.Content.ReadFromJsonAsync<TableDto>();
            Assert.NotNull(created);

            // Update it
            created!.Title = "New Title";
            var putResponse = await client.PutAsJsonAsync($"/api/table/{created.Id}", created);
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            // Verify
            var getResponse = await client.GetAsync($"/api/table/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var updated = await getResponse.Content.ReadFromJsonAsync<TableDto>();
            Assert.Equal("New Title", updated!.Title);
        }

        [Fact]
        public async Task DELETE_Should_Remove_Table()
        {
            var client = _factory.CreateClient();

            // Create table
            var table = new TableDto { Title = "To delete" };
            var postResponse = await client.PostAsJsonAsync("/api/table", table);
            postResponse.EnsureSuccessStatusCode();

            var created = await postResponse.Content.ReadFromJsonAsync<TableDto>();
            Assert.NotNull(created);

            // Delete
            var deleteResponse = await client.DeleteAsync($"/api/table/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Confirm 404 on GET
            var getResponse = await client.GetAsync($"/api/table/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
