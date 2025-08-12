using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public class BindingTokenizer : TokenizerBase<BindingToken, BindingTokenType>
    {
        private static readonly HashSet<char> operatorCharacters = new HashSet<char> { '+', '-', '*', '/', '^', '\\', '%', '<', '>', '=', '&', '|', '~', '!', ';' };
        private static readonly HashSet<char> unaryOperatorCharacters = new HashSet<char> { '+', '-', '~', '!' };
        internal readonly int bindingPositionOffset;

        public BindingTokenizer(int bindingPositionOffset = 0) : base(BindingTokenType.Identifier, BindingTokenType.WhiteSpace)
        {
            this.bindingPositionOffset = bindingPositionOffset;
        }

        public bool IsOperator(char c) => operatorCharacters.Contains(c);

        public bool IsUnaryOperator(char c) => unaryOperatorCharacters.Contains(c);

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
                    case '@':
                        FinishIncompleteIdentifier();
                        Read(); // consume @
                        ReadEscapedIdentifier();
                        break;

                    case '.':
                        if (DistanceSinceLastToken > 0 && GetCurrentTokenText().All(c => Char.IsDigit(c)))
                        {
                            // treat dot in a number as part of the number
                            Read();
                            if (!char.IsDigit(Peek()))
                            {
                                CreateToken(BindingTokenType.Identifier, 1);
                                CreateToken(BindingTokenType.Dot, ".");
                            }
                        }
                        else
                        {
                            FinishIncompleteIdentifier();
                            Read();
                            CreateToken(BindingTokenType.Dot, ".");
                        }
                        break;

                    case ',':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.Comma, ",");
                        break;

                    case '(':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OpenParenthesis, "(");
                        break;

                    case ')':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.CloseParenthesis, ")");
                        break;

                    case '[':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OpenArrayBrace, "[");
                        break;

                    case ']':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.CloseArrayBrace, "]");
                        break;

                    case '{':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OpenCurlyBrace, "{");
                        break;

                    case '}':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.CloseCurlyBrace, "}");
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
                        EnsureUnsupportedOperator(BindingTokenType.ExclusiveOrOperator);
                        break;

                    case ':':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.ColonOperator, ":");
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

                    case '~':
                        FinishIncompleteIdentifier();
                        Read();
                        CreateToken(BindingTokenType.OnesComplementOperator);
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
                        CreateToken(BindingTokenType.Semicolon, ";");
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
            return new BindingToken(text, type, lineNumber, columnNumber, length, startPosition + bindingPositionOffset);
        }

        private void ReadEscapedIdentifier()
        {
            if (!char.IsLetter(Peek()) && Peek() != '_')
            {
                CreateToken(BindingTokenType.EscapedIdentifier, errorProvider: t => CreateTokenError(t, "Expected identifier after '@'"));
                return;
            }

            while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
                Read();

            CreateToken(BindingTokenType.EscapedIdentifier);
        }

        private void FinishIncompleteIdentifier()
        {
            if (DistanceSinceLastToken > 0)
            {
                var text = GetCurrentTokenText();

                CreateToken(GetIdentifierTokenType(text));
            }
        }

        private static BindingTokenType GetIdentifierTokenType(string text) =>
            text switch {
                "abstract" => BindingTokenType.KeywordAbstract,
                "as" => BindingTokenType.KeywordAs,
                "base" => BindingTokenType.KeywordBase,
                "bool" => BindingTokenType.KeywordBool,
                "break" => BindingTokenType.KeywordBreak,
                "byte" => BindingTokenType.KeywordByte,
                "case" => BindingTokenType.KeywordCase,
                "catch" => BindingTokenType.KeywordCatch,
                "char" => BindingTokenType.KeywordChar,
                "checked" => BindingTokenType.KeywordChecked,
                "class" => BindingTokenType.KeywordClass,
                "const" => BindingTokenType.KeywordConst,
                "continue" => BindingTokenType.KeywordContinue,
                "decimal" => BindingTokenType.KeywordDecimal,
                "default" => BindingTokenType.KeywordDefault,
                "delegate" => BindingTokenType.KeywordDelegate,
                "do" => BindingTokenType.KeywordDo,
                "double" => BindingTokenType.KeywordDouble,
                "else" => BindingTokenType.KeywordElse,
                "enum" => BindingTokenType.KeywordEnum,
                "event" => BindingTokenType.KeywordEvent,
                "explicit" => BindingTokenType.KeywordExplicit,
                "extern" => BindingTokenType.KeywordExtern,
                "false" => BindingTokenType.KeywordFalse,
                "finally" => BindingTokenType.KeywordFinally,
                "fixed" => BindingTokenType.KeywordFixed,
                "float" => BindingTokenType.KeywordFloat,
                "for" => BindingTokenType.KeywordFor,
                "foreach" => BindingTokenType.KeywordForeach,
                "goto" => BindingTokenType.KeywordGoto,
                "if" => BindingTokenType.KeywordIf,
                "implicit" => BindingTokenType.KeywordImplicit,
                "in" => BindingTokenType.KeywordIn,
                "int" => BindingTokenType.KeywordInt,
                "interface" => BindingTokenType.KeywordInterface,
                "internal" => BindingTokenType.KeywordInternal,
                "is" => BindingTokenType.KeywordIs,
                "lock" => BindingTokenType.KeywordLock,
                "long" => BindingTokenType.KeywordLong,
                "namespace" => BindingTokenType.KeywordNamespace,
                "new" => BindingTokenType.KeywordNew,
                "null" => BindingTokenType.KeywordNull,
                "object" => BindingTokenType.KeywordObject,
                "operator" => BindingTokenType.KeywordOperator,
                "out" => BindingTokenType.KeywordOut,
                "override" => BindingTokenType.KeywordOverride,
                "params" => BindingTokenType.KeywordParams,
                "private" => BindingTokenType.KeywordPrivate,
                "protected" => BindingTokenType.KeywordProtected,
                "public" => BindingTokenType.KeywordPublic,
                "readonly" => BindingTokenType.KeywordReadonly,
                "ref" => BindingTokenType.KeywordRef,
                "return" => BindingTokenType.KeywordReturn,
                "sbyte" => BindingTokenType.KeywordSbyte,
                "sealed" => BindingTokenType.KeywordSealed,
                "short" => BindingTokenType.KeywordShort,
                "sizeof" => BindingTokenType.KeywordSizeof,
                "stackalloc" => BindingTokenType.KeywordStackalloc,
                "static" => BindingTokenType.KeywordStatic,
                "string" => BindingTokenType.KeywordString,
                "struct" => BindingTokenType.KeywordStruct,
                "switch" => BindingTokenType.KeywordSwitch,
                "this" => BindingTokenType.KeywordThis,
                "throw" => BindingTokenType.KeywordThrow,
                "true" => BindingTokenType.KeywordTrue,
                "try" => BindingTokenType.KeywordTry,
                "typeof" => BindingTokenType.KeywordTypeof,
                "uint" => BindingTokenType.KeywordUint,
                "ulong" => BindingTokenType.KeywordUlong,
                "unchecked" => BindingTokenType.KeywordUnchecked,
                "unsafe" => BindingTokenType.KeywordUnsafe,
                "ushort" => BindingTokenType.KeywordUshort,
                "using" => BindingTokenType.KeywordUsing,
                "virtual" => BindingTokenType.KeywordVirtual,
                "void" => BindingTokenType.KeywordVoid,
                "volatile" => BindingTokenType.KeywordVolatile,
                "while" => BindingTokenType.KeywordWhile,
                _ => BindingTokenType.Identifier
            };


        internal void EnsureUnsupportedOperator(BindingTokenType preferredOperatorToken)
        {
            if (IsUnaryOperator(Peek()) || !IsOperator(Peek()))
            {
                CreateToken(preferredOperatorToken);
            }
            else
            {
                while (IsOperator(Peek()))
                {
                    Read();
                }
                CreateToken(BindingTokenType.UnsupportedOperator);
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
