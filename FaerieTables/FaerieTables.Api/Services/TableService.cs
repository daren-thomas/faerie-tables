using FaerieTables.Api.Data;
using FaerieTables.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaerieTables.Api.Services;

public interface ITableService
{
    Task<List<Table>> GetAllAsync(string? search);
    Task<Table?> GetByIdAsync(Guid id);
    Task<Table> CreateAsync(Table table);
    Task UpdateAsync(Table table);
    Task DeleteAsync(Table table);
}

public class TableService : ITableService
{
    private readonly RandomTableContext _context;

    public TableService(RandomTableContext context)
    {
        _context = context;
    }

    public async Task<List<Table>> GetAllAsync(string? search)
    {
        IQueryable<Table> query = _context.Tables;

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                t.Description.ToLower().Contains(search) ||
                t.Source.ToLower().Contains(search)
            );
        }

        return await query.ToListAsync();
    }

    public async Task<Table?> GetByIdAsync(Guid id)
    {
        return await _context.Tables
            .Include(t => t.Columns)
            .Include(t => t.Rows)
                .ThenInclude(r => r.RowValues)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Table> CreateAsync(Table table)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();
        return table;
    }

    public async Task UpdateAsync(Table table)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Table table)
    {
        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();
    }
}