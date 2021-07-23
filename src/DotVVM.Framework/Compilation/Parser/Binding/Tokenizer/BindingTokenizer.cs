#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public class BindingTokenizer : TokenizerBase<BindingToken, BindingTokenType>
    {
        private readonly ISet<char> operatorCharacters = new HashSet<char> { '+', '-', '*', '/', '^', '\\', '%', '<', '>', '=', '&', '|', '~', '!', ';' };

        protected override BindingTokenType TextTokenType => BindingTokenType.Identifier;

        protected override BindingTokenType WhiteSpaceTokenType => BindingTokenType.WhiteSpace;

        public bool IsOperator(char c) => operatorCharacters.Contains(c);

        public override void Tokenize(string sourceText)
        {
            TokenizeInternal(sourceText, () => { TokenizeBindingValue(); return true; });
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void TokenizeBindingValue()
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
                            if (!char.IsDigit(Peek()))
                            {
                                CreateToken(BindingTokenType.Identifier, 1);
                                CreateToken(BindingTokenType.Dot);
                            }
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
                        EnsureUnsupportedOperator(BindingTokenType.AddOperator);
                        break;

                    case '-':
                        FinishIncompleteIdentifier();
                        Read();
                        EnsureUnsupportedOperator(BindingTokenType.SubtractOperator);
                        break;

                    case '*':
                        FinishIncompleteIdentifier();
                        Read();
                        EnsureUnsupportedOperator(BindingTokenType.MultiplyOperator);
                        break;

                    case '/':
                        FinishIncompleteIdentifier();
                        Read();
                        EnsureUnsupportedOperator(BindingTokenType.DivideOperator);
                        break;

                    case '%':
                        FinishIncompleteIdentifier();
                        Read();
                        EnsureUnsupportedOperator(BindingTokenType.ModulusOperator);
                        break;
                    case '^':
                        FinishIncompleteIdentifier();
                        Read();
                        EnsureUnsupportedOperator(BindingTokenType.UnsupportedOperator);
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
                            EnsureUnsupportedOperator(BindingTokenType.EqualsEqualsOperator);
                        }
                        else if (Peek() == '>')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.LambdaOperator);
                        }
                        else {
                            EnsureUnsupportedOperator(BindingTokenType.AssignOperator);
                        }
                        break;

                    case '|':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '|')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.OrElseOperator);

                        }
                        else
                        {
                            EnsureUnsupportedOperator(BindingTokenType.OrOperator);
                        }
                        break;

                    case '&':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '&')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.AndAlsoOperator);
                        }
                        else
                        {
                            EnsureUnsupportedOperator(BindingTokenType.AndOperator);
                        }
                        break;

                    case '<':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.LessThanEqualsOperator);
                        }
                        else
                        {
                            EnsureUnsupportedOperator(BindingTokenType.LessThanOperator);
                        }
                        break;

                    case '>':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.GreaterThanEqualsOperator);
                        }
                        else
                        {
                            //I need to take something like >>>> into account if it is something like
                            //>&*% or whatever, it will be other operator's problem.
                            CreateToken(BindingTokenType.GreaterThanOperator);
                        }
                        break;

                    case '!':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '=')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.NotEqualsOperator);
                        }
                        else
                        {
                            EnsureUnsupportedOperator(BindingTokenType.NotOperator);
                        }
                        break;

                    case '$':
                    case '\'':
                    case '"':
                        var bindingTokenType = default(BindingTokenType);
                        var errorMessage = default(string);
                        FinishIncompleteIdentifier();

                        if (ch == '$')
                        {
                            bindingTokenType = BindingTokenType.InterpolatedStringToken;
                            ReadInterpolatedString(out errorMessage);
                        }
                        else
                        {
                            bindingTokenType = BindingTokenType.StringLiteralToken;
                            ReadStringLiteral(out errorMessage);
                        }

                        CreateToken(bindingTokenType, errorProvider: t => CreateTokenError(t, errorMessage ?? "unknown error"));
                        break;

                    case '?':
                        FinishIncompleteIdentifier();
                        Read();
                        if (Peek() == '?')
                        {
                            Read();
                            EnsureUnsupportedOperator(BindingTokenType.NullCoalescingOperator);
                        }
                        else
                        {
                            CreateToken(BindingTokenType.QuestionMarkOperator);
                        }
                        break;
                    case ';':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.Semicolon);
                        break;

                    default:
                        if (char.IsWhiteSpace(ch))
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

        protected override BindingToken NewToken(string text, BindingTokenType type, int lineNumber, int columnNumber, int length, int startPosition)
        {
            return new BindingToken(text, type, lineNumber, columnNumber, length, startPosition);
        }

        private void FinishIncompleteIdentifier()
        {
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(BindingTokenType.Identifier);
            }
        }

        internal void EnsureUnsupportedOperator(BindingTokenType preferedOperatorToken)
        {
            if (IsOperator(Peek()))
            {
                while (IsOperator(Peek()))
                {
                    Read();
                }
                CreateToken(BindingTokenType.UnsupportedOperator);
            }
            else
            {
                CreateToken(preferedOperatorToken);
            }
        }

        internal void ReadStringLiteral(out string? errorMessage)
        {
            ReadStringLiteral(Peek, Read, out errorMessage);
        }

        internal void ReadInterpolatedString(out string? errorMessage)
        {
            ReadInterpolatedString(Peek, Read, out errorMessage);
        }

        /// <summary>
        /// Reads the string literal.
        /// </summary>
        internal static void ReadStringLiteral(Func<char> peekFunction, Func<char> readFunction, out string? errorMessage)
        {
            var quoteChar = peekFunction();
            readFunction();

            while (peekFunction() != quoteChar)
            {
                if (peekFunction() == NullChar)
                {
                    // unfinished string literal
                    errorMessage = "The string literal was not closed!";
                    return;
                }

                if (peekFunction() == '\\')
                {
                    // escape char - read it and skip the next char so it won't end the loop
                    readFunction();
                    readFunction();
                }
                else
                {
                    // normal character - read it
                    readFunction();
                }
            }
            readFunction();

            errorMessage = null;
        }

        internal static void ReadInterpolatedString(Func<int, char> peekFunction, Func<char> readFunction, out string? errorMessage)
        {
            readFunction();
            var quoteChar = readFunction();
            var exprDepth = 0;

            while (peekFunction(0) != quoteChar || exprDepth != 0)
            {
                if (peekFunction(0) == NullChar)
                {
                    errorMessage = "Interpolated string was not closed!";
                    return;
                }

                if (peekFunction(0) == '\\' && exprDepth == 0)
                {
                    readFunction();
                }
                else if (peekFunction(0) == '{' && peekFunction(1) != '{')
                {
                    exprDepth++;
                }
                else if (peekFunction(0) == '{' && peekFunction(1) == '{')
                {
                    readFunction();
                }
                else if (peekFunction(0) == '}' && peekFunction(1) != '}')
                {
                    if (--exprDepth <= -1)
                    {
                        errorMessage = "Could not find matching '{' character!";
                        return;
                    }
                }
                else if (peekFunction(0) == '}' && peekFunction(1) == '}')
                {
                    readFunction();
                }

                readFunction();
            }

            readFunction();
            errorMessage = null;
        }
    }
}
