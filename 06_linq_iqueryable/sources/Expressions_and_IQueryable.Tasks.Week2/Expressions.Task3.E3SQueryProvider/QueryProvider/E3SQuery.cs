using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Expressions.Task3.E3SQueryProvider.QueryProvider;

public class E3SQuery<T>(Expression expression, E3SLinqProvider provider) : IQueryable<T>
{
    #region public properties

    public Type ElementType => typeof(T);

    public Expression Expression { get; } = expression;

    public IQueryProvider Provider => provider;

    #endregion

    #region public methods

    public IEnumerator<T> GetEnumerator() => provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => provider.Execute<IEnumerable>(Expression).GetEnumerator();

    #endregion
}