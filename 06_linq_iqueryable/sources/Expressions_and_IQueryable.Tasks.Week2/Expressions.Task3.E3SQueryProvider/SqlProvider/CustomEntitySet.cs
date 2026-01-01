using System;
using System.Linq;

namespace Expressions.Task3.E3SQueryProvider.SqlProvider;

/// <summary>
/// Custom entity set that implements IQueryable for SQL database queries
/// This is equivalent to DbSet in Entity Framework
/// </summary>
public class CustomEntitySet<T> : IOrderedQueryable<T>
{
    private readonly SqlQueryProvider _provider;
    public IQueryProvider Provider => _provider;

    public Type ElementType => typeof(T);

    public System.Linq.Expressions.Expression Expression { get; }

    public CustomEntitySet(ISqlQueryExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);

        _provider = new SqlQueryProvider(executor);
        Expression = System.Linq.Expressions.Expression.Constant(this);
    }

    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => _provider.Execute<System.Collections.Generic.IEnumerable<T>>(Expression).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}