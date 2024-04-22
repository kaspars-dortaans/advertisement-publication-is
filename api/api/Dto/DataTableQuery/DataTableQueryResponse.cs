namespace api.Dto.DataTableQuery
{
    public class DataTableQueryResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public string? Error { get; set; }
    }
}
