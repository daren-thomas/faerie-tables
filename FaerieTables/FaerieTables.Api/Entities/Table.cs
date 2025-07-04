﻿using System.Text.Json.Serialization;

namespace FaerieTables.Api.Entities;

public class Table
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Source { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiceRange { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<TableColumn> Columns { get; set; } = new List<TableColumn>();
    public ICollection<TableRow> Rows { get; set; } = new List<TableRow>();
    public ICollection<TableTag> TableTags { get; set; } = new List<TableTag>();
}

public class TableColumn
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;

    // Navigation
    public Table? Table { get; set; }
}

public class TableRow
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }

    // Navigation
    public Table? Table { get; set; }
    public ICollection<RowValue> RowValues { get; set; } = new List<RowValue>();
}

public class RowValue
{
    public Guid Id { get; set; }
    public Guid RowId { get; set; }
    public Guid ColumnId { get; set; }
    public string Value { get; set; } = null!;

    // Navigation
    public TableRow? Row { get; set; }
    public TableColumn? Column { get; set; }
}

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<TableTag> TableTags { get; set; } = new List<TableTag>();
}

// Many-to-many join entity for Table <-> Tag
public class TableTag
{
    public Guid TableId { get; set; }
    public Guid TagId { get; set; }

    // Navigation
    public Table? Table { get; set; }
    public Tag? Tag { get; set; }
}
