using api.Dto.Common;
using api.Dto.DataTableQuery;
using api.Dto.ListQuery;
using AutoMapper;
using System.Linq.Expressions;

namespace api.Helpers;

public static class DataTableQueryResolver
{

    //TODO: Test, refactor and optimise
    //Implement datatable query suppport per https://datatables.net/manual/server-side
    public static DataTableQueryResponse<Entity> ResolveDataTableQuery<Entity>(this IQueryable<Entity> query, DataTableQuery request, DataTableQueryConfig<Entity>? config) where Entity : class
    {
        //Total record count
        var recordsTotal = query.Count();
        
        //Search
        var searchableColumns = request.Columns.Where(c => c.Searchable).ToList();
        if (!string.IsNullOrEmpty(request.Search?.Value) && searchableColumns.Any())
        {
            query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>(searchableColumns.Select(c => c.Name).ToList(), request.Search.Value));
        }

        //Filter
        var filteredColumns = request.Columns.Where(c => !string.IsNullOrEmpty(c.Search?.Value)).ToList();
        if (filteredColumns.Any())
        {
            foreach (var column in filteredColumns)
            {
                query = query.Where(ReflectionHelper.GetWhereSearchPredicate<Entity>(new List<string>() { column.Name }, column.Search!.Value));
            }
        }

        //Filtered record count
        var recordsFiltered = query.Count();


        //Order
        if (request.Order.Any())
        {
            query = OrderQuery(query, request);
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

        return new DataTableQueryResponse<Entity>()
        {
            Draw = request.Draw,
            Data = query.ToList(),
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
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
        var secondarySort = false;
        string ascendingOrderMethodName = nameof(Queryable.OrderBy), descendingOrderMethodName = nameof(Queryable.OrderByDescending);
        for(var i = 0; i < orderList.Count; i++)
        {
            var sortApplied = ApplySort(ascendingOrderMethodName, descendingOrderMethodName, orderList[i]);
            if(!secondarySort && sortApplied)
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
