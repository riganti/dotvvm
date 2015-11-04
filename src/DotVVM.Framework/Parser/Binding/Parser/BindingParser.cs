using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Parser.Binding.Tokenizer;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class BindingParser : ParserBase<BindingToken, BindingTokenType>
    {
        protected override BindingTokenType WhiteSpaceToken => BindingTokenType.WhiteSpace;

        public BindingParserNode ReadExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhiteSpace();
            return CreateNode(ReadAssignmentExpression(), startIndex);
        }

        public bool OnEnd()
        {
            return CurrentIndex >= Tokens.Count;
        }

        private BindingParserNode ReadAssignmentExpression()
        {
            var first = ReadConditionalExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.AssignOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadAssignmentExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.AssignOperator), startIndex);
            }
            else return first;
        }

        private BindingParserNode ReadConditionalExpression()
        {
            var first = ReadNullCoalescingExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.QuestionMarkOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadConditionalExpression();
                var error = IsCurrentTokenIncorrect(BindingTokenType.ColonOperator);
                Read();
                var third = ReadConditionalExpression();

                return CreateNode(new ConditionalExpressionBindingParserNode(first, second, third), startIndex, error ? "The ':' was expected." : null);
            }
            else
            {
                return first;
            }
        }

        private BindingParserNode ReadNullCoalescingExpression()
        {
            var first = ReadOrElseExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.NullCoalescingOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadNullCoalescingExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.NullCoalescingOperator), startIndex);
            }
            else
            {
                return first;
            }
        }

        private BindingParserNode ReadOrElseExpression()
        {
            var first = ReadAndAlsoExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.OrElseOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadOrElseExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.OrElseOperator), startIndex);
            }
            return first;
        }

        private BindingParserNode ReadAndAlsoExpression()
        {
            var first = ReadOrExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.AndAlsoOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadAndAlsoExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.AndAlsoOperator), startIndex);
            }
            return first;
        }

        private BindingParserNode ReadOrExpression()
        {
            var first = ReadAndExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.OrOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadOrExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.OrOperator), startIndex);
            }
            return first;
        }

        private BindingParserNode ReadAndExpression()
        {
            var first = ReadEqualityExpression();
            if (Peek() != null && Peek().Type == BindingTokenType.AndOperator)
            {
                var startIndex = CurrentIndex;
                Read();
                var second = ReadAndExpression();
                return CreateNode(new BinaryOperatorBindingParserNode(first, second, BindingTokenType.AndOperator), startIndex);
            }
            return first;
        }

        private BindingParserNode ReadEqualityExpression()
        {
            var first = ReadComparisonExpression();
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.EqualsEqualsOperator || @operator == BindingTokenType.NotEqualsOperator)
                {
                    var startIndex = CurrentIndex;
                    Read();
                    var second = ReadEqualityExpression();
                    return CreateNode(new BinaryOperatorBindingParserNode(first, second, @operator), startIndex);
                }
            }
            return first;
        }

        private BindingParserNode ReadComparisonExpression()
        {
            var first = ReadAdditiveExpression();
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.LessThanEqualsOperator || @operator == BindingTokenType.LessThanOperator
                    || @operator == BindingTokenType.GreaterThanEqualsOperator || @operator == BindingTokenType.GreaterThanOperator)
                {
                    var startIndex = CurrentIndex;
                    Read();
                    var second = ReadComparisonExpression();
                    return CreateNode(new BinaryOperatorBindingParserNode(first, second, @operator), startIndex);
                }
            }
            return first;
        }

        private BindingParserNode ReadAdditiveExpression()
        {
            var first = ReadMultiplicativeExpression();
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.AddOperator || @operator == BindingTokenType.SubtractOperator)
                {
                    var startIndex = CurrentIndex;
                    Read();
                    var second = ReadAdditiveExpression();
                    return CreateNode(new BinaryOperatorBindingParserNode(first, second, @operator), startIndex);
                }
            }
            return first;
        }

        private BindingParserNode ReadMultiplicativeExpression()
        {
            var first = ReadUnaryExpression();
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.MultiplyOperator || @operator == BindingTokenType.DivideOperator || @operator == BindingTokenType.ModulusOperator)
                {
                    var startIndex = CurrentIndex;
                    Read();
                    var second = ReadMultiplicativeExpression();
                    return CreateNode(new BinaryOperatorBindingParserNode(first, second, @operator), startIndex);
                }
            }
            return first;
        }

        private BindingParserNode ReadUnaryExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhiteSpace();

            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.NotOperator || @operator == BindingTokenType.SubtractOperator)
                {
                    Read();
                    var target = ReadUnaryExpression();
                    return CreateNode(new UnaryOperatorBindingParserNode(target, @operator), startIndex);
                }
            }
            return CreateNode(ReadIdentifierExpression(), startIndex);
        }

        private BindingParserNode ReadIdentifierExpression()
        {
            BindingParserNode expression = ReadAtomicExpression();

            var next = Peek();
            while (next != null)
            {
                var startIndex = CurrentIndex;
                if (next.Type == BindingTokenType.Dot)
                {
                    // member access
                    Read();
                    var member = ReadIdentifierNameExpression();
                    expression = CreateNode(new MemberAccessBindingParserNode(expression, member), startIndex);
                }
                else if (next.Type == BindingTokenType.OpenParenthesis)
                {
                    // function call
                    Read();
                    var arguments = new List<BindingParserNode>();
                    while (Peek() != null && Peek().Type != BindingTokenType.CloseParenthesis)
                    {
                        if (arguments.Count > 0)
                        {
                            SkipWhiteSpace();
                            if (IsCurrentTokenIncorrect(BindingTokenType.Comma))
                                arguments.Add(CreateNode(new LiteralExpressionBindingParserNode(null), CurrentIndex, "The ',' was expected"));
                            else Read();
                        }
                        arguments.Add(ReadExpression());
                    }
                    var error = IsCurrentTokenIncorrect(BindingTokenType.CloseParenthesis);
                    Read();
                    SkipWhiteSpace();
                    expression = CreateNode(new FunctionCallBindingParserNode(expression, arguments), startIndex, error ? "The ')' was expected." : null);
                }
                else if (next.Type == BindingTokenType.OpenArrayBrace)
                {
                    // array access
                    Read();
                    var innerExpression = ReadExpression();
                    var error = IsCurrentTokenIncorrect(BindingTokenType.CloseArrayBrace);
                    Read();
                    SkipWhiteSpace();
                    expression = CreateNode(new ArrayAccessBindingParserNode(expression, innerExpression), startIndex, error ? "The ']' was expected." : null);
                }
                else
                {
                    break;
                }

                next = Peek();
            }
            return expression;
        }
        private BindingParserNode ReadAtomicExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhiteSpace();

            var token = Peek();
            if (token != null && token.Type == BindingTokenType.OpenParenthesis)
            {
                // parenthesised expression
                Read();
                var innerExpression = ReadExpression();
                var error = IsCurrentTokenIncorrect(BindingTokenType.CloseParenthesis);
                Read();
                SkipWhiteSpace();
                return CreateNode(new ParenthesizedExpressionBindingParserNode(innerExpression), startIndex, error ? "The ')' was expected." : null);
            }
            else if (token != null && token.Type == BindingTokenType.StringLiteralToken)
            {
                // string literal
                var literal = Read();
                SkipWhiteSpace();

                string error;
                var node = CreateNode(new LiteralExpressionBindingParserNode(ParseStringLiteral(literal.Text, out error)), startIndex);
                if (error != null)
                {
                    node.NodeErrors.Add(error);
                }
                return node;
            }
            else
            {
                // identifier
                return CreateNode(ReadConstantExpression(), startIndex);
            }
        }

        private BindingParserNode ReadConstantExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhiteSpace();

            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Peek();
                if (identifier.Text == "true" || identifier.Text == "false")
                {
                    Read();
                    SkipWhiteSpace();
                    return CreateNode(new LiteralExpressionBindingParserNode(identifier.Text == "true"), startIndex);
                }
                else if (identifier.Text == "null")
                {
                    Read();
                    SkipWhiteSpace();
                    return CreateNode(new LiteralExpressionBindingParserNode(null), startIndex);
                }
                else if (Char.IsDigit(identifier.Text[0]))
                {
                    // number value
                    string error;
                    var number = ParseNumberLiteral(identifier.Text, out error);

                    Read();
                    SkipWhiteSpace();

                    var node = CreateNode(new LiteralExpressionBindingParserNode(number), startIndex);
                    if (error != null)
                    {
                        node.NodeErrors.Add(error);
                    }
                    return node;
                }
            }

            return CreateNode(ReadIdentifierNameExpression(), startIndex);
        }


        private IdentifierNameBindingParserNode ReadIdentifierNameExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhiteSpace();

            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Read();
                SkipWhiteSpace();
                return CreateNode(new IdentifierNameBindingParserNode(identifier.Text), startIndex);
            }

            // create virtual empty identifier expression
            return CreateNode(new IdentifierNameBindingParserNode("") { NodeErrors = { "Identifier name was expected!" } }, startIndex);
        }

        private static object ParseNumberLiteral(string text, out string error)
        {
            text = text.ToLower();
            error = null;
            NumberLiteralSuffix type = NumberLiteralSuffix.None;
            var lastDigit = text[text.Length - 1];
            if (char.IsLetter(lastDigit))
            {
                // number type suffix
                if (lastDigit == 'm') type = NumberLiteralSuffix.Decimal;
                else if (lastDigit == 'f') type = NumberLiteralSuffix.Float;
                else if (lastDigit == 'd') type = NumberLiteralSuffix.Double;
                else if (text.EndsWith("ul", StringComparison.Ordinal) || text.EndsWith("lu", StringComparison.Ordinal)) type = NumberLiteralSuffix.UnsignedLong;
                else if (lastDigit == 'u') type = NumberLiteralSuffix.Unsigned;
                else if (lastDigit == 'l') type = NumberLiteralSuffix.Long;
                else
                {
                    error = "number literal type suffix not known";
                    return null;
                }

                if (type == NumberLiteralSuffix.UnsignedLong) text = text.Remove(text.Length - 2); // remove 2 last chars
                else text = text.Remove(text.Length - 1); // remove last char
            }
            if (text.Contains(".") || text.Contains("e") || type == NumberLiteralSuffix.Float || type == NumberLiteralSuffix.Double)
            {
                const NumberStyles decimalStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                // real number
                switch (type)
                {
                    case NumberLiteralSuffix.None: // double is defualt
                    case NumberLiteralSuffix.Double:
                        return TryParse<double>(double.TryParse, text, out error, decimalStyle);
                    case NumberLiteralSuffix.Float:
                        return TryParse<float>(float.TryParse, text, out error, decimalStyle);
                    case NumberLiteralSuffix.Decimal:
                        return TryParse<decimal>(decimal.TryParse, text, out error, decimalStyle);
                    default:
                        error = $"could not parse real number of type { type }";
                        return null;
                }
            }
            const NumberStyles integerStyle = NumberStyles.AllowLeadingSign;
            // try parse integral constant
            object result = null;
            if (type == NumberLiteralSuffix.None)
            {
                result = TryParse<int>(int.TryParse, text, integerStyle) ??
                    TryParse<uint>(uint.TryParse, text, integerStyle) ??
                    TryParse<long>(long.TryParse, text, integerStyle) ??
                    TryParse<ulong>(ulong.TryParse, text, integerStyle);
            }
            else if (type == NumberLiteralSuffix.Unsigned)
            {
                result = TryParse<uint>(uint.TryParse, text, integerStyle) ??
                    TryParse<ulong>(ulong.TryParse, text, integerStyle);
            }
            else if (type == NumberLiteralSuffix.Long)
            {
                result = TryParse<long>(long.TryParse, text, integerStyle) ??
                    TryParse<ulong>(ulong.TryParse, text, integerStyle);
            }
            else if (type == NumberLiteralSuffix.UnsignedLong)
            {
                result = TryParse<ulong>(ulong.TryParse, text, integerStyle);
            }
            if (result != null) return result;
            // handle errors

            // if all are digits, or '0x' + hex digits => too large number
            if (text.All(char.IsDigit) ||
                (text.StartsWith("0x", StringComparison.Ordinal) && text.Skip(2).All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f'))))
                error = $"number number {text} is too large for integral literal, try to append 'd' to real number literal";
            else error = $"could not parse {text} as numeric literal";
            return null;
        }

        delegate bool TryParseDelegate<T>(string text, NumberStyles styles, IFormatProvider format, out T result);
        private static object TryParse<T>(TryParseDelegate<T> method, string text, out string error, NumberStyles styles)
        {
            error = null;
            T result;
            if (method(text, styles, CultureInfo.InvariantCulture, out result)) return result;
            error = $"could not parse { text } using { method.Method.DeclaringType.FullName + "." + method.Method.Name }";
            return null;
        }

        private static object TryParse<T>(TryParseDelegate<T> method, string text, NumberStyles styles)
        {
            T result;
            if (method(text, styles, CultureInfo.InvariantCulture, out result)) return result;
            return null;
        }

        private static string ParseStringLiteral(string text, out string error)
        {
            error = null;
            var sb = new StringBuilder();
            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '\\')
                {
                    // handle escaped characters
                    i++;
                    if (i == text.Length - 1)
                    {
                        error = "The escape character cannot be at the end of the string literal!";
                    }
                    else if (text[i] == '\'' || text[i] == '"' || text[i] == '\\')
                    {
                        sb.Append(text[i]);
                    }
                    else if (text[i] == 'n')
                    {
                        sb.Append('\n');
                    }
                    else if (text[i] == 'r')
                    {
                        sb.Append('\r');
                    }
                    else if (text[i] == 't')
                    {
                        sb.Append('\t');
                    }
                    else
                    {
                        error = "The escape sequence is either not valid or not supported in dotVVM bindings!";
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            return sb.ToString();
        }

        private T CreateNode<T>(T node, int startIndex, string error = null) where T : BindingParserNode
        {
            node.Tokens.Clear();
            node.Tokens.AddRange(GetTokensFrom(startIndex));

            if (startIndex < Tokens.Count)
            {
                node.StartPosition = Tokens[startIndex].StartPosition;
            }
            node.Length = node.Tokens.Sum(t => (int?)t.Length) ?? 0;

            if (error != null)
            {
                node.NodeErrors.Add(error);
            }

            return node;
        }

        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        protected bool IsCurrentTokenIncorrect(BindingTokenType desiredType)
        {
            if (Peek() == null || !Peek().Type.Equals(desiredType))
            {
                return true;
            }
            return false;
        }
    }
}
