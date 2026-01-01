using System;
using Expressions.Task3.E3SQueryProvider.Demo;
using Xunit;
using Xunit.Abstractions;

namespace Expressions.Task3.E3SQueryProvider.Test.Demo;

/// <summary>
/// Test that runs the SQL LINQ Provider demo
/// This shows the activated SqlServerQueryExecutor in action
/// </summary>
public class DemoTests
{
    private readonly ITestOutputHelper _output;

    public DemoTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void RunSqlLinqProviderDemo()
    {
        // Redirect console output to test output
        var oldOut = Console.Out;
        try
        {
            using var writer = new System.IO.StringWriter();
            Console.SetOut(writer);

            // Run the demo
            SqlLinqProviderDemo.Main(Array.Empty<string>());

            // Get the output
            string output = writer.ToString();
            
            // Display in test results
            _output.WriteLine(output);

            // Verify key outputs are present
            Assert.Contains("SQL LINQ Provider - Live Demo", output);
            Assert.Contains("BASIC QUERIES", output);
            Assert.Contains("COMPLEX QUERIES WITH AND", output);
            Assert.Contains("Demo Complete! ?", output);
        }
        finally
        {
            Console.SetOut(oldOut);
        }
    }
}
