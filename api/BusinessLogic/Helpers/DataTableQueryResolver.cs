using AutoMapper;
using BusinessLogic.Dto.DataTableQuery;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;
namespace BusinessLogic.Helpers;

public static class DataTableQueryResolver
{

    //Implement datatable query support per https://datatables.net/manual/server-side
    public static async Task<DataTableQueryResponse<Entity>> ResolveDataTableQuery<Entity>(this IQueryable<Entity> query, DataTableQuery request, DataTableQueryConfig<Entity>? config = null) where Entity : class
    {
        //Total record count
        var totalRecords = query.DeferredCount().FutureValue();

        //Search
        var searchableColumns = request.Columns.Where(c => c.Searchable).ToList();
        if (!string.IsNullOrEmpty(request.Search?.Value) && searchableColumns.Count > 0)
        {
            query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>(searchableColumns.Select(c => c.Name).ToList(), request.Search.Value));
        }

        //Filter
        var filteredColumns = request.Columns.Where(c => !string.IsNullOrEmpty(c.Search?.Value)).ToList();
        if (filteredColumns.Count > 0)
        {
            foreach (var column in filteredColumns)
            {
                query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>(new List<string>() { column.Name }, column.Search!.Value));
            }
        }
        if (config?.AdditionalFilter is not null)
        {
            query = config.AdditionalFilter(query);
        }

        //Filtered record count
        var filteredRecordCount = query.DeferredCount().FutureValue();

        //Order
        var orderApplied = false;
        if (request.Order.Any())
        {
            query = OrderQuery(query, request);
            orderApplied = true;
        }
        if (config?.AdditionalSort is not null)
        {
            query = config.AdditionalSort(query, orderApplied);
        }

        //Skip
        if (request.Start.HasValue)
        {
            query = query.Skip(request.Start.Value);
        }

        //Take
        if (request.Length.HasValue && request.Length.Value > 0)
        {
            query = query.Take(request.Length.Value);
        }

        var queryResult = await query.Future().ToListAsync();
        return new DataTableQueryResponse<Entity>()
        {
            Draw = request.Draw,
            RecordsTotal = totalRecords.Value,
            RecordsFiltered = filteredRecordCount.Value,
            Data = queryResult
        };
    }

    /// <summary>
    /// Map data table response Data property to different Dto
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="response">Data table query response instance</param>
    /// <param name="mapper">Mapper instance</param>
    /// <param name="opts">Mapping options passed to mapper</param>
    /// <returns></returns>
    public static DataTableQueryResponse<TDestination> MapDataTableResponse<TSource, TDestination>(
        this DataTableQueryResponse<TSource> response,
        IMapper mapper,
        Action<IMappingOperationOptions<object, IEnumerable<TDestination>>> opts)
    {
        return new DataTableQueryResponse<TDestination>()
        {
            Draw = response.Draw,
            Error = response.Error,
            RecordsFiltered = response.RecordsFiltered,
            RecordsTotal = response.RecordsTotal,
            Data = mapper.Map<IEnumerable<TDestination>>(response.Data, opts)
        };
    }

    /// <summary>
    /// Maps DataTableQueryResponse data to specified type
    /// </summary>
    /// <typeparam name="Entity"></typeparam>
    /// <typeparam name="Dto"></typeparam>
    /// <param name="mapper"></param>
    /// <param name="response"></param>
    /// <returns>DataTableResponse with mapped response data</returns>
    public static DataTableQueryResponse<Dto> MapDataTableResult<Entity, Dto>(this IMapper mapper, DataTableQueryResponse<Entity> response)
    {
        return new DataTableQueryResponse<Dto>()
        {
            Data = mapper.Map<IEnumerable<Entity>, IEnumerable<Dto>>(response.Data),
            Draw = response.Draw,
            Error = response.Error,
            RecordsFiltered = response.RecordsFiltered,
            RecordsTotal = response.RecordsTotal
        };
    }

    /// <summary>
    /// Applies sorting to IQueryable described with DataTableQuery
    /// </summary>
    /// <typeparam name="Entity"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <returns>IQueryable with applied sorting</returns>
    static IQueryable<Entity> OrderQuery<Entity>(IQueryable<Entity> query, DataTableQuery request)
    {
        var orderList = request.Order.ToList();
        string ascendingOrderMethodName = nameof(Queryable.OrderBy), descendingOrderMethodName = nameof(Queryable.OrderByDescending);
        for (var i = 0; i < orderList.Count; i++)
        {
            var sortApplied = ApplySort(ascendingOrderMethodName, descendingOrderMethodName, orderList[i]);
            if (sortApplied)
            {
                ascendingOrderMethodName = nameof(Queryable.ThenBy);
                descendingOrderMethodName = nameof(Queryable.ThenByDescending);
            }
        }

        return query;

        //Returns true if sort was applied
        bool ApplySort(string orderAscendingMethodName, string orderDescendingMethodName, OrderQuery order)
        {
            var columnName = request.Columns.ElementAt(order.Column).Name;
            var columnType = typeof(Entity).GetProperty(columnName)?.PropertyType;
            if (columnType == null)
            {
                return false;
            }

            var keySelectorType = typeof(Func<,>).MakeGenericType(new Type[] { typeof(Entity), columnType });
            var keySelectorExpressionType = typeof(Expression<>).MakeGenericType(keySelectorType);
            var methodName = order.Direction == Direction.Ascending ? orderAscendingMethodName : orderDescendingMethodName;

            var keySelectorLambda = ReflectionHelper.InvokeGenericMethode<Expression>(
                typeof(ReflectionHelper),
                nameof(ReflectionHelper.GetKeySelectorLambda),
                new Type[] { typeof(string) },
                new Type[] { typeof(Entity), columnType },
                new object[] { columnName });


            query = ReflectionHelper.InvokeGenericMethode<IOrderedQueryable<Entity>>(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(IQueryable<Entity>), keySelectorExpressionType },
                new Type[] { typeof(Entity), columnType },
                new object[] { query, keySelectorLambda });

            return true;
        }
    }
}
