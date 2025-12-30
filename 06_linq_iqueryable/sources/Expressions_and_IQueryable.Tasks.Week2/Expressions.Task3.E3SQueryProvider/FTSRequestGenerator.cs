using Expressions.Task3.E3SQueryProvider.Attributes;
using Expressions.Task3.E3SQueryProvider.Models.Request;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Expressions.Task3.E3SQueryProvider;

public class FtsRequestGenerator(string baseAddress)
{
    private readonly string _FTSSearchTemplate = @"/searchFts";

    #region public methods

    public Uri GenerateRequestUrl<T>(string query = "*", int start = 0, int limit = 10) 
        => GenerateRequestUrl(typeof(T), query, start, limit);

    public Uri GenerateRequestUrl(Type type, string query = "*", int start = 0, int limit = 10)
    {
        string metaTypeName = GetMetaTypeName(type);

        var ftsQueryRequest = new FtsQueryRequest
        {
            // TASK 3: Parse query string into multiple statements if AND operator is present
            Statements = ParseQueryIntoStatements(query),
            Start = start,
            Limit = limit
        };

        var ftsQueryRequestString = JsonConvert.SerializeObject(ftsQueryRequest);

        var uri = BindByName(
            $"{baseAddress}{_FTSSearchTemplate}",
            new Dictionary<string, string>()
            {
                { "metaType", metaTypeName },
                { "query", ftsQueryRequestString }
            }
        );

        return uri;
    }

    #endregion

    #region private methods

    /// <summary>
    /// Parses query string into separate Statement objects
    /// If query contains " AND ", splits it into multiple statements
    /// Otherwise, returns a single statement
    /// </summary>
    private static List<Statement> ParseQueryIntoStatements(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<Statement> { new Statement { Query = "*" } };
        }

        // Check if query contains AND operator
        if (query.Contains(" AND "))
        {
            // Split by AND and create separate statements
            var queries = query.Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            return queries.Select(q => new Statement { Query = q.Trim() }).ToList();
        }

        // Single statement
        return new List<Statement> { new Statement { Query = query } };
    }

    private static Uri BindByName(string baseAddress, Dictionary<string, string> queryParams)
        => new(QueryHelpers.AddQueryString(baseAddress, queryParams));

    private static string GetMetaTypeName(Type type)
    {
        var attributes = type.GetCustomAttributes(typeof(E3SMetaTypeAttribute), false);

        if (attributes.Length == 0)
            throw new Exception($"Entity {type.FullName} do not have attribute E3SMetaType");

        return ((E3SMetaTypeAttribute)attributes[0]).Name;
    }

    #endregion
}