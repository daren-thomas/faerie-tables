using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System;

namespace FaerieTables.Web.Pages;

public partial class TableEditor
{
    private void AddColumn()
    {
        if (table == null) return;
        table.Columns.Add(new TableColumnDto
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            Name = string.Empty,
            Type = "text"
        });
    }

    private void DeleteColumn(TableColumnDto column)
    {
        table?.Columns.Remove(column);
    }

    private void AddRow()
    {
        if (table == null) return;
        table.Rows.Add(new TableRowDto
        {
            Id = Guid.NewGuid(),
            TableId = table.Id
        });
    }

    private void DeleteRow(TableRowDto row)
    {
        table?.Rows.Remove(row);
    }
}
