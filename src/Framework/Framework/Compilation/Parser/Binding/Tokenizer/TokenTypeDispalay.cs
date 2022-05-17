using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public static class TokenTypeDisplay
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static string ToDisplayString(this BindingTokenType tokenType)
        {
            switch(tokenType)
            {
                case BindingTokenType.WhiteSpace: return " ";
                case BindingTokenType.Identifier: return "Identifier";
                case BindingTokenType.Dot: return ".";
                case BindingTokenType.Comma: return ",";
                case BindingTokenType.OpenParenthesis: return "(";
                case BindingTokenType.CloseParenthesis: return ")";
                case BindingTokenType.OpenArrayBrace: return "[";
                case BindingTokenType.CloseArrayBrace: return "]";
                case BindingTokenType.AddOperator: return "+";
                case BindingTokenType.SubtractOperator: return "-";
                case BindingTokenType.MultiplyOperator: return "*";
                case BindingTokenType.DivideOperator: return "/";
                case BindingTokenType.ModulusOperator: return "%";
                case BindingTokenType.UnsupportedOperator: return "UnsupportedOp";
                case BindingTokenType.EqualsEqualsOperator: return "==";
                case BindingTokenType.LessThanOperator: return "<";
                case BindingTokenType.LessThanEqualsOperator: return "<=";
                case BindingTokenType.GreaterThanOperator: return ">";
                case BindingTokenType.GreaterThanEqualsOperator: return ">=";
                case BindingTokenType.NotEqualsOperator: return "!=";
                case BindingTokenType.NotOperator: return "!";
                case BindingTokenType.OnesComplementOperator: return "~";
                case BindingTokenType.StringLiteralToken: return "Literal";
                case BindingTokenType.NullCoalescingOperator: return "?.";
                case BindingTokenType.QuestionMarkOperator: return "?";
                case BindingTokenType.ColonOperator: return ":";
                case BindingTokenType.AndOperator: return "&";
                case BindingTokenType.AndAlsoOperator: return "&&";
                case BindingTokenType.OrOperator: return "|";
                case BindingTokenType.OrElseOperator: return "||";
                case BindingTokenType.AssignOperator: return "=";
                default: return "Unknown";
            }
        }

    }
}
