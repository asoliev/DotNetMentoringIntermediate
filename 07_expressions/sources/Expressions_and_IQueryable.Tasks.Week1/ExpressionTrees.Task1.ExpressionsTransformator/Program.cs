/*
 * Create a class based on ExpressionVisitor, which makes expression tree transformation:
 * 1. converts expressions like <variable> + 1 to increment operations, <variable> - 1 - into decrement operations.
 * 2. changes parameter values in a lambda expression to constants, taking the following as transformation parameters:
 *    - source expression;
 *    - dictionary: <parameter name: value for replacement>
 * The results could be printed in console or checked via Debugger using any Visualizer.
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Expression Visitor for increment/decrement.");
        Console.WriteLine();

        // Example 1: Transform x + 1 to Increment(x)
        Console.WriteLine("=== Example 1: x + 1 => Increment(x) ===");
        Expression<Func<int, int>> addExpr = x => x + 1;
        Console.WriteLine($"Original: {addExpr}");
        
        var visitor1 = new IncDecExpressionVisitor();
        var transformed1 = visitor1.Visit(addExpr);
        Console.WriteLine($"Transformed: {transformed1}");
        Console.WriteLine();

        // Example 2: Transform x - 1 to Decrement(x)
        Console.WriteLine("=== Example 2: x - 1 => Decrement(x) ===");
        Expression<Func<int, int>> subtractExpr = x => x - 1;
        Console.WriteLine($"Original: {subtractExpr}");
        
        var transformed2 = visitor1.Visit(subtractExpr);
        Console.WriteLine($"Transformed: {transformed2}");
        Console.WriteLine();

        // Example 3: Replace parameters with constants
        Console.WriteLine("=== Example 3: Replace parameters with constants ===");
        Expression<Func<int, int, int>> paramExpr = (a, b) => a + b + 1;
        Console.WriteLine($"Original: {paramExpr}");
        
        var replacements = new Dictionary<string, object>
        {
            { "a", 5 },
            { "b", 10 }
        };
        
        var visitor2 = new IncDecExpressionVisitor(replacements);
        var transformed3 = visitor2.Visit(paramExpr);
        Console.WriteLine($"Transformed: {transformed3}");
        Console.WriteLine();

        // Example 4: Combined transformation
        Console.WriteLine("=== Example 4: Combined transformation ===");
        Expression<Func<int, int>> combinedExpr = x => (x + 1) * 2 - 1;
        Console.WriteLine($"Original: {combinedExpr}");
        
        var replacements2 = new Dictionary<string, object> { { "x", 7 } };
        var visitor3 = new IncDecExpressionVisitor(replacements2);
        var transformed4 = visitor3.Visit(combinedExpr);
        Console.WriteLine($"Transformed: {transformed4}");
        Console.WriteLine();

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
