using System;
using System.Linq;
using Expressions.Task3.E3SQueryProvider.SqlProvider;

namespace Expressions.Task3.E3SQueryProvider.Demo;

/// <summary>
/// Demo program showing the SQL LINQ Provider in action
/// Run this to see generated SQL queries without needing a database
/// </summary>
public class SqlLinqProviderDemo
{
    public static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("SQL LINQ Provider - Live Demo");
        Console.WriteLine("========================================\n");

        DemoBasicQueries();
        Console.WriteLine();
        DemoComplexQueries();
        Console.WriteLine();
        DemoAllOperators();
        
        Console.WriteLine("\n========================================");
        Console.WriteLine("Demo Complete! ?");
        Console.WriteLine("========================================");
    }

    static void DemoBasicQueries()
    {
        Console.WriteLine("?? BASIC QUERIES");
        Console.WriteLine("?????????????????????????????????????????\n");

        var translator = new ExpressionToSqlTranslator();

        // Example 1: Simple equality
        Console.WriteLine("1. Simple Equality:");
        var expr1 = (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)
            (p => p.ProductType == "Customised Product");
        string sql1 = translator.Translate(expr1, typeof(ProductEntity));
        Console.WriteLine($"   LINQ: p => p.ProductType == \"Customised Product\"");
        Console.WriteLine($"   SQL:  {sql1}\n");

        // Example 2: Greater than
        Console.WriteLine("2. Greater Than:");
        var expr2 = (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)
            (p => p.UnitPrice > 100);
        string sql2 = translator.Translate(expr2, typeof(ProductEntity));
        Console.WriteLine($"   LINQ: p => p.UnitPrice > 100");
        Console.WriteLine($"   SQL:  {sql2}\n");

        // Example 3: Less than
        Console.WriteLine("3. Less Than:");
        var expr3 = (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)
            (p => p.UnitsInStock < 50);
        string sql3 = translator.Translate(expr3, typeof(ProductEntity));
        Console.WriteLine($"   LINQ: p => p.UnitsInStock < 50");
        Console.WriteLine($"   SQL:  {sql3}");
    }

    static void DemoComplexQueries()
    {
        Console.WriteLine("?? COMPLEX QUERIES WITH AND");
        Console.WriteLine("?????????????????????????????????????????\n");

        var translator = new ExpressionToSqlTranslator();

        // Example 1: Simple AND
        Console.WriteLine("1. Two Conditions (AND):");
        var expr1 = (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)
            (p => p.UnitPrice > 100 && p.ProductType == "Customised Product");
        string sql1 = translator.Translate(expr1, typeof(ProductEntity));
        Console.WriteLine($"   LINQ: p => p.UnitPrice > 100 && p.ProductType == \"Customised Product\"");
        Console.WriteLine($"   SQL:  {sql1}\n");

        // Example 2: Three conditions
        Console.WriteLine("2. Three Conditions (AND):");
        var expr2 = (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)
            (p => p.UnitPrice > 50 && p.UnitPrice < 200 && p.UnitsInStock < 100);
        string sql2 = translator.Translate(expr2, typeof(ProductEntity));
        Console.WriteLine($"   LINQ: p => p.UnitPrice > 50 && p.UnitPrice < 200 && p.UnitsInStock < 100");
        Console.WriteLine($"   SQL:  {sql2}");
    }

    static void DemoAllOperators()
    {
        Console.WriteLine("?? ALL SUPPORTED OPERATORS");
        Console.WriteLine("?????????????????????????????????????????\n");

        var translator = new ExpressionToSqlTranslator();

        var examples = new[]
        {
            ("Equal (=)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.ProductType == "Premium")),
            ("Greater (>)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.UnitPrice > 100)),
            ("Less (<)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.UnitsInStock < 50)),
            ("Greater or Equal (>=)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.UnitPrice >= 100)),
            ("Less or Equal (<=)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.UnitsInStock <= 20)),
            ("AND (&&)", (System.Linq.Expressions.Expression<Func<ProductEntity, bool>>)(p => p.UnitPrice > 50 && p.UnitsInStock < 100))
        };

        foreach (var (name, expr) in examples)
        {
            string sql = translator.Translate(expr, typeof(ProductEntity));
            Console.WriteLine($"? {name}");
            Console.WriteLine($"  {sql}\n");
        }
    }
}
