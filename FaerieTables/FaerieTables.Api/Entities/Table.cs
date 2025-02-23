namespace FaerieTables.Api.Entities
{
    public class Table
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Source { get; set; } = null!;
        public string License { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DiceRange { get; set; } = null!;

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

    public class Session
    {
        public Guid SessionId { get; set; }
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Navigation
        public ICollection<Roll> Rolls { get; set; } = new List<Roll>();
    }

    public class Roll
    {
        public Guid RollId { get; set; }
        public Guid SessionId { get; set; }
        public Guid TableId { get; set; }
        public string TableTitle { get; set; } = null!;
        public string Mode { get; set; } = null!;
        public DateTime Timestamp { get; set; }

        // Navigation
        public Session? Session { get; set; }
        public ICollection<RollResult> RollResults { get; set; } = new List<RollResult>();
    }

    public class RollResult
    {
        public Guid Id { get; set; }
        public Guid RollId { get; set; }
        public Guid TableColumnId { get; set; }
        public string Value { get; set; } = null!;

        // Navigation
        public Roll? Roll { get; set; }
        public TableColumn? TableColumn { get; set; }
    }
}
