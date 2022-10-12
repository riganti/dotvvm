using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation.Binding
{
    static class OperatorResolution
    {
        public static Expression GetBinaryOperator(
            this MemberExpressionFactory expressionFactory,
            Expression left,
            Expression right,
            ExpressionType operation)
        {
            if (operation == ExpressionType.Coalesce)
            {
                // in bindings, most expressions will be nullable due to automatic null-propagation
                // the null propagation visitor however runs after this, so we need to convert left to nullable
                // to make the validation in Expression.Coalesce happy
                var leftNullable =
                    left.Type.IsValueType && !left.Type.IsNullable()
                        ? Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type))
                        : left;
                return Expression.Coalesce(leftNullable, right);
            }

            if (operation == ExpressionType.Assign)
            {
                return expressionFactory.UpdateMember(left, TypeConversion.EnsureImplicitConversion(right, left.Type, true)!)
                    .NotNull($"Expression '{right}' cannot be assigned into '{left}'.");
            }

            // lift to nullable types when one side is `null`
            if (left is ConstantExpression { Value: null } && right.Type.IsValueType)
            {
                left = Expression.Constant(null, right.Type.MakeNullableType());
                right = Expression.Convert(right, right.Type.MakeNullableType());
            }
            if (right is ConstantExpression { Value: null } && left.Type.IsValueType)
            {
                left = Expression.Convert(left, left.Type.MakeNullableType());
                right = Expression.Constant(null, left.Type.MakeNullableType());
            }

            var leftType = left.Type.UnwrapNullableType();
            var rightType = right.Type.UnwrapNullableType();

            // we only support booleans
            if (operation == ExpressionType.AndAlso)
                return Expression.AndAlso(TypeConversion.EnsureImplicitConversion(left, typeof(bool)), TypeConversion.EnsureImplicitConversion(right, typeof(bool)));
            else if (operation == ExpressionType.OrElse)
                return Expression.OrElse(TypeConversion.EnsureImplicitConversion(left, typeof(bool)), TypeConversion.EnsureImplicitConversion(right, typeof(bool)));

            // skip the slow overload resolution if possible
            if (leftType == rightType && !leftType.IsEnum && leftType.IsPrimitive)
                return Expression.MakeBinary(operation, left, right);
            if (operation == ExpressionType.Add && leftType == typeof(string) && rightType == typeof(string))
            {
                return Expression.Add(left, right, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
            }

            var customOperator = operation switch {
                ExpressionType.Add => "op_Addition",
                ExpressionType.Subtract => "op_Subtraction",
                ExpressionType.Multiply => "op_Multiply",
                ExpressionType.Divide => "op_Division",
                ExpressionType.Modulo => "op_Modulus",
                ExpressionType.LeftShift => "op_LeftShift",
                ExpressionType.RightShift => "op_RightShift",
                ExpressionType.And => "op_BitwiseAnd",
                ExpressionType.Or => "op_BitwiseOr",
                ExpressionType.ExclusiveOr => "op_ExclusiveOr",
                ExpressionType.Equal => "op_Equality",
                ExpressionType.NotEqual => "op_Inequality",
                ExpressionType.GreaterThan => "op_GreaterThan",
                ExpressionType.LessThan => "op_LessThan",
                ExpressionType.GreaterThanOrEqual => "op_GreaterThanOrEqual",
                ExpressionType.LessThanOrEqual => "op_LessThanOrEqual",
                _ => null
            };


            // Try to find user defined operator
            if (customOperator != null && (!leftType.IsPrimitive || !rightType.IsPrimitive))
            {
                var customOperatorExpr = expressionFactory.TryCallCustomBinaryOperator(left, right, customOperator, operation);
                if (customOperatorExpr is {})
                    return customOperatorExpr;
            }

            if (leftType.IsEnum && rightType.IsEnum && leftType != rightType)
            {
                throw new InvalidOperationException($"Cannot apply {operation} operator to two different enum types: {leftType.Name}, {rightType.Name}.");
            }

            // numeric operations
            if (operation == ExpressionType.LeftShift)
            {
                return Expression.LeftShift(left, ConvertToMaybeNullable(right, typeof(int), true));
            }
            else if (operation == ExpressionType.RightShift)
            {
                return Expression.RightShift(left, ConvertToMaybeNullable(right, typeof(int), true));
            }

            // List of types in order of precendence
            var enumType = leftType.IsEnum ? leftType : rightType.IsEnum ? rightType : null;
            var typeList = operation switch {
                ExpressionType.Or or ExpressionType.And or ExpressionType.ExclusiveOr =>
                    new[] { typeof(bool), enumType, typeof(int), typeof(uint), typeof(long), typeof(ulong) },
                _ =>
                    new[] { enumType, typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }
            };

            foreach (var commonType in typeList)
            {
                if (commonType == null) continue;

                var leftConverted = ConvertToMaybeNullable(left, commonType, throwExceptions: false);
                var rightConverted = ConvertToMaybeNullable(right, commonType, throwExceptions: false);

                if (leftConverted != null && rightConverted != null)
                {
                    return MakeBinary(operation, leftConverted, rightConverted);
                }
            }

            if (operation == ExpressionType.Add && (leftType == typeof(string) || rightType == typeof(string)))
            {
                return Expression.Add(
                    Expression.Convert(left, typeof(object)),
                    Expression.Convert(right, typeof(object)),
                    typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) })
                );
            }


            // if (left.Type.IsNullable() || right.Type.IsNullable())
            //     return GetBinaryOperator(expressionFactory, left.UnwrapNullable(), right.UnwrapNullable(), operation);

            // as a fallback, try finding overridden Equals method
            if (operation == ExpressionType.Equal) return EqualsMethod(expressionFactory, left, right);
            if (operation == ExpressionType.NotEqual) return Expression.Not(EqualsMethod(expressionFactory, left, right));

            throw new InvalidOperationException($"Cannot apply {operation} operator to types {left.Type.Name} and {right.Type.Name}.");
        }

        public static Expression EqualsMethod(
            MemberExpressionFactory expressionFactory,
            Expression left,
            Expression right
        )
        {
            Expression? equatable = null;
            Expression? theOther = null;
            if (typeof(IEquatable<>).IsAssignableFrom(left.Type))
            {
                equatable = left;
                theOther = right;
            }
            else if (typeof(IEquatable<>).IsAssignableFrom(right.Type))
            {
                equatable = right;
                theOther = left;
            }

            if (equatable != null)
            {
                var m = expressionFactory.CallMethod(equatable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { theOther! });
                if (m != null) return m;
            }

            if (left.Type.IsValueType)
            {
                equatable = left;
                theOther = right;
            }
            else if (left.Type.IsValueType)
            {
                equatable = right;
                theOther = left;
            }

            if (equatable != null)
            {
                theOther = TypeConversion.ImplicitConversion(theOther!, equatable.Type);
                if (theOther != null) return Expression.Equal(equatable, theOther);
            }

            return expressionFactory.CallMethod(left, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { right });
        }

        static Expression? ConvertToMaybeNullable(
            Expression expression,
            Type targetType,
            bool throwExceptions
        )
        {
            return TypeConversion.ImplicitConversion(expression, targetType) ??
                TypeConversion.ImplicitConversion(expression, targetType.MakeNullableType(), throwExceptions);
        }

        static Expression MakeBinary(ExpressionType type, Expression left, Expression right)
        {
            // Expression.MakeBinary doesn't handle enums, we need to convert it to the int and back
            // It works however, for Equals/NotEquals
            Type? enumType = null;

            if (type != ExpressionType.Equal && type != ExpressionType.NotEqual)
            {
                if (left.Type.UnwrapNullableType().IsEnum)
                {
                    enumType = left.Type.UnwrapNullableType();
                    left = ConvertToMaybeNullable(left, Enum.GetUnderlyingType(enumType), true)!;
                }
                if (right.Type.UnwrapNullableType().IsEnum)
                {
                    enumType = right.Type.UnwrapNullableType();
                    right = ConvertToMaybeNullable(right, Enum.GetUnderlyingType(enumType), true)!;
                }
            }

            var result = Expression.MakeBinary(type, left, right);
            if (enumType != null && result.Type != typeof(bool))
            {
                return Expression.Convert(result, enumType);
            }
            return result;
        }
    }
}
