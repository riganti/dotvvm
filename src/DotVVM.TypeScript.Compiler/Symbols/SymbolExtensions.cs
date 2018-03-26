using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
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

        public static TsBinaryOperator ToTsBinaryOperator(this BinaryOperatorKind binaryOperator)
        {
            switch (binaryOperator)
            {
                case BinaryOperatorKind.Add:
                    return TsBinaryOperator.Add;
                case BinaryOperatorKind.Subtract:
                    return TsBinaryOperator.Subtract;
                case BinaryOperatorKind.Multiply:
                    return TsBinaryOperator.Multiply;
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.IntegerDivide:
                    return TsBinaryOperator.Divide;
                case BinaryOperatorKind.Remainder:
                    return TsBinaryOperator.Remainder;
                case BinaryOperatorKind.Power:
                    throw new NotSupportedException("Operator Power is not supported.");
                case BinaryOperatorKind.LeftShift:
                    throw new NotSupportedException("Operator left shift is not supported.");
                case BinaryOperatorKind.RightShift:
                    throw new NotSupportedException("Operator right shift is not supported.");
                case BinaryOperatorKind.And:
                    return TsBinaryOperator.And;
                case BinaryOperatorKind.Or:
                    return TsBinaryOperator.Or;
                case BinaryOperatorKind.ExclusiveOr:
                    return TsBinaryOperator.ExclusiveOr;
                case BinaryOperatorKind.ConditionalAnd:
                    return TsBinaryOperator.ConditionalAnd;
                case BinaryOperatorKind.ConditionalOr:
                    return TsBinaryOperator.ConditionalOr;
                case BinaryOperatorKind.Concatenate:
                    return TsBinaryOperator.Add;
                case BinaryOperatorKind.Equals:
                    return TsBinaryOperator.Equals;
                case BinaryOperatorKind.ObjectValueEquals:
                    return TsBinaryOperator.ObjectValueEquals;
                case BinaryOperatorKind.NotEquals:
                    return TsBinaryOperator.NotEquals;
                case BinaryOperatorKind.ObjectValueNotEquals:
                    return TsBinaryOperator.ObjectValueNotEquals;
                case BinaryOperatorKind.LessThan:
                    return TsBinaryOperator.LessThan;
                case BinaryOperatorKind.LessThanOrEqual:
                    return TsBinaryOperator.LessThanOrEqual;
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return TsBinaryOperator.GreaterThanOrEqual;
                case BinaryOperatorKind.GreaterThan:
                    return TsBinaryOperator.GreaterThan;
                case BinaryOperatorKind.Like:
                    throw new NotSupportedException("Operator like is not currently supported");
                default:
                    throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null);
            }
        }

        public static TsModifier ToTsModifier(this Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return TsModifier.Private;
                case Accessibility.Protected:
                    return TsModifier.Protected;
                case Accessibility.Public:
                default:
                    return TsModifier.Public;
            }
        }
    }
}
