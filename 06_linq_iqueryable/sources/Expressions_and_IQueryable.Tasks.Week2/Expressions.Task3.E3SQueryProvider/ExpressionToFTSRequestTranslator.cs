using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Expressions.Task3.E3SQueryProvider;

public class ExpressionToFtsRequestTranslator : ExpressionVisitor
{
    private readonly List<string> _statements;
    private StringBuilder _currentStatementBuilder;

    public ExpressionToFtsRequestTranslator()
    {
        _statements = [];
        _currentStatementBuilder = new StringBuilder();
    }

    public string Translate(Expression exp)
    {
        Visit(exp);
        // If we have multiple statements (from AND operator), join them
        if (_statements.Count > 0)
            return string.Join(" AND ", _statements);
        return _currentStatementBuilder.ToString();
    }

    /// <summary>
    /// Translates expression to a list of individual query statements
    /// Used for AND operator support with multiple FTS statements
    /// </summary>
    public List<string> TranslateToStatements(Expression exp)
    {
        Visit(exp);
        if (_statements.Count > 0)
            return _statements;
        if (_currentStatementBuilder.Length > 0)
            return [_currentStatementBuilder.ToString()];
        return [];
    }

    #region protected methods

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(Queryable)
            && node.Method.Name == "Where")
        {
            var predicate = node.Arguments[1];
            Visit(predicate);

            return node;
        }

        // TASK 2: Handle string methods
        if (node.Method.DeclaringType == typeof(string))
            switch (node.Method.Name)
            {
                case "StartsWith":
                    // field.StartsWith("value") => field:(value*)
                    Visit(node.Object);
                    _currentStatementBuilder.Append('(');
                    Visit(node.Arguments[0]);
                    _currentStatementBuilder.Append("*)");
                    return node;

                case "EndsWith":
                    // field.EndsWith("value") => field:(*value)
                    Visit(node.Object);
                    _currentStatementBuilder.Append("(*");
                    Visit(node.Arguments[0]);
                    _currentStatementBuilder.Append(')');
                    return node;

                case "Contains":
                    // field.Contains("value") => field:(*value*)
                    Visit(node.Object);
                    _currentStatementBuilder.Append("(*");
                    Visit(node.Arguments[0]);
                    _currentStatementBuilder.Append("*)");
                    return node;

                case "Equals":
                    // field.Equals("value") => field:(value)
                    Visit(node.Object);
                    _currentStatementBuilder.Append('(');
                    Visit(node.Arguments[0]);
                    _currentStatementBuilder.Append(')');
                    return node;
            }

        return base.VisitMethodCall(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Equal:
                // TASK 1: Support both operand orders
                Expression memberExpression = null;
                Expression constantExpression = null;

                // Check if left is member and right is constant
                if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                {
                    memberExpression = node.Left;
                    constantExpression = node.Right;
                }
                // Check if right is member and left is constant (reverse order)
                else if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpression = node.Right;
                    constantExpression = node.Left;
                }
                else
                    throw new NotSupportedException($"Operands should be property/field and constant");

                Visit(memberExpression);
                _currentStatementBuilder.Append('(');
                Visit(constantExpression);
                _currentStatementBuilder.Append(')');
                break;

            case ExpressionType.AndAlso:
                // TASK 3: Handle && (AND) operator
                // Extract each side of the AND as separate statements
                var leftStatement = ExtractStatement(node.Left);
                var rightStatement = ExtractStatement(node.Right);

                _statements.Add(leftStatement);
                _statements.Add(rightStatement);
                break;

            default:
                throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
        }

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        // Only append field name when it's a parameter member access (not nested)
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            _currentStatementBuilder.Append(node.Member.Name).Append(':');
        return base.VisitMember(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        // Only append value if it's not a queryable constant
        if (node.Value != null && !IsQueryableConstant(node))
            _currentStatementBuilder.Append(node.Value);
        return node;
    }

    #endregion

    #region private methods

    private static bool IsQueryableConstant(ConstantExpression node) => node.Type.IsGenericType &&
               node.Type.GetGenericTypeDefinition() == typeof(System.Linq.EnumerableQuery<>);

    /// <summary>
    /// Extracts a single statement from an expression by visiting it with a temporary builder
    /// </summary>
    private string ExtractStatement(Expression expression)
    {
        var tempBuilder = _currentStatementBuilder;
        _currentStatementBuilder = new StringBuilder();

        Visit(expression);

        var result = _currentStatementBuilder.ToString();
        _currentStatementBuilder = tempBuilder;

        return result;
    }

    #endregion
}