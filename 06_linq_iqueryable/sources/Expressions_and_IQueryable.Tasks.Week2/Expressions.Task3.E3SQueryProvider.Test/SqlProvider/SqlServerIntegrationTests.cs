using System;
using System.Linq;
using Expressions.Task3.E3SQueryProvider.SqlProvider;
using Xunit;
using Xunit.Abstractions;

namespace Expressions.Task3.E3SQueryProvider.Test.SqlProvider;

/// <summary>
/// Integration tests for SqlServerQueryExecutor with real database
/// NOTE: These tests are skipped by default. To run them:
/// 1. Set up a SQL Server database
/// 2. Create the Products table
/// 3. Update the connection string
/// 4. Remove the Skip attribute
/// </summary>
public class SqlServerIntegrationTests(ITestOutputHelper output)
{

    // Update this connection string to point to your SQL Server
    private const string ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;";

    [Fact(Skip = "Requires SQL Server setup - Enable manually")]
    public void TestRealDatabaseQuery_WithAnd()
    {
        /*
        To enable this test:
        1. Create a database and run this SQL:
        
        CREATE TABLE [dbo].[Products] (
            [ProductId] INT PRIMARY KEY IDENTITY,
            [ProductName] NVARCHAR(100),
            [UnitPrice] DECIMAL(18,2),
            [ProductType] NVARCHAR(50),
            [UnitsInStock] INT
        );
        
        INSERT INTO [dbo].[Products] VALUES 
            ('Premium Widget', 150.00, 'Customised Product', 30),
            ('Standard Widget', 50.00, 'Standard Product', 100),
            ('Deluxe Gadget', 200.00, 'Customised Product', 15);
        
        2. Update ConnectionString above
        3. Remove Skip attribute
        */
        
        // Arrange
        var executor = new SqlServerQueryExecutor(ConnectionString);
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var results = productSet
            .Where(p => p.UnitPrice > 100 && p.ProductType == "Customised Product")
            .ToList();

        // Assert
        output.WriteLine($"Found {results.Count} products");
        foreach (var product in results)
        {
            output.WriteLine($"- {product.ProductName}: ${product.UnitPrice}");
        }

        Assert.NotEmpty(results);
        Assert.All(results, p =>
        {
            Assert.True(p.UnitPrice > 100);
            Assert.Equal("Customised Product", p.ProductType);
        });
    }

    [Fact]
    public void TestSqlGeneration_WithoutDatabase()
    {
        // This test doesn't need a database - just verifies SQL generation
        
        // Arrange
        var translator = new ExpressionToSqlTranslator();
        System.Linq.Expressions.Expression<Func<ProductEntity, bool>> expression = 
            p => p.UnitPrice > 100 && p.ProductType == "Customised Product";

        // Act
        string sql = translator.Translate(expression, typeof(ProductEntity));

        // Assert
        output.WriteLine($"Generated SQL: {sql}");
        Assert.Contains("SELECT * FROM", sql);
        Assert.Contains("[UnitPrice] > 100", sql);
        Assert.Contains("[ProductType] = 'Customised Product'", sql);
        Assert.Contains("AND", sql);
    }

    [Fact]
    public void TestComplexSqlGeneration()
    {
        // Arrange
        var translator = new ExpressionToSqlTranslator();
        System.Linq.Expressions.Expression<Func<ProductEntity, bool>> expression = 
            p => p.UnitPrice > 50 && p.UnitPrice < 200 && p.UnitsInStock < 100;

        // Act
        string sql = translator.Translate(expression, typeof(ProductEntity));

        // Assert
        output.WriteLine($"Generated SQL: {sql}");
        // ProductEntity has [Table("Products", Schema = "dbo")] attribute
        string expected = "SELECT * FROM [dbo].[Products] WHERE (([UnitPrice] > 50 AND [UnitPrice] < 200) AND [UnitsInStock] < 100)";
        Assert.Equal(expected, sql);
    }
}
