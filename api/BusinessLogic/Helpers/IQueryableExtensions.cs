using System.Linq.Expressions;

namespace BusinessLogic.Helpers;

public static class IQueryableExtensions
{
    /// <summary>
    /// Apply Where function with provided predicate if filterValue is not null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="filterValue"></param>
    /// <param name="filterPredicate"></param>
    /// <returns></returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, object? filterValue, Expression<Func<T, bool>> filterPredicate)
    {
        if(filterValue is null)
        {
            return query;
        }
        return query.Where(filterPredicate);
    }
}
