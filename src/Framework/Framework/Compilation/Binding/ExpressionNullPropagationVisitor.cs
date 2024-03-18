﻿using System;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
                {
                    return Expression.MakeBinary(node.NodeType, left, right, false, node.Method, node.Conversion);
                }, checkReferenceTypes: false);
            }

            if (node.NodeType.ToString().EndsWith("Assign"))
            {
                var right = Visit(node.Right);
                // Convert non-nullable to nullable
                // and unwrap nullable to non-nullable - throw a runtime NullReferenceException when right is null
                if (right.Type != node.Right.Type && right.Type.UnwrapNullableType() == right.Type.UnwrapNullableType())
                {
                    right = Expression.Convert(right, node.Right.Type);
                }

                // only check for left target's null, assignment to null is perfectly valid
                if (node.Left is MemberExpression memberExpression)
                {
                    return CheckForNull(Visit(memberExpression.Expression), memberTarget =>
                        node.Update(memberExpression.Update(memberTarget), node.Conversion, right));
                }
                else if (node.Left is IndexExpression indexer)
                {
                    return CheckForNull(Visit(indexer.Object), memberTarget =>
                        node.Update(indexer.Update(memberTarget, indexer.Arguments), node.Conversion, right));
                }
                // this should only be ParameterExpression
                else return node.Update(node.Left, node.Conversion, right);
            }
            else if (node.NodeType == ExpressionType.ArrayIndex)
            {
                return CheckForNull(base.Visit(node.Left), left2 => createExpr(left2));
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
            if (node.NodeType == ExpressionType.Convert && node.Method == null)
            {
                // just make sure it converts to nullable type
                if (node.Type.IsValueType && !node.Type.IsNullable())
                    return Expression.Convert(Visit(node.Operand), typeof(Nullable<>).MakeGenericType(node.Type));
                else
                    return Expression.Convert(Visit(node.Operand), node.Type);
            }

            return CheckForNull(Visit(node.Operand), operand =>
                Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method));
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
                    ifTrue = TypeConversion.EnsureImplicitConversion(ifTrue, nullable);
                    ifFalse = TypeConversion.EnsureImplicitConversion(ifFalse, nullable);
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
            if (node.Method.Attributes.HasFlag(MethodAttributes.SpecialName))
            {
                var expr = TryVisitMethodCallWithSpecialName(node);
                if (expr != null)
                    return expr;
            }

            // If the method is an extension method, we need to check the first argument for null.
            var nullPropagateMethod = node.Method.IsDefined(typeof(ExtensionAttribute)) || node.Method.DeclaringType == typeof(BoxingUtils);

            if (nullPropagateMethod && node.Object == null && node.Arguments.Any())
                return CheckForNull(Visit(node.Arguments.First()), target =>
                    Expression.Call(node.Method, UnwrapNullableTypes(node.Arguments.Skip(1)).Prepend(target)),
                    suppress: node.Arguments.First().Type.IsNullable()
                );

            return CheckForNull(Visit(node.Object), target =>
                Expression.Call(target, node.Method, UnwrapNullableTypes(node.Arguments)),
                suppress: node.Object?.Type?.IsNullable() ?? true
            );
        }

        private Expression? TryVisitMethodCallWithSpecialName(MethodCallExpression node)
        {
            // Check if we are translating an access to indexer property
            if (node.Method.Name.StartsWith("get_", StringComparison.Ordinal) || node.Method.Name.StartsWith("set_", StringComparison.Ordinal))
            {
                var targetType = node.Object?.Type;
                if (targetType == null)
                    return null;

                var indexer = targetType.GetProperties().SingleOrDefault(p => p.GetIndexParameters().Length > 0);
                if (indexer == null)
                    return null;

                if (!node.Method.Name.Equals($"get_{indexer.Name}", StringComparison.Ordinal) && !node.Method.Name.Equals($"set_{indexer.Name}", StringComparison.Ordinal))
                    return null;

                return CheckForNull(Visit(node.Object), target =>
                {
                    return CheckForNull(Visit(node.Arguments.First()), index =>
                    {
                        var convertedIndex = TypeConversion.EnsureImplicitConversion(index, node.Method.GetParameters().First().ParameterType);
                        return Expression.Call(target, node.Method, new[] { convertedIndex }.Concat(node.Arguments.Skip(1)));
                    });
                }, suppress: node.Object?.Type?.IsNullable() ?? true);
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return node.Update(UnwrapNullableTypes(node.Arguments));
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
                return Expression.Call(
                    ThrowHelpers.UnwrapNullableMethod.MakeGenericMethod(expectedType),
                    expression,
                    Expression.Constant(formattedExpression, typeof(string)),
                    Expression.Constant(expectedType.FullName, typeof(string))
                );
            }
            else
                throw new Exception($"Type mismatch: {expectedType} was expected, got {expression.Type}");
        }

        bool IsNonNull(Expression e)
        {
            if (e.Type.IsValueType && !e.Type.IsNullable())
                return true;
            return e switch {
                ConstantExpression { Value: not null } => true,
                ParameterExpression { Name: not null } p when p.Name == "currentControl" || p.Name == "vm" || p.Name.StartsWith("vm_") => true,
                ConditionalExpression c => IsNonNull(c.IfTrue) && IsNonNull(c.IfFalse),
                BlockExpression b => IsNonNull(b.Expressions.Last()),
                BinaryExpression { NodeType: ExpressionType.Coalesce } b => IsNonNull(b.Right),
                _ => false
            };
        }


        private int tmpCounter;
        protected Expression CheckForNull(Expression? parameter, Func<Expression, Expression> callback, bool checkReferenceTypes = true, bool suppress = false)
        {
            if (suppress || parameter is null || IsNonNull(parameter) || !checkReferenceTypes && !parameter.Type.IsValueType)
                return callback(parameter!);
            var p2 =
                parameter as ParameterExpression ??
                Expression.Parameter(parameter.Type, "tmp" + tmpCounter++);
            var eresult = callback(p2.Type.IsNullable() ? (Expression)Expression.Property(p2, "Value") : p2);
            eresult = TypeConversion.EnsureImplicitConversion(eresult, eresult.Type.MakeNullableType());
            var condition = parameter.Type.IsNullable() ? (Expression)Expression.Property(p2, "HasValue") : Expression.NotEqual(p2, Expression.Constant(null, p2.Type));
            var handledResult =
                Expression.Condition(condition,
                    eresult,
                    Expression.Default(eresult.Type));


            if (parameter is BlockExpression block)
            {
                // squash blocks together to reduce load on the expression compiler (and also simplify debugging of the expressions)
                return Expression.Block(
                    block.Variables.Concat(new [] { p2 }),
                    block.Expressions.Take(block.Expressions.Count - 1).Concat(new Expression[] {
                        Expression.Assign(p2, block.Expressions[block.Expressions.Count - 1]),
                        handledResult
                    })
                );
            }
            else if (parameter == p2)
            {
                return handledResult;
            }
            else
            {
                return Expression.Block(
                    new[] { p2 },
                    Expression.Assign(p2, parameter),
                    handledResult
                );
            }
        }

        public static Expression PropagateNulls(Expression expr, Func<Expression, bool> canBeNull)
        {
            var v = new ExpressionNullPropagationVisitor(canBeNull);
            return v.Visit(expr);
        }


        internal class ThrowHelpers
        {
            public static readonly MethodInfo ThrowNullReferenceMethod = typeof(ThrowHelpers).GetMethod(nameof(ThrowNullReference))!;
            public static T ThrowNullReference<T>(string expression, string type) =>
                throw new NullReferenceException($"Binding expression '{expression}' of type '{type}' has evaluated to null.");

            public static readonly MethodInfo UnwrapNullableMethod = typeof(ThrowHelpers).GetMethod(nameof(UnwrapNullable))!;
            public static T UnwrapNullable<T>(T? value, string expression, string type) where T : struct =>
                value ?? ThrowNullReference<T>(expression, type);
        }
    }
}
