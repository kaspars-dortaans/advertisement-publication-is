using api.Dto.Common;
using api.Dto.DataTableQuery;
using api.Dto.ListQuery;
using AutoMapper;
using System.Linq.Expressions;
using System.Reflection;

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
            query = query.Where(GetWhereSearchPredicate<Entity>(searchableColumns.Select(c => c.Name).ToList(), request.Search.Value));
        }

        //Filter
        var filteredColumns = request.Columns.Where(c => !string.IsNullOrEmpty(c.Search?.Value)).ToList();
        if (filteredColumns.Any())
        {
            foreach (var column in filteredColumns)
            {
                query = query.Where(GetWhereSearchPredicate<Entity>(new List<string>() { column.Name }, column.Search!.Value));
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

    //TODO: Improve field search?
    static IQueryable<Entity> OrderQuery<Entity>(IQueryable<Entity> query, DataTableQuery request)
    {
        var order = request.Order.ToList();
        var columnName = request.Columns.ElementAt(order[0].Column).Name;
        var columnType = typeof(Entity).GetProperty(columnName)!.PropertyType;
        var keySelectorType = typeof(Func<,>).MakeGenericType(new Type[] { typeof(Entity), columnType });
        var keySelectorExpressionType = typeof(Expression<>).MakeGenericType(keySelectorType);

        var methodName = order[0].Direction == Direction.Ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

        var keySelectorLambda = InvokeGenericMethode<Expression>(
            typeof(DataTableQueryResolver),
            nameof(GetKeySelectorLambda),
            new Type[] { typeof(string) },
            new Type[] { typeof(Entity), columnType },
            new object[] { columnName });


        query = InvokeGenericMethode<IOrderedQueryable<Entity>>(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(IQueryable<Entity>), keySelectorExpressionType },
            new Type[] { typeof(Entity), columnType },
            new object[] { query, keySelectorLambda });

        if (order.Count > 1)
        {
            for (var i = 0; i < order.Count; i++)
            {
                columnName = request.Columns.ElementAt(order[i].Column).Name;
                columnType = typeof(Entity).GetProperty(columnName)!.GetType();
                methodName = order[0].Direction == Direction.Ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending);
                keySelectorLambda = InvokeGenericMethode<Expression>(
                    typeof(DataTableQueryResolver),
                    nameof(GetKeySelectorLambda),
                    new Type[] { typeof(string) },
                    new Type[] { typeof(Entity), columnType },
                    new object[] { columnName });

                query = InvokeGenericMethode<IOrderedQueryable<Entity>>(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(IQueryable<Entity>), keySelectorExpressionType },
                    new Type[] { typeof(Entity), columnType },
                    new object[] { query, keySelectorLambda });
            }
        }

        return query;
    }

    static TResult InvokeGenericMethode<TResult>(Type objType, string methodName, Type[] methodArgumentTypes, Type[] typeParameters, object[] methodParameters, object? currentInstance = null) where TResult : class
    {
        var method = objType
            .GetMethods()
            .Where(m => m.Name == methodName && m.GetGenericArguments().Length == typeParameters.Length && m.GetParameters().Length == methodArgumentTypes.Length)
            .First();

        var generic = method!.MakeGenericMethod(typeParameters);
        var result = generic.Invoke(currentInstance, methodParameters);
        return (result as TResult)!;
    }

    static Expression GetPropertyExpression(Expression pe, string chain)
    {
        var properties = chain.Split('.');
        foreach (var property in properties)
            pe = Expression.Property(pe, property);

        return pe;
    }

    /// <summary>
    /// Get lambda predicate expression. Which searches for string in objects fields. Object mathes if any of the fields values contains search value, both strings are converted to lowercase. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="searchFieldList"></param>
    /// <param name="searchValue"></param>
    /// <returns>Expression containing lambda function predicate</returns>
    public static Expression<Func<T, bool>> GetWhereSearchPredicate<T>(IEnumerable<string> searchFieldList, string searchValue)
    {
        //the 'IN' parameter for expression ie T=> condition
        ParameterExpression pe = Expression.Parameter(typeof(T), typeof(T).Name);

        //combine them with and 1=1 Like no expression
        Expression? combined = null;

        if (searchFieldList != null)
        {
            var toLowerMethod = typeof(string).GetMethod("ToLower", BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>());
            var containsMethod = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string) });
            var searchValueLowercase = Expression.Call(Expression.Constant(searchValue), toLowerMethod);

            foreach (var fieldItem in searchFieldList)
            {
                //Expression for accessing Fields name property
                var entityProperty = GetPropertyExpression(pe, fieldItem);

                if(entityProperty.Type != typeof(string))
                {
                    var toStringMethod = entityProperty.Type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, new Type[] { });
                    entityProperty = Expression.Call(entityProperty, toStringMethod);
                }

                var propertyLowercase = Expression.Call(entityProperty, toLowerMethod);
                var containsExporession = Expression.Call(propertyLowercase, containsMethod, new Expression[] { searchValueLowercase });

                if (combined == null)
                {
                    combined = containsExporession;
                }
                else
                {
                    combined = Expression.Or(combined, containsExporession);
                }
            }
        }

        //create and return the predicate
        return Expression.Lambda<Func<T, bool>>(combined ?? Expression.Constant(true), new ParameterExpression[] { pe });
    }

    public static Expression<Func<T, TKey>> GetKeySelectorLambda<T, TKey>(string fieldName)
    {
        //the 'IN' parameter for expression ie T=> condition
        ParameterExpression pe = Expression.Parameter(typeof(T), typeof(T).Name);

        var propertyExpression = GetPropertyExpression(pe, fieldName);

        return Expression.Lambda<Func<T, TKey>>(propertyExpression, new ParameterExpression[] { pe });
    }

}
