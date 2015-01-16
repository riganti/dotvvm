using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.Framework.Resources;
using System.Text;
using System.Net;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    /// <summary>
    /// Parses the results of the RwHtmlTokenizer into a tree structure.
    /// </summary>
    public class RwHtmlParser
    {
        private readonly List<RwHtmlToken> tokens;
        private readonly string fileName;
        private Stack<RwHtmlNodeWithContent> elementHierarchy;

        private int CurrentIndex { get; set; }


        private List<RwHtmlNode> CurrentElementContent
        {
            get { return elementHierarchy.Peek().Content; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RwHtmlParser"/> class.
        /// </summary>
        public RwHtmlParser(List<RwHtmlToken> tokens, string fileName)
        {
            this.tokens = tokens;
            this.fileName = fileName;
        }


        /// <summary>
        /// Parses the token stream and gets the node.
        /// </summary>
        public RwHtmlRootNode Parse()
        {
            CurrentIndex = 0;
            elementHierarchy = new Stack<RwHtmlNodeWithContent>();

            // read file
            var root = new RwHtmlRootNode();
            elementHierarchy.Push(root);
            SkipWhitespace();

            // read directives
            while (Peek() != null && Peek().Type == RwHtmlTokenType.DirectiveStart)
            {
                root.Directives.Add(ReadDirective());
            }

            SkipWhitespace();
            if(Peek().Type == RwHtmlTokenType.OpenDoctype)
            {
                root.Directives.Add(ReadDoctype());
            }

            // read content
            while (Peek() != null)
            {
                if (Peek().Type == RwHtmlTokenType.OpenTag)
                {
                    // element - check element hierarchy
                    var element = ReadElement();
                    if (!element.IsSelfClosingTag)
                    {
                        if (!element.IsClosingTag)
                        {
                            // open tag
                            CurrentElementContent.Add(element);
                            elementHierarchy.Push(element);
                        }
                        else
                        {
                            // close tag
                            var beginTagName = ((RwHtmlElementNode)elementHierarchy.Peek()).FullTagName;
                            if (elementHierarchy.Count == 1 || beginTagName != element.FullTagName)
                            {
                                // TODO: try to recover on tag crossing etc.
                                throw new ParserException(string.Format(Parser_RwHtml.Parser_ClosingTagHasNoMatchingOpenTag, beginTagName), fileName, Peek().LineNumber, Peek().ColumnNumber);
                            }
                            elementHierarchy.Pop();
                        }
                    }
                    else
                    {
                        // self closing tag
                        CurrentElementContent.Add(element);
                    }
                }
                else if (Peek().Type == RwHtmlTokenType.OpenBinding)
                {
                    // binding
                    CurrentElementContent.Add(ReadBinding());
                }
                else if (Peek().Type == RwHtmlTokenType.OpenComment)
                {
                    CurrentElementContent.Add(ReadComment());
                }
                else if (Peek().Type == RwHtmlTokenType.OpenCdata)
                {
                    CurrentElementContent.Add(ReadCdata());
                }
                else
                {
                    // text
                    CurrentElementContent.Add(new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() } });
                    Read();
                }
            }

            // check element hierarchy
            if (elementHierarchy.Count > 1)
            {
                throw new ParserException(Parser_RwHtml.UnexpectedEndOfInput);
            }

            return root;
        }

        public RwHtmlLiteralNode ReadComment()
        {
            Assert(RwHtmlTokenType.OpenComment);
            Read();
            var node = new RwHtmlLiteralNode();
            var content = new StringBuilder("<!--");
            while (Peek().Type != RwHtmlTokenType.CloseTag)
            {
                if (Peek().Type == RwHtmlTokenType.Text || Peek().Type == RwHtmlTokenType.WhiteSpace)
                {
                    content.Append(Peek().Text);
                    node.Tokens.Add(Peek());
                    Read();
                }
                else throw new ParserException("");
            }
            Read();
            content.Append("-->");
            node.Value = content.ToString();
            return node;
        }

        public RwHtmlLiteralNode ReadCdata()
        {
            Assert(RwHtmlTokenType.OpenCdata);
            Read();
            var node = new RwHtmlLiteralNode();
            var content = new StringBuilder();
            while (Peek().Type != RwHtmlTokenType.CloseTag)
            {
                if (Peek().Type == RwHtmlTokenType.Text || Peek().Type == RwHtmlTokenType.WhiteSpace)
                {
                    content.Append(WebUtility.HtmlEncode(Peek().Text));
                    node.Tokens.Add(Peek());
                    Read();
                }
                else throw new ParserException("");
            }
            Read();
            node.Value = content.ToString();
            return node;
        }

        /// <summary>
        /// Reads the element.
        /// </summary>
        private RwHtmlElementNode ReadElement()
        {
            var startIndex = CurrentIndex;
            var node = new RwHtmlElementNode();

            Assert(RwHtmlTokenType.OpenTag);
            Read();
            SkipWhitespace();

            if (Peek().Type == RwHtmlTokenType.Slash)
            {
                Read();
                SkipWhitespace();
                node.IsClosingTag = true;
            }

            // element name
            Assert(RwHtmlTokenType.Text);
            node.TagName = Read().Text;
            if (Peek().Type == RwHtmlTokenType.Colon)
            {
                Read();
                node.TagPrefix = node.TagName;
                Assert(RwHtmlTokenType.Text);
                node.TagName = Read().Text;
            }
            SkipWhitespace();

            // attributes
            if (!node.IsClosingTag)
            {
                while (Peek().Type == RwHtmlTokenType.Text)
                {
                    node.Attributes.Add(ReadAttribute());
                    SkipWhitespace();
                }

                if (Peek().Type == RwHtmlTokenType.Slash)
                {
                    Read();
                    SkipWhitespace();
                    node.IsSelfClosingTag = true;
                }
            }

            Assert(RwHtmlTokenType.CloseTag);
            Read();
            SkipWhitespace();

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        /// <summary>
        /// Reads the attribute.
        /// </summary>
        private RwHtmlAttributeNode ReadAttribute()
        {
            var startIndex = CurrentIndex;
            var attribute = new RwHtmlAttributeNode();

            // attribute name
            Assert(RwHtmlTokenType.Text);
            attribute.Name = Read().Text;
            if (Peek().Type == RwHtmlTokenType.Colon)
            {
                Read();
                attribute.Prefix = attribute.Name;
                Assert(RwHtmlTokenType.Text);
                attribute.Name = Read().Text;
            }
            SkipWhitespace();

            if (Peek().Type == RwHtmlTokenType.Equals)
            {
                Read();
                SkipWhitespace();

                // attribute value
                if (Peek().Type == RwHtmlTokenType.SingleQuote || Peek().Type == RwHtmlTokenType.DoubleQuote)
                {
                    var quote = Peek().Type;
                    Read();

                    if (Peek().Type == RwHtmlTokenType.OpenBinding)
                    {
                        attribute.Literal = ReadBinding();
                    }
                    else
                    {
                        Assert(RwHtmlTokenType.Text);
                        attribute.Literal = new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() } };
                        Read();
                    }

                    Assert(quote);
                    Read();
                }
                else
                {
                    Assert(RwHtmlTokenType.Text);
                    attribute.Literal = new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() } };
                    Read();
                }
                SkipWhitespace();
            }

            attribute.Tokens.AddRange(GetTokensFrom(startIndex));
            return attribute;
        }

        /// <summary>
        /// Reads the binding.
        /// </summary>
        private RwHtmlLiteralNode ReadBinding()
        {
            var startIndex = CurrentIndex;
            var binding = new RwHtmlBindingNode();

            Assert(RwHtmlTokenType.OpenBinding);
            Read();
            SkipWhitespace();

            // binding type
            Assert(RwHtmlTokenType.Text);
            binding.Name = Read().Text;
            SkipWhitespace();

            Assert(RwHtmlTokenType.Colon);
            Read();
            SkipWhitespace();

            // expression
            Assert(RwHtmlTokenType.Text);
            binding.Value = Read().Text;
            SkipWhitespace();

            Assert(RwHtmlTokenType.CloseBinding);
            Read();

            binding.Tokens.AddRange(GetTokensFrom(startIndex));
            return binding;
        }


        /// <summary>
        /// Reads the directive.
        /// </summary>
        private RwHtmlDirectiveNode ReadDirective()
        {
            var startIndex = CurrentIndex;
            var node = new RwHtmlDirectiveNode();

            Assert(RwHtmlTokenType.DirectiveStart);
            Read();
            SkipWhitespace();

            Assert(RwHtmlTokenType.Text);
            node.Name = Read().Text;
            SkipWhitespace();

            Assert(RwHtmlTokenType.Text);
            node.Value = Read().Text;
            SkipWhitespace();

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        private RwHtmlDirectiveNode ReadDoctype()
        {
            var startIndex = CurrentIndex;
            var node = new RwHtmlDirectiveNode();
            node.Name = Constants.DoctypeDirectiveName;

            Assert(RwHtmlTokenType.OpenDoctype);
            Read();
            SkipWhitespace();

            Assert(RwHtmlTokenType.Text);
            node.Value = Read().Text;
            SkipWhitespace();

            Assert(RwHtmlTokenType.CloseTag);
            Read();

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        /// <summary>
        /// Gets the tokens from.
        /// </summary>
        private IEnumerable<RwHtmlToken> GetTokensFrom(int startIndex)
        {
            return tokens.Skip(startIndex).Take(CurrentIndex - startIndex);
        }

        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        private void Assert(RwHtmlTokenType desiredType)
        {
            if (Peek() == null || Peek().Type != desiredType)
            {
                throw new Exception("Assertion failed!");
            }
        }

        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        private void SkipWhitespace()
        {
            ReadMultiple(t => t.Type == RwHtmlTokenType.WhiteSpace).ToList();
        }


        /// <summary>
        /// Peeks the current token.
        /// </summary>
        public RwHtmlToken Peek()
        {
            if (CurrentIndex < tokens.Count)
            {
                return tokens[CurrentIndex];
            }
            return null;
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public RwHtmlToken Read()
        {
            if (CurrentIndex < tokens.Count)
            {
                return tokens[CurrentIndex++];
            }
            throw new ParserException(Parser_RwHtml.UnexpectedEndOfInput);
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public IEnumerable<RwHtmlToken> ReadMultiple(Func<RwHtmlToken, bool> filter)
        {
            var current = Peek();
            while (current != null && filter(current))
            {
                yield return current;
                Read();
                current = Peek();
            }
        }
    }
}
