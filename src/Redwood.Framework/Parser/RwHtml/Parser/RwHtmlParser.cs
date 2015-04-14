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
        private IList<RwHtmlToken> Tokens { get; set; }
        private Stack<RwHtmlNodeWithContent> ElementHierarchy { get; set; }
        private int CurrentIndex { get; set; }
        private List<RwHtmlNode> CurrentElementContent
        {
            get { return ElementHierarchy.Peek().Content; }
        }

        public RwHtmlRootNode Root { get; private set; }


        /// <summary>
        /// Parses the token stream and gets the node.
        /// </summary>
        public RwHtmlRootNode Parse(IList<RwHtmlToken> tokens)
        {
            Root = null;
            Tokens = tokens;
            CurrentIndex = 0;
            ElementHierarchy = new Stack<RwHtmlNodeWithContent>();

            // read file
            var root = new RwHtmlRootNode();
            root.Tokens.AddRange(Tokens);
            ElementHierarchy.Push(root);
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
                    if (ElementHierarchy.Any())
                    {
                        element.ParentElement = ElementHierarchy.Peek() as RwHtmlElementNode;
                    }

                    if (!element.IsSelfClosingTag)
                    {
                        if (!element.IsClosingTag)
                        {
                            // open tag
                            CurrentElementContent.Add(element);
                            ElementHierarchy.Push(element);
                        }
                        else
                        {
                            // close tag
                            if (ElementHierarchy.Count <= 1)
                            {
                                element.NodeErrors.Add(string.Format(RwHtmlParserErrors.ClosingTagHasNoMatchingOpenTag, element.FullTagName));
                            }
                            else
                            {
                                var beginTag = (RwHtmlElementNode)ElementHierarchy.Peek();
                                var beginTagName = beginTag.FullTagName;
                                if (beginTagName != element.FullTagName)
                                {
                                    element.NodeErrors.Add(string.Format(RwHtmlParserErrors.ClosingTagHasNoMatchingOpenTag, beginTagName));
                                }
                                else
                                {
                                    ElementHierarchy.Pop();
                                }

                                beginTag.CorrespondingEndTag = element;
                            }
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
                        CurrentElementContent.Add(new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition });
                    }
                    Read();
                }
            }

            // check element hierarchy
            if (ElementHierarchy.Count > 1)
            {
                root.NodeErrors.Add(string.Format(RwHtmlParserErrors.UnexpectedEndOfInputTagNotClosed, ElementHierarchy.Peek()));
            }

            // set lengths to all nodes
            foreach (var node in root.EnumerateNodes())
            {
                node.Length = node.Tokens.Select(t => t.Length).DefaultIfEmpty(0).Sum();
            }

            Root = root;
            return root;
        }

        /// <summary>
        /// Reads the element.
        /// </summary>
        private RwHtmlElementNode ReadElement()
        {
            var startIndex = CurrentIndex;
            var node = new RwHtmlElementNode() { StartPosition = Peek().StartPosition };

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
            node.TagNameToken = Read();
            node.TagName = node.TagNameToken.Text;
            if (Peek().Type == RwHtmlTokenType.Colon)
            {
                Read();

                node.TagPrefix = node.TagName;
                node.TagPrefixToken = node.TagNameToken;
                Assert(RwHtmlTokenType.Text);
                node.TagNameToken = Read();
                node.TagName = node.TagNameToken.Text;
            }
            SkipWhitespace();

            // attributes
            if (!node.IsClosingTag)
            {
                while (Peek().Type == RwHtmlTokenType.Text)
                {
                    var attribute = ReadAttribute();
                    attribute.ParentElement = node;
                    node.Attributes.Add(attribute);
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
            var attribute = new RwHtmlAttributeNode() { StartPosition = Peek().StartPosition };

            // attribute name
            Assert(RwHtmlTokenType.Text);
            attribute.AttributeNameToken = Read();
            attribute.AttributeName = attribute.AttributeNameToken.Text;
            if (Peek().Type == RwHtmlTokenType.Colon)
            {
                Read();

                attribute.AttributePrefix = attribute.AttributeName;
                attribute.AttributePrefixToken = attribute.AttributeNameToken;
                Assert(RwHtmlTokenType.Text);
                attribute.AttributeNameToken = Read();
                attribute.AttributeName = attribute.AttributeNameToken.Text;
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
                        attribute.Literal = new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition };
                        Read();
                    }

                    Assert(quote);
                    Read();
                }
                else
                {
                    Assert(RwHtmlTokenType.Text);
                    attribute.Literal = new RwHtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition };
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
            var binding = new RwHtmlBindingNode() { StartPosition = Peek().StartPosition };

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
            var node = new RwHtmlDirectiveNode() { StartPosition = Peek().StartPosition };

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
            return Tokens.Skip(startIndex).Take(CurrentIndex - startIndex);
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
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex];
            }
            return null;
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public RwHtmlToken Read()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex++];
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
