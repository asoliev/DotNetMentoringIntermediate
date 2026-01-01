using System;
using System.Linq;
using Expressions.Task3.E3SQueryProvider.SqlProvider;
using Xunit;
using Xunit.Abstractions;

namespace Expressions.Task3.E3SQueryProvider.Test.SqlProvider;

/// <summary>
/// Tests for the Complex Task: SQL LINQ Provider implementation
/// These tests demonstrate:
/// 1. Translation of LINQ queries to SQL
/// 2. Support for WHERE clause with =, >, < operators
/// 3. Support for AND operator
/// 4. Support for numeric and string data types
/// 5. CustomEntitySet usage (similar to DbSet)
/// </summary>
public class SqlLinqProviderTests
{
    private readonly ITestOutputHelper _output;

    public SqlLinqProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestSimpleWhere_EqualOperator_String()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => p.ProductType == "Customised Product");
        var _ = query.ToList(); // Execute query

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [ProductType] = 'Customised Product'";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestSimpleWhere_GreaterThan_Numeric()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => p.UnitPrice > 100);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [UnitPrice] > 100";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestSimpleWhere_LessThan_Numeric()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => p.UnitsInStock < 50);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [UnitsInStock] < 50";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestComplexWhere_AndOperator()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act - This is the example from the Complex Task description
        var query = productSet.Where(p => p.UnitPrice > 100 && p.ProductType == "Customised Product");
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE ([UnitPrice] > 100 AND [ProductType] = 'Customised Product')";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestComplexWhere_MultipleAndOperators()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => 
            p.UnitPrice > 100 && 
            p.ProductType == "Customised Product" && 
            p.UnitsInStock < 50);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE (([UnitPrice] > 100 AND [ProductType] = 'Customised Product') AND [UnitsInStock] < 50)";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestWhere_ReverseOperandOrder()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act - Constant on the left side
        var query = productSet.Where(p => "Customised Product" == p.ProductType);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [ProductType] = 'Customised Product'";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestWhere_GreaterThanOrEqual()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => p.UnitPrice >= 100);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [UnitPrice] >= 100";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestWhere_LessThanOrEqual()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => p.UnitsInStock <= 20);
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE [UnitsInStock] <= 20";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }

    [Fact]
    public void TestTranslator_DirectUsage()
    {
        // Arrange
        var translator = new ExpressionToSqlTranslator();
        System.Linq.Expressions.Expression<Func<ProductEntity, bool>> expression = 
            p => p.UnitPrice > 100 && p.ProductType == "Customised Product";

        // Act
        string sql = translator.Translate(expression, typeof(ProductEntity));

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE ([UnitPrice] > 100 AND [ProductType] = 'Customised Product')";
        _output.WriteLine($"Generated SQL: {sql}");
        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void TestWithMockData_ReturnsResults()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var mockProducts = new[]
        {
            new ProductEntity 
            { 
                ProductId = 1, 
                ProductName = "Custom Widget", 
                UnitPrice = 150, 
                ProductType = "Customised Product",
                UnitsInStock = 30
            },
            new ProductEntity 
            { 
                ProductId = 2, 
                ProductName = "Premium Gadget", 
                UnitPrice = 200, 
                ProductType = "Customised Product",
                UnitsInStock = 15
            }
        };
        executor.SetMockData(mockProducts);

        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var results = productSet
            .Where(p => p.UnitPrice > 100 && p.ProductType == "Customised Product")
            .ToList();

        // Assert
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        _output.WriteLine($"Results count: {results.Count}");
        Assert.Equal(2, results.Count);
        Assert.All(results, p => 
        {
            Assert.True(p.UnitPrice > 100);
            Assert.Equal("Customised Product", p.ProductType);
        });
    }

    [Fact]
    public void TestComplexQuery_WithDecimalAndString()
    {
        // Arrange
        var executor = new MockSqlQueryExecutor();
        var productSet = new CustomEntitySet<ProductEntity>(executor);

        // Act
        var query = productSet.Where(p => 
            p.UnitPrice > 99.99m && 
            p.ProductName == "Widget");
        var _ = query.ToList();

        // Assert
        string expectedSql = "SELECT * FROM [dbo].[Products] WHERE ([UnitPrice] > 99.99 AND [ProductName] = 'Widget')";
        _output.WriteLine($"Generated SQL: {executor.LastGeneratedSql}");
        Assert.Equal(expectedSql, executor.LastGeneratedSql);
    }
}
