using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Binding.Tokenizer;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Parser.Dothtml.Tokenizer
{
    /// <summary>
    /// Reads the Dothtml content and returns tokens.
    /// </summary>
    public class DothtmlTokenizer : TokenizerBase<DothtmlToken, DothtmlTokenType>
    {

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
                        CreateToken(DothtmlTokenType.Text);
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
                            CreateToken(DothtmlTokenType.Text, 1);
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
                CreateToken(DothtmlTokenType.Text);
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
        private bool ReadIdentifier(DothtmlTokenType tokenType, params char[] stopChars)
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
                CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError());
                CreateToken(DothtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.TagNameExpected));
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
                        CreateToken(DothtmlTokenType.CloseTag, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenTag, DothtmlTokenizerErrors.InvalidCharactersInTag));        
                        return false;
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
                return false;
            }

            Read();
            CreateToken(DothtmlTokenType.CloseTag);
            return true;
        }

        public void ReadHtmlSpecial(bool openBraceConsumed = false)
        {
            var s = ReadOneOf("![CDATA[", "!--", "!DOCTYPE", "?");
            if (s == "![CDATA[") 
            {
                // CDATA section
                CreateToken(DothtmlTokenType.OpenCData);
                if (ReadTextUntil(DothtmlTokenType.CDataBody, "]]>", false))
                {
                    CreateToken(DothtmlTokenType.CloseCData);
                }
                else
                {
                    CreateToken(DothtmlTokenType.CDataBody, errorProvider: t => CreateTokenError());
                    CreateToken(DothtmlTokenType.CloseCData, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenCData, DothtmlTokenizerErrors.CDataNotClosed));
                }
            }
            else if (s == "!--") 
            {
                // comment
                CreateToken(DothtmlTokenType.OpenComment);
                if (ReadTextUntil(DothtmlTokenType.CommentBody, "-->", false))
                {
                    CreateToken(DothtmlTokenType.CloseComment);
                }
                else
                {
                    CreateToken(DothtmlTokenType.CommentBody, errorProvider: t => CreateTokenError());
                    CreateToken(DothtmlTokenType.CloseComment, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenComment, DothtmlTokenizerErrors.CommentNotClosed));
                }
            }
            else if (s == "!DOCTYPE")
            {
                // DOCTYPE
                CreateToken(DothtmlTokenType.OpenDoctype);
                if (ReadTextUntil(DothtmlTokenType.DoctypeBody, ">", true))
                {
                    CreateToken(DothtmlTokenType.CloseDoctype);
                }
                else
                {
                    CreateToken(DothtmlTokenType.DoctypeBody, errorProvider: t => CreateTokenError());
                    CreateToken(DothtmlTokenType.CloseDoctype, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenDoctype, DothtmlTokenizerErrors.DoctypeNotClosed));                    
                }
            }
            else if (s == "?")
            {
                // XML processing instruction
                CreateToken(DothtmlTokenType.OpenXmlProcessingInstruction);
                if (ReadTextUntil(DothtmlTokenType.XmlProcessingInstructionBody, "?>", true))
                {
                    CreateToken(DothtmlTokenType.CloseXmlProcessingInstruction);
                }
                else
                {
                    CreateToken(DothtmlTokenType.XmlProcessingInstructionBody, errorProvider: t => CreateTokenError());
                    CreateToken(DothtmlTokenType.CloseXmlProcessingInstruction, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenXmlProcessingInstruction, DothtmlTokenizerErrors.XmlProcessingInstructionNotClosed));         
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
                if (!ReadIdentifier(DothtmlTokenType.Text, '=', ':', '/', '>'))
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

                if (!ReadIdentifier(DothtmlTokenType.Text, '=', '/', '>'))
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
                    if (!ReadIdentifier(DothtmlTokenType.Text, '/', '>'))
                    {
                        CreateToken(DothtmlTokenType.Text, errorProvider: t => CreateTokenError(t, DothtmlTokenType.Text, DothtmlTokenizerErrors.MissingAttributeValue));        
                    }
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
                        return true;
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
                return true;
            }
            Read();
            
            if (doubleCloseBrace)
            {
                if (Peek() != '}')
                {
                    CreateToken(DothtmlTokenType.CloseBinding, errorProvider: t => CreateTokenError(t, DothtmlTokenType.OpenBinding, DothtmlTokenizerErrors.DoubleBraceBindingNotClosed));
                    return true;
                }
                Read();
            }
            CreateToken(DothtmlTokenType.CloseBinding);
            return true;
        }
    }
}
