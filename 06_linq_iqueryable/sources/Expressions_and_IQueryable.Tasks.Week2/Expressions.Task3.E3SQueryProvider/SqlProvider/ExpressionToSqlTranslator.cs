using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider.SqlProvider;

/// <summary>
/// Translates LINQ expressions to SQL queries
/// Supports: SELECT, FROM, WHERE with =, >, <, AND operators
/// </summary>
public class ExpressionToSqlTranslator : ExpressionVisitor
{
    private StringBuilder _whereClause;
    private StringBuilder _selectClause;
    private string _tableName;
    private bool _isLeftSide = true;
    private readonly Stack<string> _operands = new();

    public string Translate(Expression expression, Type elementType)
    {
        _whereClause = new StringBuilder();
        _selectClause = new StringBuilder();
        _tableName = GetTableName(elementType);

        Visit(expression);

        // Build the complete SQL query
        var sql = new StringBuilder();
        sql.Append("SELECT ");
        sql.Append(_selectClause.Length > 0 ? _selectClause.ToString() : "*");
        sql.Append(" FROM ");
        sql.Append(_tableName);

        if (_whereClause.Length > 0)
        {
            sql.Append(" WHERE ");
            sql.Append(_whereClause);
        }

        return sql.ToString();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(System.Linq.Queryable))
            switch (node.Method.Name)
            {
                case "Where":
                    Visit(node.Arguments[0]); // Visit source
                    var predicate = node.Arguments[1];
                    Visit(predicate); // Visit the lambda
                    return node;

                case "Select":
                    Visit(node.Arguments[0]); // Visit source
                    // For now, we'll use SELECT * (selector handling could be added)
                    return node;
            }

        return base.VisitMethodCall(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        Visit(node.Body);
        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Equal:
                VisitComparison(node, "=");
                break;

            case ExpressionType.GreaterThan:
                VisitComparison(node, ">");
                break;

            case ExpressionType.LessThan:
                VisitComparison(node, "<");
                break;

            case ExpressionType.GreaterThanOrEqual:
                VisitComparison(node, ">=");
                break;

            case ExpressionType.LessThanOrEqual:
                VisitComparison(node, "<=");
                break;

            case ExpressionType.NotEqual:
                VisitComparison(node, "<>");
                break;

            case ExpressionType.AndAlso:
                _whereClause.Append('(');
                Visit(node.Left);
                _whereClause.Append(" AND ");
                Visit(node.Right);
                _whereClause.Append(')');
                break;

            case ExpressionType.OrElse:
                _whereClause.Append('(');
                Visit(node.Left);
                _whereClause.Append(" OR ");
                Visit(node.Right);
                _whereClause.Append(')');
                break;

            default:
                throw new NotSupportedException($"Binary operator '{node.NodeType}' is not supported");
        }

        return node;
    }

    private void VisitComparison(BinaryExpression node, string op)
    {
        _isLeftSide = true;
        Visit(node.Left);
        string left = _operands.Pop();

        _isLeftSide = false;
        Visit(node.Right);
        string right = _operands.Pop();

        // Ensure column is on the left side
        if (IsColumnReference(right) && !IsColumnReference(left))
            _whereClause.Append($"{right} {op} {left}");
        else
            _whereClause.Append($"{left} {op} {right}");
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            // This is a column reference
            _operands.Push($"[{node.Member.Name}]");
        }
        else
        {
            // This might be a member access on a constant (e.g., a captured variable)
            var value = GetValue(node);
            _operands.Push(FormatValue(value));
        }

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value != null && !IsQueryableConstant(node))
            _operands.Push(FormatValue(node.Value));
        return node;
    }

    private static bool IsQueryableConstant(ConstantExpression node) => node.Type.IsGenericType &&
               (node.Type.GetGenericTypeDefinition() == typeof(System.Linq.EnumerableQuery<>) ||
                node.Type.GetGenericTypeDefinition() == typeof(SqlQuery<>));

    private static bool IsColumnReference(string value) => value.StartsWith('[') && value.EndsWith(']');

    private static string FormatValue(object value)
    {
        if (value == null)
            return "NULL";

        return value switch
        {
            string s => $"'{s.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            _ => value.ToString()
        };
    }

    private static object GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }

    private static string GetTableName(Type type)
    {
        // Check for custom table attribute or use type name
        var tableAttr = type.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), false);
        if (tableAttr.Length > 0)
        {
            var attr = (System.ComponentModel.DataAnnotations.Schema.TableAttribute)tableAttr[0];
            return $"[{attr.Schema}].[{attr.Name}]";
        }

        // Default: use class name + "s" as table name
        return $"[dbo].[{type.Name}]";
    }
}