using Expressions.Task3.E3SQueryProvider.Helpers;
using Expressions.Task3.E3SQueryProvider.Services;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Expressions.Task3.E3SQueryProvider.QueryProvider;

public class E3SLinqProvider(E3SSearchService client) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new E3SQuery<TElement>(expression, this);

    public object Execute(Expression expression)
    {
        throw new NotImplementedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        Type itemType = TypeHelper.GetElementType(expression.Type);

        var translator = new ExpressionToFtsRequestTranslator();
        string queryString = translator.Translate(expression);

        return (TResult)client.SearchFts(itemType, queryString);
    }
}