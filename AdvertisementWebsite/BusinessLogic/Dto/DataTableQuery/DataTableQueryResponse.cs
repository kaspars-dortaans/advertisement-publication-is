using BusinessLogic.Helpers;

namespace BusinessLogic.Dto.DataTableQuery;

[IncludeInOpenApi]
public class DataTableQueryResponse<T>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public IEnumerable<T> Data { get; set; } = [];
    public IDictionary<string, dynamic> Aggregates { get; set; } = default!;
    public string? Error { get; set; }
}
