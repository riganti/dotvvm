using System;
using System.Linq;

namespace DotVVM.Framework.Parser.Binding.Tokenizer
{
    public class BindingTokenizer : TokenizerBase<BindingToken, BindingTokenType>
    {
        protected override BindingTokenType TextTokenType => BindingTokenType.Identifier;

        protected override BindingTokenType WhiteSpaceTokenType => BindingTokenType.WhiteSpace;

        protected override void TokenizeCore()
        {
            while (Peek() != NullChar)
            {
                var ch = Peek();
                
                switch (ch)
                {
                    case '.':
                        if (CurrentTokenChars.Length > 0 && Enumerable.Range(0, CurrentTokenChars.Length).All(i => Char.IsDigit(CurrentTokenChars[i])))
                        {
                            // treat dot in a number as part of the number
                            Read();
                        }
                        else
                        {
                            FinishIncompleteIdentifier();
                            Read();
                            CreateToken(BindingTokenType.Dot);
                        }
                        break;

                    case ',':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.Comma);
                        break;

                    case '(':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OpenParenthesis);
                        break;

                    case ')':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.CloseParenthesis);
                        break;

                    case '[':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OpenArrayBrace);
                        break;

                    case ']':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.CloseArrayBrace);
                        break;

                    case '+':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.AddOperator);
                        break;

                    case '-':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.SubtractOperator);
                        break;

                    case '*':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.MultiplyOperator);
                        break;

                    case '/':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.DivideOperator);
                        break;

                    case '%':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.ModulusOperator);
                        break;

                    case ':':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.ColonOperator);
                        break;

                    case '=':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            CreateToken(BindingTokenType.EqualsEqualsOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.EqualsEqualsOperator, errorProvider: t => CreateTokenError(t, "Assignments are not allowed in bindings!"));
                        }
                        break;

                    case '|':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '|')
                        {
                            Read();
                            CreateToken(BindingTokenType.OrElseOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.OrOperator);
                        }
                        break;

                    case '&':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '&')
                        {
                            Read();
                            CreateToken(BindingTokenType.AndAlsoOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.AndOperator);
                        }
                        break;

                    case '<':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            CreateToken(BindingTokenType.LessThanEqualsOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.LessThanOperator);
                        }
                        break;

                    case '>':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            CreateToken(BindingTokenType.GreaterThanEqualsOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.GreaterThanOperator);
                        }
                        break;

                    case '!':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            CreateToken(BindingTokenType.NotEqualsOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.NotOperator);
                        }
                        break;

                    case '\'':
                    case '"':
                        FinishIncompleteIdentifier();
                        ReadStringLiteral();
                        break;

                    case '?':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '?')
                        {
                            Read();
                            CreateToken(BindingTokenType.NullCoalescingOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.QuestionMarkOperator);
                        }
                        break;

                    default:
                        if (Char.IsWhiteSpace(ch))
                        {
                            // white space
                            FinishIncompleteIdentifier();
                            SkipWhitespace();
                        }
                        else
                        {
                            // text content
                            Read();
                        }
                        break;
                }
            }

            // treat remaining content as text
            FinishIncompleteIdentifier();
        }

        private void FinishIncompleteIdentifier()
        {
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(BindingTokenType.Identifier);
            }
        }

        private void ReadStringLiteral()
        {
            var quoteChar = Peek();
            Read();

            while (Peek() != quoteChar)
            {
                if (Peek() == NullChar)
                {
                    // unfinished string literal
                    CreateToken(BindingTokenType.StringLiteralToken, errorProvider: t => CreateTokenError(t, "The string literal was not closed!"));
                    return;
                }

                if (Peek() == '\\')
                {
                    // escape char - read it and skip the next char so it won't end the loop
                    Read();
                    Read();
                }
                else
                {
                    // normal character - read it
                    Read();
                }
            }
            Read();

            CreateToken(BindingTokenType.StringLiteralToken);
        }
    }
}