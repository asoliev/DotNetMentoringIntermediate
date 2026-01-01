using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Expressions.Task3.E3SQueryProvider.SqlProvider;

/// <summary> IQueryable implementation for SQL queries </summary>
public class SqlQuery<T> : IOrderedQueryable<T>
{
    public Expression Expression { get; }
    public Type ElementType => typeof(T);
    public IQueryProvider Provider { get; }

    public SqlQuery(SqlQueryProvider provider)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Expression = Expression.Constant(this);
    }

    public SqlQuery(SqlQueryProvider provider, Expression expression)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            throw new ArgumentException($"Expression must be assignable to IQueryable<{typeof(T).Name}>");
    }

    public IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}