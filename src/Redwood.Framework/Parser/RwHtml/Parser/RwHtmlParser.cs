using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    /// <summary>
    /// Parses the results of the RwHtmlTokenizer into a tree structure.
    /// </summary>
    public class RwHtmlParser
    {
        private readonly List<RwHtmlToken> tokens;
        private Stack<RwHtmlNodeWithContent> elementHierarchy;

        private int CurrentIndex { get; set; }


        private List<RwHtmlNode> CurrentElementContent
        {
            get { return elementHierarchy.Peek().Content; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RwHtmlParser"/> class.
        /// </summary>
        public RwHtmlParser(List<RwHtmlToken> tokens)
        {
            this.tokens = tokens;
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
                            if (elementHierarchy.Count == 0 || beginTagName != element.FullTagName)
                            {
                                element.NodeErrors.Add(string.Format(RwHtmlParserErrors.ClosingTagHasNoMatchingOpenTag, beginTagName));
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
                else
                {
                    // text
                    if (CurrentElementContent.Count > 0 && CurrentElementContent[CurrentElementContent.Count - 1].GetType() == typeof (RwHtmlLiteralNode))
                    {
                        // append to the previous literal
                        var lastLiteral = (RwHtmlLiteralNode)CurrentElementContent[CurrentElementContent.Count - 1];
                        lastLiteral.Value += Peek().Text;
                        lastLiteral.Tokens.Add(Peek());
                    }
                    else
                    {
                        CurrentElementContent.Add(new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() } });
                    }
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

            Assert(RwHtmlTokenType.DirectiveName);
            var directiveNameToken = Read();
            node.Name = directiveNameToken.Text.Trim();
            
            SkipWhitespace();

            Assert(RwHtmlTokenType.DirectiveValue);
            var directiveValueToken = Read();
            node.Value = directiveValueToken.Text.Trim();
            SkipWhitespace();

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
                throw new Exception("Assertion failed! This is internal error of the RWHTML parser.");
            }
        }

        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        private List<RwHtmlToken> SkipWhitespace()
        {
            return ReadMultiple(t => t.Type == RwHtmlTokenType.WhiteSpace).ToList();
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
