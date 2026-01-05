using Expressions.Task3.E3SQueryProvider.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Expressions.Task3.E3SQueryProvider.SqlProvider;

/// <summary>
/// IQueryProvider implementation for SQL database queries
/// </summary>
public class SqlQueryProvider(ISqlQueryExecutor executor) : IQueryProvider
{
    private readonly ISqlQueryExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));

    public IQueryable CreateQuery(Expression expression)
    {
        Type elementType = TypeHelper.GetElementType(expression.Type);
        try
        {
            return (IQueryable)Activator.CreateInstance(
                typeof(SqlQuery<>).MakeGenericType(elementType),
                [this, expression]);
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            throw ex.InnerException;
        }
    }
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new SqlQuery<TElement>(this, expression);

    public object Execute(Expression expression) => Execute<object>(expression);
    public TResult Execute<TResult>(Expression expression)
    {
        Type elementType = TypeHelper.GetElementType(expression.Type);
        
        var translator = new ExpressionToSqlTranslator();
        string sqlQuery = translator.Translate(expression, elementType);
        
        return (TResult)_executor.Execute(elementType, sqlQuery);
    }
}