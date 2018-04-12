using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Symbols
{
    public static class SymbolExtensions
    {
        public static bool HasAttribute<T>(this ISymbol symbol)
        {
            var attributeType = typeof(T);
            return symbol.GetAttributes().Any(a => a.AttributeClass.ToString().Equals(attributeType.FullName));
        }

        public static string GetTypescriptEquivalent(this ITypeSymbol symbol)
        {
            if (symbol.IsIntegerType() || symbol.IsFloatingNumberType())
                return "number";
            if (symbol.IsEquivalentTo(typeof(bool)))
                return "boolean";
            if (symbol.IsEquivalentTo(typeof(string)))
                return "string";
            if (symbol.IsArrayType() && symbol is INamedTypeSymbol namedType)
            {
                return namedType.TypeArguments.Select(t => t.GetTypescriptEquivalent()).StringJoin(",");
            }
            return symbol.Name;
        }

        public static bool IsArrayType(this ITypeSymbol symbol)
        {
            return symbol.Interfaces.Any(i => i.IsEquivalentTo(typeof(IEnumerable)) || i.IsEquivalentTo(typeof(IGridViewDataSet)));
        }

        public static bool IsFloatingNumberType(this ITypeSymbol symbol)
        {
            return symbol.IsEquivalentTo(typeof(float))
                   || symbol.IsEquivalentTo(typeof(double))
                   || symbol.IsEquivalentTo(typeof(decimal));
        }

        public static bool IsIntegerType(this ITypeSymbol symbol)
        {
            return symbol.IsEquivalentTo(typeof(short))
                || symbol.IsEquivalentTo(typeof(int))
                || symbol.IsEquivalentTo(typeof(long))
                || symbol.IsEquivalentTo(typeof(ushort))
                || symbol.IsEquivalentTo(typeof(uint))
                || symbol.IsEquivalentTo(typeof(ulong));
        }

        public static bool IsEquivalentTo(this ISymbol symbol, Type type)
        {
            var typeName = type.FullName;
            if (typeName.Contains("`"))
            {
                typeName = typeName.Remove(type.FullName.IndexOf('`'));
            }
            return symbol.GetFullName() == typeName;
        }

        public static string GetFullName(this ISymbol symbol)
        {
            return $"{symbol.ContainingNamespace.ToString()}.{symbol.Name}";
        }

        public static BinaryOperator ToTsBinaryOperator(this BinaryOperatorKind binaryOperator)
        {
            switch (binaryOperator)
            {
                case BinaryOperatorKind.Add:
                    return BinaryOperator.Add;
                case BinaryOperatorKind.Subtract:
                    return BinaryOperator.Subtract;
                case BinaryOperatorKind.Multiply:
                    return BinaryOperator.Multiply;
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.IntegerDivide:
                    return BinaryOperator.Divide;
                case BinaryOperatorKind.Remainder:
                    return BinaryOperator.Remainder;
                case BinaryOperatorKind.Power:
                    throw new NotSupportedException("Operator Power is not supported.");
                case BinaryOperatorKind.LeftShift:
                    throw new NotSupportedException("Operator left shift is not supported.");
                case BinaryOperatorKind.RightShift:
                    throw new NotSupportedException("Operator right shift is not supported.");
                case BinaryOperatorKind.And:
                    return BinaryOperator.And;
                case BinaryOperatorKind.Or:
                    return BinaryOperator.Or;
                case BinaryOperatorKind.ExclusiveOr:
                    return BinaryOperator.ExclusiveOr;
                case BinaryOperatorKind.ConditionalAnd:
                    return BinaryOperator.ConditionalAnd;
                case BinaryOperatorKind.ConditionalOr:
                    return BinaryOperator.ConditionalOr;
                case BinaryOperatorKind.Concatenate:
                    return BinaryOperator.Add;
                case BinaryOperatorKind.Equals:
                    return BinaryOperator.Equals;
                case BinaryOperatorKind.ObjectValueEquals:
                    return BinaryOperator.ObjectValueEquals;
                case BinaryOperatorKind.NotEquals:
                    return BinaryOperator.NotEquals;
                case BinaryOperatorKind.ObjectValueNotEquals:
                    return BinaryOperator.ObjectValueNotEquals;
                case BinaryOperatorKind.LessThan:
                    return BinaryOperator.LessThan;
                case BinaryOperatorKind.LessThanOrEqual:
                    return BinaryOperator.LessThanOrEqual;
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return BinaryOperator.GreaterThanOrEqual;
                case BinaryOperatorKind.GreaterThan:
                    return BinaryOperator.GreaterThan;
                case BinaryOperatorKind.Like:
                    throw new NotSupportedException("Operator like is not currently supported");
                default:
                    throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null);
            }
        }

        public static UnaryOperator ToTsUnaryOperator(this UnaryOperatorKind unaryOperator)
        {
            switch (unaryOperator)
            {
                case UnaryOperatorKind.BitwiseNegation:
                    return UnaryOperator.BitwiseNegation;
                case UnaryOperatorKind.Not:
                    return UnaryOperator.Not;
                case UnaryOperatorKind.Plus:
                    return UnaryOperator.Plus;
                case UnaryOperatorKind.Minus:
                    return UnaryOperator.Minus;
                case UnaryOperatorKind.True:
                    return UnaryOperator.True;
                case UnaryOperatorKind.False:
                    return UnaryOperator.False;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryOperator), unaryOperator, null);
            }
        }

        public static bool IsEquivalentToMethod(this IMethodSymbol symbol, MethodInfo methodInfo)
        {
            if (symbol.ContainingType.IsEquivalentTo(methodInfo.DeclaringType))
            {
                if (symbol.Parameters.Length == methodInfo.GetParameters().Length)
                {
                    //for (int i = 0; i < symbol.Parameters.Length; i++)
                    //{
                    //    var symbolParameter = symbol.Parameters[i];
                    //    var parameterInfo = methodInfo.GetParameters()[i];
                    //    if (symbolParameter.Type.IsEquivalentTo(parameterInfo.ParameterType) == false)
                    //    {
                    //        return false;
                    //    }
                    //}
                    return true;
                }
            }

            return false;
        }

        public static AccessModifier ToTsModifier(this Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return AccessModifier.Private;
                case Accessibility.Protected:
                    return AccessModifier.Protected;
                case Accessibility.Public:
                default:
                    return AccessModifier.Public;
            }
        }
    }
}
