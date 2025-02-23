using Microsoft.EntityFrameworkCore;

namespace FaerieTables.Api.Data;

public class RandomTableContext(DbContextOptions<RandomTableContext> options) : DbContext(options)
{
    // No entities yet, as instructed. 
    // Later you might add DbSet<Table> Tables, DbSet<Column> Columns, etc.
}