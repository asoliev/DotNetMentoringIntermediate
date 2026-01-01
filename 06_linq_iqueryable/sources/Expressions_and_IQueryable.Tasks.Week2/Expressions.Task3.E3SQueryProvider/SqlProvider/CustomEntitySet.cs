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

    public CustomEntitySet(ISqlQueryExecutor executor)
    {
        if (executor == null)
            throw new ArgumentNullException(nameof(executor));

        _provider = new SqlQueryProvider(executor);
        Expression = System.Linq.Expressions.Expression.Constant(this);
    }

    public Type ElementType => typeof(T);

    public System.Linq.Expressions.Expression Expression { get; }

    public IQueryProvider Provider => _provider;

    public System.Collections.Generic.IEnumerator<T> GetEnumerator()
    {
        return _provider.Execute<System.Collections.Generic.IEnumerable<T>>(Expression).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
