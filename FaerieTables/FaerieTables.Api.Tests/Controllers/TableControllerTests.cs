using System.Net;
using System.Net.Http.Json;
using FaerieTables.Api.Data;
using FaerieTables.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaerieTables.Api.Tests.Controllers;

public class TableControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;

    public TableControllerTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the normal DB with in-memory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RandomTableContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services
                    .AddDbContext<RandomTableContext>(opts => { opts.UseSqlite(_connection); });
            });
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
            db.Database.EnsureCreated(); // <-- Creates the schema from your model
        }
    }


    ~TableControllerTests()
    {
        _connection.Close();
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
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Integration Test Table", created.Title);
    }
}