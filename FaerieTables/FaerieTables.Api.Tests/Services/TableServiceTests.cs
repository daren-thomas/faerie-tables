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
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        var service = new TableService(dbContext);

        var table = new Table
        {
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

        Assert.NotEqual(Guid.Empty, result.Id);
        var count = await dbContext.Tables.CountAsync();
        Assert.Equal(1, count);
    }



    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_Not_Found()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        var service = new TableService(dbContext);

        var result = await service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        var service = new TableService(dbContext);

        var table = new Table { Title = "Old Title" };
        var created = await service.CreateAsync(table);

        // Act
        created.Title = "New Title";
        await service.UpdateAsync(created);

        // Assert
        using var scope2 = _factory.Services.CreateScope();
        await using var context = GetRandomTableContext(scope2);
        var updated = await context.Tables.FindAsync(created.Id);
        Assert.Equal("New Title", updated?.Title);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Table()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        var service = new TableService(dbContext);

        var table = new Table { Title = "To be removed" };
        var created = await service.CreateAsync(table);

        await service.DeleteAsync(created);

        using var scope2 = _factory.Services.CreateScope();
        await using var context = GetRandomTableContext(scope2);
        var found = await context.Tables.FindAsync(created.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_Search_Should_Filter_Results()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        var service = new TableService(dbContext);

        // Create a few test tables
        await service.CreateAsync(new Table { Title = "Dungeon" });
        await service.CreateAsync(new Table { Title = "Dragon" });
        await service.CreateAsync(new Table { Title = "Goblin" });

        // Act
        var results1 = await service.GetAllAsync("drag");
        var results2 = await service.GetAllAsync("dung");
        var results3 = await service.GetAllAsync("xyz");

        // Assert
        Assert.Single(results1); // "Dragon"
        Assert.Single(results2); // "Dungeon"
        Assert.Empty(results3); // none match "xyz"
    }

    private RandomTableContext GetRandomTableContext(IServiceScope scope)
    {
        // Obtain the real EF context from DI
        var dbContext = scope.ServiceProvider.GetRequiredService<RandomTableContext>();
        return dbContext;
    }
}