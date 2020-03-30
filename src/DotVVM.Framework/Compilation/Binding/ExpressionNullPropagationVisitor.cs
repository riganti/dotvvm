using System;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotVVM.Framework.Compilation.Binding
{
    public class ExpressionNullPropagationVisitor : ExpressionVisitor
    {
        public Func<Expression, bool> CanBeNull { get; }

        public ExpressionNullPropagationVisitor(Func<Expression, bool> canBeNull)
        {
            CanBeNull = canBeNull;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression?.Type?.IsNullable() == true)
            {
                if (node.Member.Name == "Value") return Visit(node.Expression);
                else return base.VisitMember(node);
            }
            else return CheckForNull(Visit(node.Expression), expr =>
                Expression.MakeMemberAccess(expr, node.Member));
        }

        protected override Expression VisitLambda<T>(Expression<T> expression)
        {
            // assert non-null for lambda expressions returning value types
            var body = Visit(expression.Body);
            if (body.Type != expression.ReturnType)
            {
                Debug.Assert(Nullable.GetUnderlyingType(body.Type) == expression.ReturnType);
                body = Expression.Property(body, "Value");
            }
            return expression.Update(body, expression.Parameters);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {

            Expression createExpr(Expression left)
            {
                return CheckForNull(Visit(node.Right), right =>
                                Expression.MakeBinary(node.NodeType, left, right, false, node.Method, node.Conversion),
                            checkReferenceTypes: false);
            }

            if (node.NodeType.ToString().EndsWith("Assign"))
            {
                // only check for left target's null, assignment to null is perfectly valid
                if (node.Left is MemberExpression memberExpression)
                {
                    return CheckForNull(Visit(memberExpression.Expression), memberTarget =>
                        createExpr(memberExpression.Update(memberTarget)));
                }
                else if (node.Left is IndexExpression indexer)
                {
                    return CheckForNull(Visit(indexer.Object), memberTarget =>
                        createExpr(indexer.Update(memberTarget, indexer.Arguments)));
                }
                // this should only be ParameterExpression
                else return createExpr(node.Left);
            }
            else
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);
                var nullable = left.Type.IsNullable() ? left.Type : right.Type;
                left = TypeConversion.ImplicitConversion(left, nullable);
                right = TypeConversion.ImplicitConversion(right, nullable);

                if (right != null && left != null)
                    return Expression.MakeBinary(node.NodeType, left, right, left.Type.IsNullable() && node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual, node.Method);
                else return CheckForNull(base.Visit(node.Left), left2 =>
                    createExpr(left2),
                checkReferenceTypes: false);
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return CheckForNull(Visit(node.Operand), operand =>
                Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method),
                checkReferenceTypes: node.Method == null);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return CheckForNull(Visit(node.Expression), target =>
                    Expression.Invoke(target, UnwrapNullableTypes(node.Arguments)));
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return CheckForNull(Visit(node.Test), test => {
                var ifTrue = Visit(node.IfTrue);
                var ifFalse = Visit(node.IfFalse);
                if (ifTrue.Type != ifFalse.Type)
                {
                    var nullable = ifTrue.Type.IsNullable() ? ifTrue.Type : ifFalse.Type;
                    ifTrue = TypeConversion.ImplicitConversion(ifTrue, nullable);
                    ifFalse = TypeConversion.ImplicitConversion(ifFalse, nullable);
                }
                return Expression.Condition(test, ifTrue, ifFalse);
            });
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            return CheckForNull(Visit(node.Object), target =>
                Expression.MakeIndex(target, node.Indexer, UnwrapNullableTypes(node.Arguments))
            );
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return CheckForNull(Visit(node.Object), target =>
                Expression.Call(target, node.Method, UnwrapNullableTypes(node.Arguments)),
                suppress: node.Object?.Type?.IsNullable() ?? true
            );
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return Expression.New(node.Constructor, UnwrapNullableTypes(node.Arguments));
        }

        protected Expression[] UnwrapNullableTypes(IEnumerable<Expression> uncheckedArguments) =>
            uncheckedArguments.Select(UnwrapNullableType).ToArray();
        protected Expression UnwrapNullableType(Expression expression) =>
            UnwrapNullableType(Visit(expression), expression.Type, expression.ToString());
        protected Expression UnwrapNullableType(Expression expression, Type expectedType, string formattedExpression)
        {
            if (expression.Type == expectedType)
                return expression;
            else if (expression.Type == typeof(Nullable<>).MakeGenericType(expectedType))
            {
                var tmp = Expression.Parameter(expression.Type);
                var nreCtor = typeof(NullReferenceException).GetConstructor(new [] { typeof(string) });
                return Expression.Block(new [] { tmp },
                    Expression.Assign(tmp, expression),
                    Expression.Condition(
                        Expression.Property(tmp, "HasValue"),
                        Expression.Property(tmp, "Value"),
                        Expression.Throw(Expression.New(nreCtor, Expression.Constant($"Binding expression '{formattedExpression}' of type '{expectedType}' has evaluated to null.")), expectedType)
                    )
                );
            }
            else
                throw new Exception($"Type mismatch: {expectedType} was expected, got {expression.Type}");
        }

        private int tmpCounter;
        protected Expression CheckForNull(Expression parameter, Func<Expression, Expression> callback, bool checkReferenceTypes = true, bool suppress = false)
        {
            if (suppress || parameter == null || (parameter.Type.GetTypeInfo().IsValueType && !parameter.Type.IsNullable()) || !checkReferenceTypes && !parameter.Type.GetTypeInfo().IsValueType)
                return callback(parameter);
            var p2 = Expression.Parameter(parameter.Type, "tmp" + tmpCounter++);
            var eresult = callback(p2.Type.IsNullable() ? (Expression)Expression.Property(p2, "Value") : p2);
            eresult = TypeConversion.ImplicitConversion(eresult, eresult.Type.MakeNullableType());
            return Expression.Block(
                new[] { p2 },
                Expression.Assign(p2, parameter),
                Expression.Condition(parameter.Type.IsNullable() ? (Expression)Expression.Property(p2, "HasValue") : Expression.NotEqual(p2, Expression.Constant(null, p2.Type)),
                    eresult,
                    Expression.Default(eresult.Type)));
        }

        public static Expression PropagateNulls(Expression expr, Func<Expression, bool> canBeNull)
        {
            var v = new ExpressionNullPropagationVisitor(canBeNull);
            return v.Visit(expr);
        }
    }
}
