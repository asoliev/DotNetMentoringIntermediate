using System;

namespace Expressions.Task3.E3SQueryProvider.SqlProvider;

/// <summary>
/// Interface for executing SQL queries against a database
/// </summary>
public interface ISqlQueryExecutor
{
    /// <summary>
    /// Executes a SQL query and returns results
    /// </summary>
    object Execute(Type elementType, string sqlQuery);
}
