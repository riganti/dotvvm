using System;
using System.Collections.Generic;
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
                    CreateToken(RwHtmlTokenType.DirectiveName, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.DirectiveStart, RwHtmlTokenizerErrors.DirectiveNameExpected));
                }
                SkipWhitespace(false);
                
                // whitespace
                if (LastToken.Type != RwHtmlTokenType.WhiteSpace)
                {
                    CreateToken(RwHtmlTokenType.WhiteSpace, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.DirectiveStart, RwHtmlTokenizerErrors.DirectiveValueExpected));
                }

                // directive value
                if (Peek() == '\r' || Peek() == '\n' || Peek() == NullChar)
                {
                    CreateToken(RwHtmlTokenType.DirectiveValue, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.DirectiveStart, RwHtmlTokenizerErrors.DirectiveValueExpected));
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
                return true;
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
                CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError());
                CreateToken(RwHtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenTag, RwHtmlTokenizerErrors.TagNameExpected));
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
                        CreateToken(RwHtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenTag, RwHtmlTokenizerErrors.InvalidCharactersInTag));        
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
                CreateToken(RwHtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenTag, RwHtmlTokenizerErrors.TagNotClosed));
                return false;
            }

            Read();
            CreateToken(RwHtmlTokenType.CloseTag);
            return true;
        }

        public void ReadHtmlSpecial(bool openBraceConsumed = false)
        {
            var s = ReadOneOf("![CDATA[", "!--", "!DOCTYPE", "?");
            if (s == "![CDATA[") 
            {
                // CDATA section
                CreateToken(RwHtmlTokenType.OpenCData);
                if (ReadTextUntil(RwHtmlTokenType.CDataBody, "]]>"))
                {
                    CreateToken(RwHtmlTokenType.CloseCData);
                }
                else
                {
                    CreateToken(RwHtmlTokenType.CDataBody, errorProvider: t => CreateTokenError());
                    CreateToken(RwHtmlTokenType.CloseCData, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenCData, RwHtmlTokenizerErrors.CDataNotClosed));
                }
            }
            else if (s == "!--") 
            {
                // comment
                CreateToken(RwHtmlTokenType.OpenComment);
                if (ReadTextUntil(RwHtmlTokenType.CommentBody, "-->"))
                {
                    CreateToken(RwHtmlTokenType.CloseComment);
                }
                else
                {
                    CreateToken(RwHtmlTokenType.CommentBody, errorProvider: t => CreateTokenError());
                    CreateToken(RwHtmlTokenType.CloseComment, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenComment, RwHtmlTokenizerErrors.CommentNotClosed));
                }
            }
            else if (s == "!DOCTYPE")
            {
                // DOCTYPE
                CreateToken(RwHtmlTokenType.OpenDoctype);
                if (ReadTextUntil(RwHtmlTokenType.DoctypeBody, ">"))
                {
                    CreateToken(RwHtmlTokenType.CloseDoctype);
                }
                else
                {
                    CreateToken(RwHtmlTokenType.DoctypeBody, errorProvider: t => CreateTokenError());
                    CreateToken(RwHtmlTokenType.CloseDoctype, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenDoctype, RwHtmlTokenizerErrors.DoctypeNotClosed));                    
                }
            }
            else if (s == "?")
            {
                // XML processing instruction
                CreateToken(RwHtmlTokenType.OpenXmlProcessingInstruction);
                if (ReadTextUntil(RwHtmlTokenType.XmlProcessingInstructionBody, "?>"))
                {
                    CreateToken(RwHtmlTokenType.CloseXmlProcessingInstruction);
                }
                else
                {
                    CreateToken(RwHtmlTokenType.XmlProcessingInstructionBody, errorProvider: t => CreateTokenError());
                    CreateToken(RwHtmlTokenType.CloseXmlProcessingInstruction, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenXmlProcessingInstruction, RwHtmlTokenizerErrors.XmlProcessingInstructionNotClosed));         
                }
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
            if (Peek() != ':')
            {
                // read the identifier
                if (!ReadIdentifier(RwHtmlTokenType.Text, '=', ':', '/', '>'))
                {
                    return false;
                }
            }
            else
            {
                // missing tag prefix
                CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenTag, RwHtmlTokenizerErrors.MissingTagPrefix));
            }

            if (Peek() == ':')
            {
                Read();
                CreateToken(RwHtmlTokenType.Colon);

                if (!ReadIdentifier(RwHtmlTokenType.Text, '=', '/', '>'))
                {
                    CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenTag, RwHtmlTokenizerErrors.MissingTagName));
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
                    if (!ReadIdentifier(RwHtmlTokenType.Text, '/', '>'))
                    {
                        CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.Text, RwHtmlTokenizerErrors.MissingAttributeValue));        
                    }
                    SkipWhitespace();
                }
            }
            else
            {
                CreateToken(RwHtmlTokenType.Equals, errorProvider: t => CreateTokenError());
                CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.Text, RwHtmlTokenizerErrors.MissingAttributeValue));
                return false;
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
            var quotesToken = quotes == '\'' ? RwHtmlTokenType.SingleQuote : RwHtmlTokenType.DoubleQuote;
            Read();
            CreateToken(quotesToken);

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
                    var ch = Peek();
                    if (ch == NullChar || ch == '<' || ch == '>')
                    {
                        CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError());
                        CreateToken(quotesToken, errorProvider: t => CreateTokenError(t, quotesToken, RwHtmlTokenizerErrors.AttributeValueNotClosed));
                        return false;
                    }
                    Read();
                }
                CreateToken(RwHtmlTokenType.Text);
            }

            // read the ending quotes
            if (Peek() != quotes)
            {
                CreateToken(quotesToken, errorProvider: t => CreateTokenError(t, quotesToken, RwHtmlTokenizerErrors.AttributeValueNotClosed));
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
            SkipWhitespace();

            // read binding name
            if (!ReadIdentifier(RwHtmlTokenType.Text, ':'))
            {
                CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                SkipWhitespace();
            }

            // colon
            if (Peek() != ':')
            {
                CreateToken(RwHtmlTokenType.Colon, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                Read();
                CreateToken(RwHtmlTokenType.Colon);
                SkipWhitespace();
            }

            // binding value
            if (Peek() == '}')
            {
                CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.BindingInvalidFormat));
            }
            else
            {
                while (Peek() != '}')
                {
                    var ch = Peek();
                    if (ch == NullChar)
                    {
                        CreateToken(RwHtmlTokenType.Text, errorProvider: t => CreateTokenError());
                        CreateToken(RwHtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.BindingNotClosed));
                        return true;
                    }
                    Read();
                }
                CreateToken(RwHtmlTokenType.Text);
            }

            // close brace
            if (Peek() != '}')
            {
                CreateToken(RwHtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.BindingNotClosed));
                return true;
            }
            Read();
            
            if (doubleCloseBrace)
            {
                if (Peek() != '}')
                {
                    CreateToken(RwHtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, RwHtmlTokenType.OpenBinding, RwHtmlTokenizerErrors.DoubleBraceBindingNotClosed));
                    return true;
                }
                Read();
            }
            CreateToken(RwHtmlTokenType.CloseBinding);
            return true;
        }
    }
}
