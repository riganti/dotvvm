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
                return expressionFactory.UpdateMember(left, TypeConversion.EnsureImplicitConversion(right, left.Type, true))
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

            // lift the other side to null
            if (left.Type.IsNullable() && right.Type.IsValueType && !right.Type.IsNullable())
            {
                right = Expression.Convert(right, right.Type.MakeNullableType());
            }
            else if (right.Type.IsNullable() && left.Type.IsValueType && !left.Type.IsNullable())
            {
                left = Expression.Convert(left, left.Type.MakeNullableType());
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

            if (operation is ExpressionType.Equal or ExpressionType.NotEqual && !leftType.IsValueType && !rightType.IsValueType)
            {
                // https://github.com/dotnet/csharpstandard/blob/standard-v6/standard/expressions.md#11117-reference-type-equality-operators
                // Every class type C implicitly provides the following predefined reference type equality operators:
                // bool operator ==(C x, C y);
                // bool operator !=(C x, C y);
                return ReferenceEquality(left, right, operation == ExpressionType.NotEqual);
            }

            if (operation is ExpressionType.LeftShift or ExpressionType.RightShift)
            {
                // https://github.com/dotnet/csharpstandard/blob/standard-v6/standard/expressions.md#1110-shift-operators
                // * shift operators always take int32 as the second argument
                var rightConverted = ConvertToMaybeNullable(right, typeof(int), true)!;
                // * the first argument is int, uint, long, ulong (in this order)
                var leftConverted = ConvertToMaybeNullable(left, typeof(int), false) ?? ConvertToMaybeNullable(left, typeof(uint), false) ?? ConvertToMaybeNullable(left, typeof(long), false) ?? ConvertToMaybeNullable(left, typeof(ulong), false)!;
                if (leftConverted is null)
                    throw new InvalidOperationException($"Cannot apply {operation} operator to type {leftType.ToCode()}. The type must be convertible to an integer or have a custom operator defined.");
                return operation == ExpressionType.LeftShift ? Expression.LeftShift(leftConverted, rightConverted) : Expression.RightShift(leftConverted, rightConverted);
            }

            // List of types in order of precendence
            var enumType = leftType.IsEnum ? leftType : rightType.IsEnum ? rightType : null;
            // all operators have defined "overloads" for two
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

            throw new InvalidOperationException($"Cannot apply {operation} operator to types {left.Type.Name} and {right.Type.Name}.");
        }

        static Expression ReferenceEquality(Expression left, Expression right, bool not)
        {
            // * It is a binding-time error to use the predefined reference type equality operators to compare two references that are known to be different at binding-time. For example, if the binding-time types of the operands are two class types, and if neither derives from the other, then it would be impossible for the two operands to reference the same object. Thus, the operation is considered a binding-time error.
            var leftT = left.Type;
            var rightT = right.Type;
            if (leftT != rightT && !(leftT.IsAssignableFrom(rightT) || rightT.IsAssignableFrom(leftT)))
            {
                if (!leftT.IsInterface && rightT.IsInterface)
                    throw new InvalidOperationException($"Cannot compare types {leftT.ToCode()} and {rightT.ToCode()}, because the classes are unrelated.");
                if (leftT.IsSealed || rightT.IsSealed)
                    throw new InvalidOperationException($"Cannot compare types {leftT.ToCode()} and {rightT.ToCode()}, because {(leftT.IsSealed ? leftT : rightT).ToCode(stripNamespace: true)} is sealed and does not implement {rightT.ToCode(stripNamespace: true)}.");
            }
            return not ? Expression.ReferenceNotEqual(left, right) : Expression.ReferenceEqual(left, right);
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
