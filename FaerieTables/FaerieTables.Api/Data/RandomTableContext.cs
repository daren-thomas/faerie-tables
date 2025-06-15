using FaerieTables.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaerieTables.Api.Data;

public class RandomTableContext : DbContext
{
    public RandomTableContext(DbContextOptions<RandomTableContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<TableColumn> TableColumns => Set<TableColumn>();
    public DbSet<TableRow> TableRows => Set<TableRow>();
    public DbSet<RowValue> RowValues => Set<RowValue>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TableTag> TableTags => Set<TableTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TableTag composite key
        modelBuilder.Entity<TableTag>()
            .HasKey(tt => new { tt.TableId, tt.TagId });

        // Table -> TableColumn (1-to-many)
        modelBuilder.Entity<Table>()
            .HasMany(t => t.Columns)
            .WithOne(c => c.Table)
            .HasForeignKey(c => c.TableId);

        // Table -> TableRow (1-to-many)
        modelBuilder.Entity<Table>()
            .HasMany(t => t.Rows)
            .WithOne(r => r.Table)
            .HasForeignKey(r => r.TableId);

        // TableRow -> RowValue (1-to-many)
        modelBuilder.Entity<TableRow>()
            .HasMany(row => row.RowValues)
            .WithOne(rv => rv.Row)
            .HasForeignKey(rv => rv.RowId);

        // Table -> TableTag (1-to-many)
        modelBuilder.Entity<Table>()
            .HasMany(t => t.TableTags)
            .WithOne(tt => tt.Table)
            .HasForeignKey(tt => tt.TableId);

        // Tag -> TableTag (1-to-many)
        modelBuilder.Entity<Tag>()
            .HasMany(tg => tg.TableTags)
            .WithOne(tt => tt.Tag)
            .HasForeignKey(tt => tt.TagId);
    }
}