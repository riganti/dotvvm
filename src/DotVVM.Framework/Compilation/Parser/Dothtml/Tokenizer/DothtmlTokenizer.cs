using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer
{
    /// <summary>
    /// Reads the Dothtml content and returns tokens.
    /// </summary>
    public class DothtmlTokenizer : TokenizerBase<DothtmlToken, DothtmlTokenType>
    {

        private static readonly HashSet<char> AllowedAttributeFirstChars = new HashSet<char>()
        {
            '_', '[', '('
        };

        private static readonly HashSet<char> AllowedAttributeChars = new HashSet<char>()
        {
            ':', '_', '-', '.', '[', ']', '(', ')'
        };

        private static readonly HashSet<char> AllowedIdentifierChars = new HashSet<char>()
        {
            ':', '_', '-', '.'
        };


        public override void Tokenize(string sourceText)
        {
            TokenizeInternal(sourceText, () => { ReadDocument(); return true; });
        }

        public bool TokenizeBinding(string sourceText, bool usesDoubleBraces = false)
        {
            return TokenizeInternal(sourceText, () => {
                ReadBinding(usesDoubleBraces);
                //Finished?
                Assert(Peek() == NullChar);
                //Properly opened/closed
                Assert((Tokens.FirstOrDefault()?.Length ?? 0) > 0);
                Assert((Tokens.LastOrDefault()?.Length ?? 0) > 0);
                return true;
            });
        }

        /// <summary>
        /// Gets the type of the text token.
        /// </summary>
        protected override DothtmlTokenType TextTokenType
        {
            get { return DothtmlTokenType.Text; }
        }

        /// <summary>
        /// Gets the type of the white space token.
        /// </summary>
        protected override DothtmlTokenType WhiteSpaceTokenType
        {
            get { return DothtmlTokenType.WhiteSpace; }
        }

        /// <summary>
        /// Tokenizes whole dothtml document from start to end.
        /// </summary>
        private void ReadDocument()
        {
            var directivesAllowed = true;

            // read the body
            while (Peek() != NullChar)
            {
                if (Peek() == '@' && directivesAllowed)
                {
                    ReadDirective();
                }
                else if (Peek() == '<')
                {
                    if (DistanceSinceLastToken > 0)
                    {
                        CreateToken(DothtmlTokenType.Text);
                    }

                    // HTML element
                    var elementType = ReadElement();
                    if (elementType != ReadElementType.Comment && elementType != ReadElementType.ServerComment)
                    {
                        directivesAllowed = false;
                    }
                }
                else if (Peek() == '{')
                {
                    // possible binding
                    Read();
                    if (Peek() == '{')
                    {
                        if (DistanceSinceLastToken > 1)
                        {
                            CreateToken(DothtmlTokenType.Text, 1);
                        }

                        // it really is a binding
                        ReadBinding(true);
                    }

                    directivesAllowed = false;
                }
                else
                {
                    if (!char.IsWhiteSpace(Peek()))
                    {
                        directivesAllowed = false;
                    }

                    // text content
                    Read();
                }
            }

            // treat remaining content as text
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(DothtmlTokenType.Text);
            }
        }

        private void ReadDirective()
        {
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(DothtmlTokenType.WhiteSpace);
            }

            // the directive started
            Read();
            CreateToken(DothtmlTokenType.DirectiveStart);

            // identifier
            if (!ReadIdentifier(DothtmlTokenType.DirectiveName, '\r', '\n'))
            {
                CreateToken(DothtmlTokenType.DirectiveName, errorProvider: t => CreateTokenError(t, DothtmlTokenType.DirectiveStart, DothtmlTokenizerErrors.DirectiveNameExpected));
            }
            SkipWhitespace(false);

            // whitespace
            if (LastToken.Type != DothtmlTokenType.WhiteSpace)
            {
                CreateToken(DothtmlTokenType.WhiteSpace, errorProvider: t => CreateTokenError(t, DothtmlTokenType.DirectiveStart, DothtmlTokenizerErrors.DirectiveValueExpected));
            }

            // directive value
            if (Peek() == '\r' || Peek() == '\n' || Peek() == NullChar)
            {
                CreateToken(DothtmlTokenType.DirectiveValue, errorProvider: t => CreateTokenError(t, DothtmlTokenType.DirectiveStart, DothtmlTokenizerErrors.DirectiveValueExpected));
                SkipWhitespace();
            }
            else
            {
                ReadTextUntilNewLine(DothtmlTokenType.DirectiveValue);
            }
        }

        /// <summary>
        /// Reads the identifier.
        /// </summary>
        private bool ReadIdentifier(DothtmlTokenType tokenType, params char[] stopChars)
        {
            // read first character
            if ((!char.IsLetter(Peek()) && Peek() != '_') || stopChars.Contains(Peek()))
                return false;

            // read identifier
            while ((Char.IsLetterOrDigit(Peek()) || AllowedIdentifierChars.Contains(Peek())) && !stopChars.Contains(Peek()))
            {
                Read();
            }
            CreateToken(tokenType);
            return true;
        }

        /// <summary>
        /// Reads the attribute name.
        /// </summary>
        private bool ReadAttributeName(DothtmlTokenType tokenType, params char[] stopChars)
        {
            // read first character
            if ((!char.IsLetter(Peek()) && !AllowedAttributeFirstChars.Contains(Peek())) || stopChars.Contains(Peek()))
                return false;

            // read identifier
            while ((Char.IsLetterOrDigit(Peek()) || AllowedAttributeChars.Contains(Peek())) && !stopChars.Contains(Peek()))
            {
                Read();
            }
            CreateToken(tokenType);
            return true;
        }

        /// <summary>
        /// Reads the element.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private ReadElementType ReadElement(bool wasOpenBraceRead = false)
        {
            var isClosingTag = false;

            if (!wasOpenBraceRead)
            {
                // open tag brace
                Assert(Peek() == '<');
                Read();
            }

            if (Peek() == '!' || Peek() == '?' || Peek() == '%')
            {
                return ReadHtmlSpecial(true);
            }

            if (!char.IsLetterOrDigit(Peek()) && Peek() != '/' && Peek() != ':')
            {
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, "'<' char is not allowed in normal text"));
                return ReadElementType.Error;
            }

            CreateToken(DothtmlTokenType.OpenTag);

            if (Peek() == '/')
            {
                // it is a closing tag
                Read();
                CreateToken(DothtmlTokenType.Slash);
                isClosingTag = true;
            }

            // read tag name
            if (!ReadTagOrAttributeName(isAttributeName: false))
            {
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.TagNameExpected));
                CreateToken(DothtmlTokenType.CloseTag, errorProvider: t => CreateTokenError());
                return ReadElementType.Error;
            }

            // read tag attributes
            SkipWhitespace();
            if (!isClosingTag)
            {
                while (Peek() != '/' && Peek() != '>')
                {
                    if (Peek() == '<')
                    {
                        // comment inside element
                        Read();
                        if (Peek() == '!' || Peek() == '%')
                        {
                            var c = ReadOneOf("!--", "%--");
                            if (c == null) CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, ""));
                            else if (c == "!--") ReadComment();
                            else if (c == "%--") ReadServerComment();
                            else throw new Exception();
                            SkipWhitespace();
                        }
                        else
                        {
                            CreateToken(DothtmlTokenType.CloseTag, charsFromEndToSkip: 1, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.InvalidCharactersInTag, isCritical: true));
                            ReadElement(wasOpenBraceRead: true);
                            return ReadElementType.Error;
                        }
                    }
                    else
                    if (!ReadAttribute())
                    {
                        CreateToken(DothtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.InvalidCharactersInTag, isCritical: true));
                        return ReadElementType.Error;
                    }
                }
            }

            if (Peek() == '/' && !isClosingTag)
            {
                // self closing tag
                Read();
                CreateToken(DothtmlTokenType.Slash);
            }
            if (Peek() != '>')
            {
                // tag is not closed
                CreateToken(DothtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.TagNotClosed));
                return ReadElementType.Error;
            }

            Read();
            CreateToken(DothtmlTokenType.CloseTag);
            return ReadElementType.ValidTag;
        }

        public enum ReadElementType
        {
            Error,
            ValidTag,
            CData,
            Comment,
            Doctype,
            XmlProcessingInstruction,
            ServerComment
        }

        public ReadElementType ReadHtmlSpecial(bool openBraceConsumed = false)
        {
            var s = ReadOneOf("![CDATA[", "!--", "!DOCTYPE", "?", "%--");
            if (s == "![CDATA[")
            {
                return ReadCData();
            }
            else if (s == "!--")
            {
                return ReadComment();
            }
            else if (s == "%--")
            {
                return ReadServerComment();
            }
            else if (s == "!DOCTYPE")
            {
                return ReadDoctype();
            }
            else if (s == "?")
            {
                return ReadXmlPI();
            }
            return ReadElementType.Error;
        }

        private ReadElementType ReadCData()
        {
            CreateToken(DothtmlTokenType.OpenCData);
            if (ReadTextUntil(DothtmlTokenType.CDataBody, "]]>", false))
            {
                CreateToken(DothtmlTokenType.CloseCData);
                return ReadElementType.CData;
            }
            else
            {
                CreateToken(DothtmlTokenType.CDataBody, errorProvider: t => CreateTokenError());
                CreateToken(DothtmlTokenType.CloseCData, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenCData, DothtmlTokenizerErrors.CDataNotClosed));
                return ReadElementType.Error;
            }
        }

        private ReadElementType ReadComment()
        {
            CreateToken(DothtmlTokenType.OpenComment);
            if (ReadTextUntil(DothtmlTokenType.CommentBody, "-->", false))
            {
                CreateToken(DothtmlTokenType.CloseComment);
                return ReadElementType.Comment;
            }
            else
            {
                CreateToken(DothtmlTokenType.CommentBody);
                CreateToken(DothtmlTokenType.CloseComment, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenComment, DothtmlTokenizerErrors.CommentNotClosed));
                return ReadElementType.Error;
            }
        }

        private ReadElementType ReadServerComment()
        {
            CreateToken(DothtmlTokenType.OpenServerComment);
            if (ReadTextUntil(DothtmlTokenType.CommentBody, "--%>", false, nestString: "<%--"))
            {
                CreateToken(DothtmlTokenType.CloseComment);
                return ReadElementType.ServerComment;
            }
            else
            {
                CreateToken(DothtmlTokenType.CommentBody);
                CreateToken(DothtmlTokenType.CloseComment, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenComment, DothtmlTokenizerErrors.CommentNotClosed));
                return ReadElementType.Error;
            }
        }

        private ReadElementType ReadDoctype()
        {
            CreateToken(DothtmlTokenType.OpenDoctype);
            if (ReadTextUntil(DothtmlTokenType.DoctypeBody, ">", true))
            {
                CreateToken(DothtmlTokenType.CloseDoctype);
                return ReadElementType.Doctype;
            }
            else
            {
                CreateToken(DothtmlTokenType.DoctypeBody, errorProvider: t => CreateTokenError());
                CreateToken(DothtmlTokenType.CloseDoctype, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenDoctype, DothtmlTokenizerErrors.DoctypeNotClosed));
                return ReadElementType.Error;
            }
        }

        private ReadElementType ReadXmlPI()
        {
            CreateToken(DothtmlTokenType.OpenXmlProcessingInstruction);
            if (ReadTextUntil(DothtmlTokenType.XmlProcessingInstructionBody, "?>", true))
            {
                CreateToken(DothtmlTokenType.CloseXmlProcessingInstruction);
                return ReadElementType.XmlProcessingInstruction;
            }
            else
            {
                CreateToken(DothtmlTokenType.XmlProcessingInstructionBody, errorProvider: t => CreateTokenError());
                CreateToken(DothtmlTokenType.CloseXmlProcessingInstruction, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenXmlProcessingInstruction, DothtmlTokenizerErrors.XmlProcessingInstructionNotClosed));
                return ReadElementType.Error;
            }
        }

        private void Assert(bool expression)
        {
            if (!expression)
            {
                throw new Exception("Assertion failed!");
            }
        }

        /// <summary>
        /// Reads the name of the tag or attribute.
        /// </summary>
        private bool ReadTagOrAttributeName(bool isAttributeName)
        {
            var readIdentifierFunc = isAttributeName ? (Func<DothtmlTokenType, char[], bool>)ReadAttributeName : (Func<DothtmlTokenType, char[], bool>)ReadIdentifier;

            if (Peek() != ':')
            {
                // read the identifier
                if (!readIdentifierFunc(DothtmlTokenType.Text, new[] { ':' }))
                {
                    return false;
                }
            }
            else
            {
                // missing tag prefix
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.MissingTagPrefix));
            }

            if (Peek() == ':')
            {
                Read();
                CreateToken(DothtmlTokenType.Colon);

                if (!readIdentifierFunc(DothtmlTokenType.Text, new char[] { }))
                {
                    CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.MissingTagName));
                    return true;
                }
            }

            SkipWhitespace();
            return true;
        }

        /// <summary>
        /// Reads the attribute of a tag.
        /// </summary>
        private bool ReadAttribute()
        {
            // attribute name
            if (!ReadTagOrAttributeName(isAttributeName: true))
            {
                return false;
            }

            if (Peek() == '=')
            {
                // equals sign
                Read();
                CreateToken(DothtmlTokenType.Equals);
                SkipWhitespace();

                // attribute value
                if (Peek() == '\'' || Peek() == '"')
                {
                    // single or double quotes
                    if (!ReadQuotedAttributeValue())
                    {
                        return false;
                    }
                }
                else
                {
                    // unquoted value
                    if (Peek() == '>' || Peek() == '<')
                    {
                        CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.Text, DothtmlTokenizerErrors.MissingAttributeValue));
                        return false;
                    }
                    do
                    {
                        if (Read() == NullChar || Peek() == '<') { CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError()); return false; }
                    } while (!char.IsWhiteSpace(Peek()) && Peek() != '/' && Peek() != '>');
                    CreateToken(DothtmlTokenType.Text);
                    SkipWhitespace();
                }
            }
            //else
            //{
            //    CreateToken(DothtmlTokenType.Equals, errorProvider: t => CreateTokenError());
            //    CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.Text, DothtmlTokenizerErrors.MissingAttributeValue));
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// Reads the quoted attribute value.
        /// </summary>
        private bool ReadQuotedAttributeValue()
        {
            // read the beginning quotes
            var quotes = Peek();
            var quotesToken = quotes == '\'' ? DothtmlTokenType.SingleQuote : DothtmlTokenType.DoubleQuote;
            Read();
            CreateToken(quotesToken);

            // read value
            if (Peek() == '{')
            {
                // binding
                ReadBinding(false);
            }
            else
            {
                // read text
                while (Peek() != quotes)
                {
                    var ch = Peek();
                    if (ch == NullChar || ch == '<' || ch == '>')
                    {
                        CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError());
                        CreateToken(quotesToken, errorProvider: t => CreateTokenError(t, quotesToken, DothtmlTokenizerErrors.AttributeValueNotClosed));
                        return false;
                    }
                    Read();
                }
                CreateToken(DothtmlTokenType.Text);
            }

            // read the ending quotes
            if (Peek() != quotes)
            {
                CreateToken(quotesToken, errorProvider: t => CreateTokenError(t, quotesToken, DothtmlTokenizerErrors.AttributeValueNotClosed));
            }
            else
            {
                Read();
                CreateToken(quotesToken);
                SkipWhitespace();
            }
            return true;
        }

        /// <summary>
        /// Reads the binding.
        /// </summary>
        private void ReadBinding(bool doubleCloseBrace)
        {
            // read open brace
            Assert(Peek() == '{');
            Read();
            if (!doubleCloseBrace && Peek() == '{')
            {
                doubleCloseBrace = true;
                Read();
            }
            CreateToken(DothtmlTokenType.OpenBinding);
            SkipWhitespace();

            // read binding name
            if (!ReadIdentifier(DothtmlTokenType.Text, ':'))
            {
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                SkipWhitespace();
            }

            // colon
            if (Peek() != ':')
            {
                CreateToken(DothtmlTokenType.Colon, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                Read();
                CreateToken(DothtmlTokenType.Colon);
                SkipWhitespace();
            }

            // binding value
            if (Peek() == '}')
            {
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                char ch;
                while ((ch = Peek()) != '}')
                {
                    if (ch == '\'' || ch == '"')
                    {
                        // string literal - ignore curly braces inside
                        string errorMessage;
                        BindingTokenizer.ReadStringLiteral(Peek, Read, out errorMessage);
                    }
                    else if (ch == NullChar)
                    {
                        CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError());
                        CreateToken(DothtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.BindingNotClosed));
                        return;
                    }
                    else
                    {
                        Read();
                    }
                }
                CreateToken(DothtmlTokenType.Text);
            }

            // close brace
            if (Peek() != '}')
            {
                CreateToken(DothtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.BindingNotClosed));
                return;
            }
            Read();

            if (doubleCloseBrace)
            {
                if (Peek() != '}')
                {
                    CreateToken(DothtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.DoubleBraceBindingNotClosed));
                    return;
                }
                Read();
            }
            CreateToken(DothtmlTokenType.CloseBinding);
        }

        protected override DothtmlToken NewToken()
        {
            return new DothtmlToken();
        }
    }
}
