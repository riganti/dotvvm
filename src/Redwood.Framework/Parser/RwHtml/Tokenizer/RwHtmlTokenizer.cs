using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Parser.RwHtml.Tokenizer
{
    /// <summary>
    /// Reads the RWHTML content and returns tokens.
    /// </summary>
    public class RwHtmlTokenizer : TokenizerBase<RwHtmlToken, RwHtmlTokenType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RwHtmlTokenizer"/> class.
        /// </summary>
        public RwHtmlTokenizer()
        {
        }

        /// <summary>
        /// Gets the type of the text token.
        /// </summary>
        protected override RwHtmlTokenType TextTokenType
        {
            get { return RwHtmlTokenType.Text; }
        }

        /// <summary>
        /// Gets the type of the white space token.
        /// </summary>
        protected override RwHtmlTokenType WhiteSpaceTokenType
        {
            get { return RwHtmlTokenType.WhiteSpace; }
        }



        /// <summary>
        /// Tokenizes the input.
        /// </summary>
        protected override void TokenizeCore()
        {
            // read directives
            while (ReadDirective()) { }

            // read the body
            while (Peek() != NullChar)
            {
                if (Peek() == '<')
                {
                    if (DistanceSinceLastToken > 0)
                    {
                        CreateToken(RwHtmlTokenType.Text);
                    }

                    // HTML element
                    ReadElement();
                }
                else if (Peek() == '{')
                {
                    // possible binding
                    Read();
                    if (Peek() == '{')
                    {
                        if (DistanceSinceLastToken > 1)
                        {
                            CreateToken(RwHtmlTokenType.Text, 1);
                        }

                        // it really is a binding 
                        ReadBinding(true);
                    }
                }
                else
                {
                    // text content
                    Read();
                }
            }

            // treat remaining content as text
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(RwHtmlTokenType.Text);
            }
        }




        /// <summary>
        /// Reads the directive.
        /// </summary>
        private bool ReadDirective()
        {
            SkipWhitespace();

            if (Peek() == '@')
            {
                // the directive started
                Read();
                CreateToken(RwHtmlTokenType.DirectiveStart);

                // identifier
                if (!ReadIdentifier(RwHtmlTokenType.DirectiveName, '\r', '\n'))
                {
                    CreateToken(RwHtmlTokenType.DirectiveName, errorMessage: "TODO");
                }
                SkipWhitespace(false);
                
                // whitespace
                if (LastToken.Type != RwHtmlTokenType.WhiteSpace)
                {
                    CreateToken(RwHtmlTokenType.WhiteSpace, errorMessage: "TODO");
                }

                // directive value
                if (Peek() == '\r' || Peek() == '\n' || Peek() == NullChar)
                {
                    CreateToken(RwHtmlTokenType.DirectiveValue, errorMessage: "TODO");
                    SkipWhitespace();
                }
                else
                {
                    ReadTextUntilNewLine(RwHtmlTokenType.DirectiveValue);
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        static readonly HashSet<char> EnabledIdentifierChars = new HashSet<char>()
        {
            ':', '_', '-', '.'
        };
        /// <summary>
        /// Reads the identifier.
        /// </summary>
        private bool ReadIdentifier(RwHtmlTokenType tokenType, params char[] stopChars)
        {
            // read first character
            if ((!char.IsLetter(Peek()) && Peek() != '_') || stopChars.Contains(Peek()))
                return false;

            // read identifier
            while ((Char.IsLetterOrDigit(Peek()) || EnabledIdentifierChars.Contains(Peek())) && !stopChars.Contains(Peek()))
            {
                Read();
            }
            CreateToken(tokenType);
            return true;
        }


        /// <summary>
        /// Reads the element.
        /// </summary>
        private bool ReadElement()
        {
            var isClosingTag = false;

            // open tag brace
            Assert(Peek() == '<');
            Read();

            if (Peek() == '!' || Peek() == '?')
            {
                ReadHtmlSpecial(true);
                return false;
            }

            CreateToken(RwHtmlTokenType.OpenTag);

            if (Peek() == '/')
            {
                // it is a closing tag
                Read();
                CreateToken(RwHtmlTokenType.Slash);
                isClosingTag = true;
            }

            // read tag name
            if (!ReadTagOrAttributeName(isAttributeName: false))
            {
                return false;
            }

            // read tag attributes
            SkipWhitespace();
            if (!isClosingTag)
            {
                while (Peek() != '/' && Peek() != '>')
                {
                    if (!ReadAttribute())
                    {
                        return false;
                    }
                }
            }

            if (Peek() == '/' && !isClosingTag)
            {
                // self closing tag
                Read();
                CreateToken(RwHtmlTokenType.Slash);
            }
            if (Peek() != '>')
            {
                // tag is not closed
                ReportError(Parser_RwHtml.Element_TagNotClosed);
                ReadTextUntilNewLineOrTag();
                return false;
            }

            Read();
            CreateToken(RwHtmlTokenType.CloseTag);
            return true;
        }

        public void ReadHtmlSpecial(bool openBraceConsumed = false)
        {
            if (!openBraceConsumed)
            {
                Assert(Peek() == '<');
                Read();
            }
            Assert(Peek() == '!' || Peek() == '?');
            Read();
            
            var s = ReadOneOf("![CDATA[", "!--", "!DOCTYPE", "?");
            if (s == "![CDATA[") 
            {
                // CDATA section
                CreateToken(RwHtmlTokenType.OpenCData);
                ReadTextUntil("]]>");
                CreateToken(RwHtmlTokenType.CDataBody, 3);
                CreateToken(RwHtmlTokenType.CloseCData);
            }
            else if (s == "!--") 
            {
                // comment
                CreateToken(RwHtmlTokenType.OpenComment);
                ReadTextUntil("-->");
                CreateToken(RwHtmlTokenType.CommentBody, 3);
                CreateToken(RwHtmlTokenType.CloseComment);
            }
            else if (s == "!DOCTYPE")
            {
                CreateToken(RwHtmlTokenType.OpenDoctype);
                ReadTextUntil(">");
                CreateToken(RwHtmlTokenType.DoctypeBody, 1);
                CreateToken(RwHtmlTokenType.CloseDoctype);
            }
            else if (s == "?")
            {
                CreateToken(RwHtmlTokenType.OpenXmlProcessingInstruction);
                ReadTextUntil("?>");
                CreateToken(RwHtmlTokenType.XmlProcessingInstructionBody, 2);
                CreateToken(RwHtmlTokenType.CloseXmlProcessingInstruction);
            }
            else
            {
                ReportError("Element is not supported");
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
            if (!ReadIdentifier(RwHtmlTokenType.Text, ':', '/', '>'))
            {
                ReportError(isAttributeName ? Parser_RwHtml.Element_IdentifierExpectedAfterTagOpenBrace : Parser_RwHtml.Element_AttributeNameExpected);
                ReadTextUntilNewLineOrTag();
                return false;
            }
            SkipWhitespace();

            if (Peek() == ':')
            {
                Read();
                CreateToken(RwHtmlTokenType.Colon);

                if (!ReadIdentifier(RwHtmlTokenType.Text, '/', '>'))
                {
                    ReportError(Parser_RwHtml.Element_IdentifierExpectedAfterColonInTagName);
                    ReadTextUntilNewLineOrTag();
                    return false;
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
                CreateToken(RwHtmlTokenType.Equals);
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
                    ReadIdentifier(RwHtmlTokenType.Text, '/', '>');
                    SkipWhitespace();
                }
            }
            return true;
        }

        /// <summary>
        /// Reads the quoted attribute value.
        /// </summary>
        private bool ReadQuotedAttributeValue()
        {
            // read the beginning quotes
            var quotes = Peek();
            Read();
            CreateToken(quotes == '\'' ? RwHtmlTokenType.SingleQuote : RwHtmlTokenType.DoubleQuote);

            // read value
            if (Peek() == '{')
            {
                // binding
                if (!ReadBinding(false))
                {
                    return false;
                }
            }
            else
            {
                // read text 
                while (Peek() != quotes)
                {
                    var ch = Read();
                    if (ch == NullChar)
                    {
                        ReportError(Parser_RwHtml.UnexpectedEndOfInput);
                        return false;
                    }
                    if (ch == '>')
                    {
                        ReportError(Parser_RwHtml.Attribute_ValueMustNotContainTagCloseBrace);
                        return false;
                    }
                }
                if (DistanceSinceLastToken > 0)
                {
                    CreateToken(RwHtmlTokenType.Text);
                }
            }

            // read the ending quotes
            Read();
            CreateToken(quotes == '\'' ? RwHtmlTokenType.SingleQuote : RwHtmlTokenType.DoubleQuote);
            SkipWhitespace();
            return true;
        }


        /// <summary>
        /// Reads the binding.
        /// </summary>
        private bool ReadBinding(bool doubleCloseBrace)
        {
            // read open brace
            Assert(Peek() == '{');
            Read();
            if (!doubleCloseBrace && Peek() == '{')
            {
                doubleCloseBrace = true;
                Read();
            }
            CreateToken(RwHtmlTokenType.OpenBinding);

            // read binding name
            if (!ReadIdentifier(RwHtmlTokenType.Text, ':'))
            {
                ReportError(Parser_RwHtml.Binding_MustStartWithIdentifier);
                return false;
            }
            SkipWhitespace();

            // colon
            if (Peek() != ':')
            {
                ReportError(Parser_RwHtml.Binding_ColonAfterIdentifier);
                return false;
            }
            Read();
            CreateToken(RwHtmlTokenType.Colon);
            SkipWhitespace();

            // binding value
            while (Peek() != '}')
            {
                var ch = Read();
                if (ch == NullChar)
                {
                    ReportError(Parser_RwHtml.UnexpectedEndOfInput);
                    return false;
                }
            }
            CreateToken(RwHtmlTokenType.Text);

            // close brace
            Read();
            if (doubleCloseBrace)
            {
                if (Peek() != '}')
                {
                    ReportError(Parser_RwHtml.Binding_DoubleCloseBraceRequired);
                }
                else
                {
                    Read();
                }
            }
            CreateToken(RwHtmlTokenType.CloseBinding);
            return true;
        }


        /// <summary>
        /// Reads the text until new line or tag.
        /// </summary>
        private void ReadTextUntilNewLineOrTag()
        {
            ReadTextUntilNewLine(TextTokenType, '<', '>');
        }
    }
}
