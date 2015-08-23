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
    public class BindingParser : ParserBase<BindingToken, BindingTokenType>, IBindingParser
    {
        protected override BindingTokenType WhiteSpaceToken => BindingTokenType.WhiteSpace;


        public Expression Parse(string expression, DataContextStack dataContexts)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(new StringReader(expression));
            
            var node = ReadExpression(dataContexts);
            if (Peek() != null)
            {
                throw new Exception("End of expression expected!");
            }

            throw new NotSupportedException();
        }

        internal BindingParserNode ReadExpression(DataContextStack dataContexts)
        {
            SkipWhitespace();
            return ReadConditionalExpression(dataContexts);
        }



        private BindingParserNode ReadConditionalExpression(DataContextStack dataContextStack)
        {
            var first = ReadNullCoalescingExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.QuestionMarkOperator)
            {
                Read();
                var second = ReadConditionalExpression(dataContextStack);
                Assert(BindingTokenType.ColonOperator);
                Read();
                var third = ReadConditionalExpression(dataContextStack);

                return new ConditionalBindingParserNode(first, second, third);
            }
            else
            {
                return first;
            }
        }

        private BindingParserNode ReadNullCoalescingExpression(DataContextStack dataContextStack)
        {
            var first = ReadOrElseExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.NullCoalescingOperator)
            {
                Read();
                var second = ReadNullCoalescingExpression(dataContextStack);
                return new BinaryOperatorBindingParserNode(first, second, BindingTokenType.NullCoalescingOperator);
            }
            else
            {
                return first;
            }
        }

        private BindingParserNode ReadOrElseExpression(DataContextStack dataContextStack)
        {
            var first = ReadAndAlsoExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.OrElseOperator)
            {
                Read();
                var second = ReadOrElseExpression(dataContextStack);
                return new BinaryOperatorBindingParserNode(first, second, BindingTokenType.OrElseOperator);
            }
            return first;
        }

        private BindingParserNode ReadAndAlsoExpression(DataContextStack dataContextStack)
        {
            var first = ReadOrExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.AndAlsoOperator)
            {
                Read();
                var second = ReadAndAlsoExpression(dataContextStack);
                return new BinaryOperatorBindingParserNode(first, second, BindingTokenType.AndAlsoOperator);
            }
            return first;
        }

        private BindingParserNode ReadOrExpression(DataContextStack dataContextStack)
        {
            var first = ReadAndExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.OrOperator)
            {
                Read();
                var second = ReadOrExpression(dataContextStack);
                return new BinaryOperatorBindingParserNode(first, second, BindingTokenType.OrOperator);
            }
            return first;
        }

        private BindingParserNode ReadAndExpression(DataContextStack dataContextStack)
        {
            var first = ReadEqualityExpression(dataContextStack);
            if (Peek() != null && Peek().Type == BindingTokenType.AndOperator)
            {
                Read();
                var second = ReadAndExpression(dataContextStack);
                return new BinaryOperatorBindingParserNode(first, second, BindingTokenType.AndOperator);
            }
            return first;
        }

        private BindingParserNode ReadEqualityExpression(DataContextStack dataContextStack)
        {
            var first = ReadComparisonExpression(dataContextStack);
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.EqualsEqualsOperator || @operator == BindingTokenType.NotEqualsOperator)
                {
                    Read();
                    var second = ReadEqualityExpression(dataContextStack);
                    return new BinaryOperatorBindingParserNode(first, second, @operator);
                }
            }
            return first;
        }

        private BindingParserNode ReadComparisonExpression(DataContextStack dataContextStack)
        {
            var first = ReadAdditiveExpression(dataContextStack);
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.LessThanEqualsOperator || @operator == BindingTokenType.LessThanOperator
                    || @operator == BindingTokenType.GreaterThanEqualsOperator || @operator == BindingTokenType.GreaterThanOperator)
                {
                    Read();
                    var second = ReadComparisonExpression(dataContextStack);
                    return new BinaryOperatorBindingParserNode(first, second, @operator);
                }
            }
            return first;
        }

        private BindingParserNode ReadAdditiveExpression(DataContextStack dataContextStack)
        {
            var first = ReadMultiplicativeExpression(dataContextStack);
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.AddOperator || @operator == BindingTokenType.SubtractOperator)
                {
                    Read();
                    var second = ReadAdditiveExpression(dataContextStack);
                    return new BinaryOperatorBindingParserNode(first, second, @operator);
                }
            }
            return first;
        }

        private BindingParserNode ReadMultiplicativeExpression(DataContextStack dataContextStack)
        {
            var first = ReadUnaryExpression(dataContextStack);
            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.MultiplyOperator || @operator == BindingTokenType.DivideOperator || @operator == BindingTokenType.ModulusOperator)
                {
                    Read();
                    var second = ReadMultiplicativeExpression(dataContextStack);
                    return new BinaryOperatorBindingParserNode(first, second, @operator);
                }
            }
            return first;
        }

        private BindingParserNode ReadUnaryExpression(DataContextStack dataContextStack)
        {
            SkipWhitespace();

            if (Peek() != null)
            {
                var @operator = Peek().Type;
                if (@operator == BindingTokenType.NotOperator || @operator == BindingTokenType.SubtractOperator)
                {
                    Read();
                    var target = ReadUnaryExpression(dataContextStack);
                    return new UnaryOperatorBindingParserNode(target, @operator);
                }
            }
            return ReadAtomicExpression(dataContextStack);
        }

        private BindingParserNode ReadAtomicExpression(DataContextStack dataContexts)
        {
            SkipWhitespace();

            var token = Peek();
            if (token.Type == BindingTokenType.OpenParenthesis)
            {
                // parenthesised expression
                Read();
                var innerExpression = ReadExpression(dataContexts);
                Assert(BindingTokenType.CloseParenthesis);
                Read();
                SkipWhitespace();
                return new ParenthesizedBindingParserNode(innerExpression);
            }
            else if (token.Type == BindingTokenType.StringLiteralToken)
            {
                // string literal
                var literal = Read();
                SkipWhitespace();
                return new LiteralBindingParserNode(ParseStringLiteral(dataContexts, literal.Text));
            }
            else 
            {
                // identifier
                return ReadConstantExpression(dataContexts);
            }
        }

        private BindingParserNode ReadConstantExpression(DataContextStack dataContexts)
        {
            SkipWhitespace();

            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Peek();
                if (identifier.Text == "true" || identifier.Text == "false")
                {
                    Read();
                    SkipWhitespace();
                    return new LiteralBindingParserNode(identifier.Text == "true");
                }
                else if (identifier.Text == "null")
                {
                    Read();
                    SkipWhitespace();
                    return new LiteralBindingParserNode(null);
                }
                else if (identifier.Text == "_this")
                {
                    Read();
                    SkipWhitespace();
                    return new SpecialPropertyBindingParserNode(BindingSpecialProperty.This);
                }
                else if (identifier.Text == "_parent")
                {
                    Read();
                    SkipWhitespace();
                    return new SpecialPropertyBindingParserNode(BindingSpecialProperty.Parent);
                }
                else if (identifier.Text == "_root")
                {
                    Read();
                    SkipWhitespace();
                    return new SpecialPropertyBindingParserNode(BindingSpecialProperty.Root);
                }
                else if (Char.IsDigit(identifier.Text[0]))
                {
                    // number value
                    double number;
                    if (!double.TryParse(identifier.Text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
                    {
                        ThrowParserException(dataContexts, $"The value '{identifier.Text}' is not a valid number.");
                    }

                    Read();
                    SkipWhitespace();
                    return new LiteralBindingParserNode(number);
                }
            }

            return ReadIdentifierExpression(dataContexts);
        }

        private BindingParserNode ReadIdentifierExpression(DataContextStack dataContexts)
        {
            BindingParserNode expression = ReadIdentifierNameExpression(dataContexts);

            var next = Peek();
            while (next != null)
            {
                if (next.Type == BindingTokenType.Dot)
                {
                    // member access
                    Read();
                    var member = ReadIdentifierNameExpression(dataContexts);                                        // TODO - change dataContexts
                    expression = new MemberAccessBindingParserNode(expression, member);
                }
                else if (next.Type == BindingTokenType.OpenParenthesis)
                {
                    // function call
                    Read();
                    var arguments = new List<BindingParserNode>();
                    while (Peek() != null && Peek().Type != BindingTokenType.CloseParenthesis)
                    {
                        arguments.Add(ReadExpression(dataContexts));                                                // TODO - change dataContexts
                    }
                    Assert(BindingTokenType.CloseParenthesis);
                    Read();
                    SkipWhitespace();
                    expression = new FunctionCallBindingParserNode(expression, arguments);
                }
                else if (next.Type == BindingTokenType.OpenArrayBrace)
                {
                    // array access
                    Read();
                    var innerExpression = ReadExpression(dataContexts);                                             // TODO - change dataContexts
                    Assert(BindingTokenType.CloseArrayBrace);
                    Read();
                    SkipWhitespace();
                    expression = new ArrayAccessBindingParserNode(expression, innerExpression);
                }
                else
                {
                    break;
                }

                next = Peek();
            }
            return expression;
        }

        private IdentifierNameBindingParserNode ReadIdentifierNameExpression(DataContextStack dataContexts)
        {
            SkipWhitespace();
            if (Peek() != null && Peek().Type == BindingTokenType.Identifier)
            {
                var identifier = Read();
                SkipWhitespace();
                return new IdentifierNameBindingParserNode(identifier.Text);
            }

            ThrowParserException(dataContexts, "Identifier was expected!");
            return null;
        }

        private string ParseStringLiteral(DataContextStack dataContexts, string text)
        {
            var sb = new StringBuilder();
            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '\\')
                {
                    // handle escaped characters
                    i++;
                    if (i == text.Length - 1)
                    {
                        ThrowParserException(dataContexts, "The escape character cannot be at the end of the string literal!");
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
                        ThrowParserException(dataContexts, "The escape sequence is either not valid or not supported in dotVVM bindings!");
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            return sb.ToString();
        }

        private void ThrowParserException(DataContextStack dataContexts, string message)
        {
            throw new NotImplementedException();
        }
    }

    public class MemberAccessBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; set; }
        public IdentifierNameBindingParserNode MemberNameExpression { get; set; }

        public MemberAccessBindingParserNode(BindingParserNode targetExpression, IdentifierNameBindingParserNode memberNameExpression)
        {
            TargetExpression = targetExpression;
            MemberNameExpression = memberNameExpression;
        }
    }

    public class ConditionalBindingParserNode : BindingParserNode
    {
        public BindingParserNode ConditionExpression { get; private set; }
        public BindingParserNode TrueExpression { get; private set; }
        public BindingParserNode FalseExpression { get; private set; }

        public ConditionalBindingParserNode(BindingParserNode conditionExpression, BindingParserNode trueExpression, BindingParserNode falseExpression)
        {
            ConditionExpression = conditionExpression;
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;
        }
    }

    public class BinaryOperatorBindingParserNode : BindingParserNode
    {
        public BindingParserNode FirstExpression { get; private set; }
        public BindingParserNode SecondExpression { get; private set; }
        public BindingTokenType Operator { get; private set; }

        public BinaryOperatorBindingParserNode(BindingParserNode firstExpression, BindingParserNode secondExpression, BindingTokenType @operator)
        {
            FirstExpression = firstExpression;
            SecondExpression = secondExpression;
            Operator = @operator;
        }
    }

    public class FunctionCallBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; private set; }
        public List<BindingParserNode> ArgumentExpressions { get; private set; }

        public FunctionCallBindingParserNode(BindingParserNode targetExpression, List<BindingParserNode> argumentExpressions)
        {
            TargetExpression = targetExpression;
            ArgumentExpressions = argumentExpressions;
        }
    }

    public class ArrayAccessBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; private set; }
        public BindingParserNode ArrayIndexExpression { get; private set; }

        public ArrayAccessBindingParserNode(BindingParserNode targetExpression, BindingParserNode arrayIndexExpression)
        {
            TargetExpression = targetExpression;
            ArrayIndexExpression = arrayIndexExpression;
        }
    }

    public class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name { get; private set; }

        public IdentifierNameBindingParserNode(string name)
        {
            Name = name;
        }
    }

    public class SpecialPropertyBindingParserNode : BindingParserNode
    {
        public BindingSpecialProperty SpecialProperty { get; private set; }

        public SpecialPropertyBindingParserNode(BindingSpecialProperty specialProperty)
        {
            SpecialProperty = specialProperty;
        }
    }

    public enum BindingSpecialProperty
    {
        This,
        Parent,
        Root
    }

    public class LiteralBindingParserNode : BindingParserNode
    {
        public object Value { get; set; }

        public LiteralBindingParserNode(object value)
        {
            Value = value;
        }
    }

    public class UnaryOperatorBindingParserNode : BindingParserNode
    {
        public BindingParserNode InnerExpression { get; private set; }
        public BindingTokenType Operator { get; private set; }

        public UnaryOperatorBindingParserNode(BindingParserNode innerExpression, BindingTokenType @operator)
        {
            InnerExpression = innerExpression;
            Operator = @operator;
        }
    }

    public class ParenthesizedBindingParserNode : BindingParserNode
    {
        public BindingParserNode InnerExpression { get; private set; }

        public ParenthesizedBindingParserNode(BindingParserNode innerExpression)
        {
            InnerExpression = innerExpression;
        }
    }

    public abstract class BindingParserNode
    {
    }
}
