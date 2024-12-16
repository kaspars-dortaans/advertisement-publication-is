namespace BusinessLogic.Dto.DataTableQuery;

public class DataTableQuery
{
    public int Draw { get; set; }
    public int? Start { get; set; }
    public int? Length { get; set; }
    public SearchQuery? Search { get; set; } = default!;
    public IEnumerable<OrderQuery> Order { get; set; } = new List<OrderQuery>();
    public IEnumerable<TableColumn> Columns { get; set; } = new List<TableColumn>();
}

public class SearchQuery
{
    public string Value { get; set; } = default!;
    public bool Regex { get; set; }
}

public class OrderQuery
{
    public int Column { get; set; }
    public string Direction { get; set; } = default!;
}

public class TableColumn
{
    public string Data { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool Searchable { get; set; }
    public bool Orderable { get; set; }
    public SearchQuery? Search { get; set; }
}

public static class Direction
{
    public const string Ascending = "asc";
    public const string Descending = "desc";
}