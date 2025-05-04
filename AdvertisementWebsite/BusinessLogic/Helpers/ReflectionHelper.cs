using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Z.EntityFramework.Plus;

namespace BusinessLogic.Helpers;

public static class ReflectionHelper
{
    /// <summary>
    /// Invokes Generic method from passed type, method is searched by name, argument count and type parameter count. Tries to cast result to result generic type.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="objType"></param>
    /// <param name="methodName"></param>
    /// <param name="methodArgumentTypes"></param>
    /// <param name="typeParameters"></param>
    /// <param name="methodParameters"></param>
    /// <param name="currentInstance"></param>
    /// <returns>Invoked method result or null if could not cast to result type</returns>
    public static TResult InvokeGenericMethod<TResult>(Type objType, string methodName, Type[] methodArgumentTypes, Type[] typeParameters, object[] methodParameters, object? currentInstance = null) where TResult : class
    {
        var result = InvokeGenericMethod(objType, methodName, methodArgumentTypes, typeParameters, methodParameters, currentInstance);
        return (result as TResult)!;
    }

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
    public static object? InvokeGenericMethod(Type objType, string methodName, Type[] methodArgumentTypes, Type[] typeParameters, object[] methodParameters, object? currentInstance = null)
    {
        var methods = objType.GetMethods().Where(m => m.Name == methodName);

        MethodInfo methodInfo = null!;
        foreach (var method in methods)
        {
            var genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != typeParameters.Length)
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (methodArgumentTypes.Length != parameters.Length)
            {
                continue;
            }

            var continueFlag = false;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.Equals(methodArgumentTypes[i])
                    && !(parameters[i].ParameterType.IsGenericType && MatchGenericTypes(parameters[i].ParameterType, methodArgumentTypes[i])))
                {
                    continueFlag = true;
                    break;
                }
            }

            if (!continueFlag)
            {
                methodInfo = method;
                break;
            }
        }

        var generic = methodInfo.MakeGenericMethod(typeParameters);
        return generic.Invoke(currentInstance, methodParameters);
    }

    /// <summary>
    /// Compare two generic types.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool MatchGenericTypes(Type a, Type b)
    {
       
        var aGenericDefinition = a.GetGenericTypeDefinition();
       
        //Try tp match base type definitions in case type is derived class
        var baseType = b;
        var typesMatched = false;
        while (baseType != null)
        {
            Type bBaseTypeGenericDefinition;
            if (baseType.IsGenericType)
            {
                bBaseTypeGenericDefinition = baseType.GetGenericTypeDefinition();
                baseType = baseType.BaseType;
            }
            else
            {
                bBaseTypeGenericDefinition = baseType;
                baseType = null;
            }

            if(aGenericDefinition == bBaseTypeGenericDefinition)
            {
                typesMatched = true;
                break;
            }
        }

        if (!typesMatched)
        {
            return false;
        }

        //Compare generic arguments
        var aArguments = a.GetGenericArguments();
        var bArguments = b.GetGenericArguments();
        if (aArguments.Length != bArguments.Length)
        {
            return false;
        }

        for (var i = 0; i < aArguments.Length; i++)
        {
            var equalTypes = aArguments[i] == bArguments[i];
            //TODO: find a way to check is argument is assignable to generic parameter
            var assignableGenericParameter = aArguments[i].IsGenericParameter;//&& aArguments[i].IsAssignableFrom(bArguments[i]);
            var equalGenericTypes = aArguments[i].IsGenericType && MatchGenericTypes(aArguments[i], bArguments[i]);
            if (!equalTypes && !assignableGenericParameter && !equalGenericTypes)
            {
                return false;
            }
        }
        return true;
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
    /// Get lambda predicate expression. Which searches for string in objects fields. Object matches if any of the fields values contains search value, both strings are converted to lowercase. 
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
            var toLowerMethod = typeof(string).GetMethod("ToLower", BindingFlags.Public | BindingFlags.Instance, [])!;
            var containsMethod = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance, [typeof(string)])!;
            var searchValueLowercase = Expression.Call(Expression.Constant(searchValue), toLowerMethod);

            foreach (var fieldItem in searchFieldList)
            {
                //Expression for accessing Fields name property
                var entityProperty = GetPropertyExpression(pe, fieldItem);

                if (entityProperty.Type != typeof(string))
                {
                    var toStringMethod = entityProperty.Type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, []);
                    entityProperty = toStringMethod is not null ? Expression.Call(entityProperty, toStringMethod) : Expression.Constant(string.Empty);
                }

                var propertyLowercase = Expression.Call(entityProperty, toLowerMethod);
                var containsExpression = Expression.Call(propertyLowercase, containsMethod, [searchValueLowercase]);

                if (combined == null)
                {
                    combined = containsExpression;
                }
                else
                {
                    //Be sure to use Expression.OrElse to not run in to bug: https://github.com/dotnet/efcore/issues/30181
                    combined = Expression.OrElse(combined, containsExpression);
                }
            }
        }

        //create and return the predicate
        return Expression.Lambda<Func<T, bool>>(combined ?? Expression.Constant(true), [pe]);
    }

    /// <summary>
    /// Builds expression with lambda function which returns property
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

        return Expression.Lambda<Func<T, TKey>>(propertyExpression, [pe]);
    }

    public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attributeType)
    {
        return assembly.GetTypes().Where(t => t.GetCustomAttributes(attributeType, true).Length > 0).ToList();
    }

    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> a,
        Expression<Func<T, bool>> b)
    {
        return CombineExpressions(a, b, Expression.AndAlso);
    }

    public static Expression<Func<T, bool>> OrElse<T>(
        this Expression<Func<T, bool>> a,
        Expression<Func<T, bool>> b)
    {
        return CombineExpressions(a, b, Expression.OrElse);
    }

    public static Expression<Func<T, bool>> CombineExpressions<T>(
        this Expression<Func<T, bool>> a,
        Expression<Func<T, bool>> b,
        Func<Expression, Expression, Expression> operation)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(a.Parameters[0], parameter);
        var left = leftVisitor.Visit(a.Body);

        var rightVisitor = new ReplaceExpressionVisitor(b.Parameters[0], parameter);
        var right = rightVisitor.Visit(b.Body);

        return Expression.Lambda<Func<T, bool>>(
            operation(left, right), parameter);
    }

    private class ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
                : ExpressionVisitor
    {
        private readonly Expression _oldValue = oldValue;
        private readonly Expression _newValue = newValue;

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node == _oldValue)
                return _newValue;

            return base.Visit(node);
        }
    }
}
