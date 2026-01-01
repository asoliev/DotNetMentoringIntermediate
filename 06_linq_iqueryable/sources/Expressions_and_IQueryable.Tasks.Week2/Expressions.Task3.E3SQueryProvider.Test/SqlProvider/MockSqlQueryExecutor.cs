using System;
using System.Collections;
using System.Collections.Generic;
using Expressions.Task3.E3SQueryProvider.SqlProvider;

namespace Expressions.Task3.E3SQueryProvider.Test.SqlProvider;

/// <summary>
/// Mock SQL executor for testing that captures generated SQL queries
/// and returns mock data
/// </summary>
public class MockSqlQueryExecutor : ISqlQueryExecutor
{
    public string LastGeneratedSql { get; private set; }

    private readonly Dictionary<Type, IEnumerable> _mockData = new();

    public void SetMockData<T>(IEnumerable<T> data)
    {
        _mockData[typeof(T)] = data;
    }

    public object Execute(Type elementType, string sqlQuery)
    {
        LastGeneratedSql = sqlQuery;

        // Return mock data if available
        if (_mockData.TryGetValue(elementType, out var data))
        {
            return data;
        }

        // Return empty collection
        var listType = typeof(List<>).MakeGenericType(elementType);
        return Activator.CreateInstance(listType);
    }
}
