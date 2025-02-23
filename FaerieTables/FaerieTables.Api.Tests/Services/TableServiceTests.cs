using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using FaerieTables.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaerieTables.Api.Tests.Services;

public class TableServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;

    public TableServiceTests(WebApplicationFactory<Program> factory)
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

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        db.Database.EnsureCreated(); // <-- Creates the schema from your model
    }

    ~TableServiceTests()
    {
        _connection.Close();
    }

    [Fact]
    public async Task CreateAsync_Should_Add_NewTable_To_Db()
    {
        // ARRANGE
        // Get a scoped service provider from the *modified* factory
        using var scope = _factory.Services.CreateScope();

        // Obtain the real EF context from DI
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();

        // Instantiate TableService with that context
        var service = new TableService(dbContext);

        // ACT
        var table = new Table
        {
            Id = default,
            Title = "Test Table",
            Source = "Test",
            License = "Test",
            Description = string.Empty,
            DiceRange = "1d6",
            Columns = new List<TableColumn>(),
            Rows = new List<TableRow>(),
            TableTags = new List<TableTag>()
        };
        var result = await service.CreateAsync(table);

        // ASSERT
        Assert.NotEqual(Guid.Empty, result.Id);

        // Double-check the table count from the same context
        var count = await dbContext.Tables.CountAsync();
        Assert.Equal(1, count);
    }
}