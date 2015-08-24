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

        
        internal BindingParserNode ReadExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhitespace();
            return CreateNode(ReadConditionalExpression(), startIndex);
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
            SkipWhitespace();

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
            return CreateNode(ReadAtomicExpression(), startIndex);
        }

        private BindingParserNode ReadAtomicExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhitespace();

            var token = Peek();
            if (token != null && token.Type == BindingTokenType.OpenParenthesis)
            {
                // parenthesised expression
                Read();
                var innerExpression = ReadExpression();
                var error = IsCurrentTokenIncorrect(BindingTokenType.CloseParenthesis);
                Read();
                SkipWhitespace();
                return CreateNode(new ParenthesizedExpressionBindingParserNode(innerExpression), startIndex, error ? "The ')' was expected." : null);
            }
            else if (token != null && token.Type == BindingTokenType.StringLiteralToken)
            {
                // string literal
                var literal = Read();
                SkipWhitespace();

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
            SkipWhitespace();

            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Peek();
                if (identifier.Text == "true" || identifier.Text == "false")
                {
                    Read();
                    SkipWhitespace();
                    return CreateNode(new LiteralExpressionBindingParserNode(identifier.Text == "true"), startIndex);
                }
                else if (identifier.Text == "null")
                {
                    Read();
                    SkipWhitespace();
                    return CreateNode(new LiteralExpressionBindingParserNode(null), startIndex);
                }
                else if (identifier.Text == "_this")
                {
                    Read();
                    SkipWhitespace();
                    return CreateNode(new SpecialPropertyBindingParserNode(BindingSpecialProperty.This), startIndex);
                }
                else if (identifier.Text == "_parent")
                {
                    Read();
                    SkipWhitespace();
                    return CreateNode(new SpecialPropertyBindingParserNode(BindingSpecialProperty.Parent), startIndex);
                }
                else if (identifier.Text == "_root")
                {
                    Read();
                    SkipWhitespace();
                    return CreateNode(new SpecialPropertyBindingParserNode(BindingSpecialProperty.Root), startIndex);
                }
                else if (Char.IsDigit(identifier.Text[0]))
                {
                    // number value
                    double number;
                    string error = null;
                    if (!double.TryParse(identifier.Text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
                    {
                        error = $"The value '{identifier.Text}' is not a valid number.";
                        number = 0;
                    }

                    Read();
                    SkipWhitespace();

                    var node = CreateNode(new LiteralExpressionBindingParserNode(number), startIndex);
                    if (error != null)
                    {
                        node.NodeErrors.Add(error);
                    }
                    return node;
                }
            }

            return CreateNode(ReadIdentifierExpression(), startIndex);
        }

        private BindingParserNode ReadIdentifierExpression()
        {
            BindingParserNode expression = ReadIdentifierNameExpression();

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
                        arguments.Add(ReadExpression());          
                    }
                    var error = IsCurrentTokenIncorrect(BindingTokenType.CloseParenthesis);
                    Read();
                    SkipWhitespace();
                    expression = CreateNode(new FunctionCallBindingParserNode(expression, arguments), startIndex, error ? "The ')' was expected." : null);
                }
                else if (next.Type == BindingTokenType.OpenArrayBrace)
                {
                    // array access
                    Read();
                    var innerExpression = ReadExpression();
                    var error = IsCurrentTokenIncorrect(BindingTokenType.CloseArrayBrace);
                    Read();
                    SkipWhitespace();
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

        private IdentifierNameBindingParserNode ReadIdentifierNameExpression()
        {
            var startIndex = CurrentIndex;
            SkipWhitespace();

            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Read();
                SkipWhitespace();
                return CreateNode(new IdentifierNameBindingParserNode(identifier.Text), startIndex);
            }

            // create virtual empty identifier expression
            return CreateNode(new IdentifierNameBindingParserNode("") { NodeErrors = { "Identifier name was expected!" }}, startIndex);
        }

        private string ParseStringLiteral(string text, out string error)
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
