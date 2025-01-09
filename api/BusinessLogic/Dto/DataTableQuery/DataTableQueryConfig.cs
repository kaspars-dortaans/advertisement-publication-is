namespace BusinessLogic.Dto.DataTableQuery;

public class DataTableQueryConfig<TResult> where TResult : class
{
    public Func<IQueryable<TResult>, bool, IQueryable<TResult>> AdditionalSort { get; set; } = default!;
    public Func<IQueryable<TResult>, IQueryable<TResult>> AdditionalFilter { get; set; } = default!;
}
