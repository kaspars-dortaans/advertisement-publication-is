using AutoMapper;
using BusinessLogic.Dto.DataTableQuery;
using System.Linq.Expressions;
using System.Reflection;
using Z.EntityFramework.Plus;
namespace BusinessLogic.Helpers;

public static class DataTableQueryResolver
{
    public static async Task<DataTableQueryResponse<Entity>> ResolveDataTableQuery<Entity>(this IQueryable<Entity> query, DataTableQuery request, DataTableQueryConfig<Entity>? config = null) where Entity : class, new()
    {
        //Total record count
        var totalRecords = query.DeferredCount().FutureValue();

        //Search
        var searchableColumns = request.Columns.Where(c => c.Searchable).ToList();
        if (!string.IsNullOrEmpty(request.Search?.Value) && searchableColumns.Count > 0)
        {
            query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>(searchableColumns.Select(c => c.Data).ToList(), request.Search.Value));
        }

        //Filter
        var filteredColumns = request.Columns.Where(c => !string.IsNullOrEmpty(c.Search?.Value)).ToList();
        if (filteredColumns.Count > 0)
        {
            foreach (var column in filteredColumns)
            {
                query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>([column.Data], column.Search!.Value));
            }
        }
        if (config?.AdditionalFilter is not null)
        {
            query = config.AdditionalFilter(query);
        }

        //Filtered record count
        var filteredRecordCount = query.DeferredCount().FutureValue();

        //Aggregates
        var aggregates = request.Columns
            .Where(c => c.Aggregate)
            .ToDictionary(c => c.Name, c => BuildAggregateFunction(query, c));

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
        var response = new DataTableQueryResponse<Entity>()
        {
            RecordsTotal = totalRecords.Value,
            RecordsFiltered = filteredRecordCount.Value,
            Data = queryResult,
            Aggregates = new Dictionary<string, dynamic>(aggregates
                .Where(p => p.Value != null)
                .Select(p => new KeyValuePair<string, dynamic>(p.Key, p.Value!.Value)))
        };
        return response;
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
            Data = mapper.Map<IEnumerable<TDestination>>(response.Data, opts),
            Aggregates = response.Aggregates
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
            var columnName = request.Columns.ElementAt(order.Column).Data;
            var columnType = typeof(Entity).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.PropertyType;
            if (columnType == null)
            {
                return false;
            }

            var keySelectorType = typeof(Func<,>).MakeGenericType([typeof(Entity), columnType]);
            var keySelectorExpressionType = typeof(Expression<>).MakeGenericType(keySelectorType);
            var methodName = order.Direction == Direction.Ascending ? orderAscendingMethodName : orderDescendingMethodName;

            var keySelectorLambda = ReflectionHelper.InvokeGenericMethod<Expression>(
                typeof(ReflectionHelper),
                nameof(ReflectionHelper.GetKeySelectorLambda),
                [typeof(string)],
                [typeof(Entity), columnType],
                [columnName]);


            query = ReflectionHelper.InvokeGenericMethod<IOrderedQueryable<Entity>>(
                typeof(Queryable),
                methodName,
                [typeof(IQueryable<Entity>), keySelectorExpressionType],
                [typeof(Entity), columnType],
                [query, keySelectorLambda]);

            return true;
        }
    }

    static dynamic? BuildAggregateFunction<Entity>(IQueryable<Entity> query, TableColumn column)
    {
        var entityType = typeof(Entity);
        var propertyType = entityType.GetProperty(column.Data, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.PropertyType;
        if (propertyType == null)
        {
            return null;
        }
        var keySelector = ReflectionHelper.InvokeGenericMethod<object>(
            typeof(ReflectionHelper),
            nameof(ReflectionHelper.GetKeySelectorLambda),
            [typeof(string)],
            [entityType, propertyType],
            [column.Data]);

        var deferredQuery = ReflectionHelper.InvokeGenericMethod(
            typeof(QueryDeferredExtensions),
            nameof(QueryDeferredExtensions.DeferredSum),
            [typeof(IQueryable<Entity>), keySelector.GetType()],
            [entityType],
            [query, keySelector]);

        if (deferredQuery == null)
        {
            return null;
        }

        var futureValue = ReflectionHelper.InvokeGenericMethod(
            typeof(QueryFutureExtensions),
            nameof(QueryFutureExtensions.FutureValue),
            [deferredQuery.GetType()],
            [propertyType],
            [deferredQuery]);

        return futureValue;
    }
}
