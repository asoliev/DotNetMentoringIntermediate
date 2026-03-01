using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer;

public class IncDecExpressionVisitor : ExpressionVisitor
{
    private readonly Dictionary<string, object>? _parameterReplacements;

    public IncDecExpressionVisitor()
    {
    }

    public IncDecExpressionVisitor(Dictionary<string, object> parameterReplacements)
    {
        _parameterReplacements = parameterReplacements;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Transform x + 1 to Increment(x) and x - 1 to Decrement(x)
        if (node.NodeType == ExpressionType.Add || node.NodeType == ExpressionType.Subtract)
        {
            // Check if right operand is a constant with value 1
            if (node.Right is ConstantExpression rightConstant && 
                rightConstant.Value is int intValue && 
                intValue == 1)
            {
                var left = Visit(node.Left);
                
                if (node.NodeType == ExpressionType.Add)
                {
                    return Expression.Increment(left);
                }
                else // Subtract
                {
                    return Expression.Decrement(left);
                }
            }
            
            // Check if left operand is a constant with value 1 (for 1 + x case)
            if (node.Left is ConstantExpression leftConstant && 
                leftConstant.Value is int leftIntValue && 
                leftIntValue == 1 &&
                node.NodeType == ExpressionType.Add)
            {
                var right = Visit(node.Right);
                return Expression.Increment(right);
            }
        }

        return base.VisitBinary(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        // Visit the body of the lambda, where parameter replacements will occur
        var body = Visit(node.Body);

        // Determine which parameters should remain after replacement
        var newParameters = node.Parameters
            .Where(p => _parameterReplacements == null || !_parameterReplacements.ContainsKey(p.Name!))
            .ToList();

        // If the body or parameters changed, create a new lambda
        if (body != node.Body || newParameters.Count != node.Parameters.Count)
        {
            return Expression.Lambda(body, newParameters);
        }

        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        // Replace parameters with constants if replacement dictionary is provided
        if (_parameterReplacements != null && _parameterReplacements.TryGetValue(node.Name!, out var value))
        {
            return Expression.Constant(value, node.Type);
        }

        return base.VisitParameter(node);
    }
}
