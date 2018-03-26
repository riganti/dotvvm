using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public static class TsSyntaxExtensions
    {
        public static string ToDisplayString(this TsModifier modifier)
        {
            switch (modifier)
            {
                case TsModifier.Public:
                    return "public";
                case TsModifier.Private:
                    return "private";
                case TsModifier.Protected:
                    return "protected";
                default:
                    return string.Empty;
            }
        }

        public static string ToDisplayString(this TsBinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case TsBinaryOperator.Add:
                    return "+";
                case TsBinaryOperator.Subtract:
                    return "-";
                case TsBinaryOperator.Multiply:
                    return "*";
                case TsBinaryOperator.Divide:
                    return "/";
                case TsBinaryOperator.Remainder:
                    return "%";
                case TsBinaryOperator.And:
                    return "&";
                case TsBinaryOperator.Or:
                    return "|";
                case TsBinaryOperator.ExclusiveOr:
                    return "^";
                case TsBinaryOperator.ConditionalAnd:
                    return "&&";
                case TsBinaryOperator.ConditionalOr:
                    return "||";
                case TsBinaryOperator.Equals:
                case TsBinaryOperator.ObjectValueEquals:
                    return "==";
                case TsBinaryOperator.NotEquals:
                case TsBinaryOperator.ObjectValueNotEquals:
                    return "!=";
                case TsBinaryOperator.LessThan:
                    return "<";
                case TsBinaryOperator.LessThanOrEqual:
                    return "<=";
                case TsBinaryOperator.GreaterThan:
                    return ">";
                case TsBinaryOperator.GreaterThanOrEqual:
                    return ">=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null);
            }
        }
    }
}
