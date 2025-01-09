using System.Linq.Expressions;
using System.Reflection;

namespace BusinessLogic.Helpers;

public static class ReflectionHelper
{
    /// <summary>
    /// Invokes Generic method from passed type, method is searched by name, argument count and type parameter count
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="objType"></param>
    /// <param name="methodName"></param>
    /// <param name="methodArgumentTypes"></param>
    /// <param name="typeParameters"></param>
    /// <param name="methodParameters"></param>
    /// <param name="currentInstance"></param>
    /// <returns>Invoked method result</returns>
    public static TResult InvokeGenericMethode<TResult>(Type objType, string methodName, Type[] methodArgumentTypes, Type[] typeParameters, object[] methodParameters, object? currentInstance = null) where TResult : class
    {
        var method = objType
            .GetMethods()
            .Where(m => m.Name == methodName && m.GetGenericArguments().Length == typeParameters.Length && m.GetParameters().Length == methodArgumentTypes.Length)
            .First();

        var generic = method!.MakeGenericMethod(typeParameters);
        var result = generic.Invoke(currentInstance, methodParameters);
        return (result as TResult)!;
    }

    /// <summary>
    /// Builds property expression ()
    /// </summary>
    /// <param name="parameterExpression"></param>
    /// <param name="chain"></param>
    /// <returns>Property expression</returns>
    public static Expression GetPropertyExpression(Expression parameterExpression, string chain)
    {
        var properties = chain.Split('.');
        foreach (var property in properties)
            parameterExpression = Expression.Property(parameterExpression, property);

        return parameterExpression;
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

        if (searchFieldList is not null)
        {
            var toLowerMethod = typeof(string).GetMethod("ToLower", BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>())!;
            var containsMethod = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string) })!;
            var searchValueLowercase = Expression.Call(Expression.Constant(searchValue), toLowerMethod);

            foreach (var fieldItem in searchFieldList)
            {
                //Expression for accessing Fields name property
                var entityProperty = GetPropertyExpression(pe, fieldItem);

                if (entityProperty.Type != typeof(string))
                {
                    var toStringMethod = entityProperty.Type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, new Type[] { });
                    entityProperty = toStringMethod is not null ? Expression.Call(entityProperty, toStringMethod) : Expression.Constant(string.Empty);
                }

                var propertyLowercase = Expression.Call(entityProperty, toLowerMethod);
                var containsExporession = Expression.Call(propertyLowercase, containsMethod, new Expression[] { searchValueLowercase });

                if (combined == null)
                {
                    combined = containsExporession;
                }
                else
                {
                    //Be sure to use Expression.OrElse to not run in to bug: https://github.com/dotnet/efcore/issues/30181
                    combined = Expression.OrElse(combined, containsExporession);
                }
            }
        }

        //create and return the predicate
        return Expression.Lambda<Func<T, bool>>(combined ?? Expression.Constant(true), new ParameterExpression[] { pe });
    }

    /// <summary>
    /// Builds expression with lamba function which returns property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="fieldName"></param>
    /// <returns>Expression with lambda function which returns property</returns>
    public static Expression<Func<T, TKey>> GetKeySelectorLambda<T, TKey>(string fieldName)
    {
        //the 'IN' parameter for expression ie T=> condition
        ParameterExpression pe = Expression.Parameter(typeof(T), typeof(T).Name);

        var propertyExpression = GetPropertyExpression(pe, fieldName);

        return Expression.Lambda<Func<T, TKey>>(propertyExpression, new ParameterExpression[] { pe });
    }
}
