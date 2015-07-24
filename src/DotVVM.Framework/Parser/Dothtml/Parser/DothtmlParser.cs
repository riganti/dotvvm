using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    /// <summary>
    /// Parses the results of the DothtmlTokenizer into a tree structure.
    /// </summary>
    public class DothtmlParser
    {
        private IList<DothtmlToken> Tokens { get; set; }
        private Stack<DothtmlNodeWithContent> ElementHierarchy { get; set; }
        private int CurrentIndex { get; set; }
        private List<DothtmlNode> CurrentElementContent
        {
            get { return ElementHierarchy.Peek().Content; }
        }

        public DothtmlRootNode Root { get; private set; }


        /// <summary>
        /// Parses the token stream and gets the node.
        /// </summary>
        public DothtmlRootNode Parse(IList<DothtmlToken> tokens)
        {
            Root = null;
            Tokens = tokens;
            CurrentIndex = 0;
            ElementHierarchy = new Stack<DothtmlNodeWithContent>();

            // read file
            var root = new DothtmlRootNode();
            root.Tokens.AddRange(Tokens);
            ElementHierarchy.Push(root);
            SkipWhitespace();

            // read directives
            while (Peek() != null && Peek().Type == DothtmlTokenType.DirectiveStart)
            {
                root.Directives.Add(ReadDirective());
            }

            SkipWhitespace();
            
            // read content
            while (Peek() != null)
            {
                if (Peek().Type == DothtmlTokenType.OpenTag)
                {
                    // element - check element hierarchy
                    var element = ReadElement();
                    if (ElementHierarchy.Any())
                    {
                        element.ParentElement = ElementHierarchy.Peek() as DothtmlElementNode;
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
                                element.NodeErrors.Add(string.Format(DothtmlParserErrors.ClosingTagHasNoMatchingOpenTag, element.FullTagName));
                            }
                            else
                            {
                                var beginTag = (DothtmlElementNode)ElementHierarchy.Peek();
                                var beginTagName = beginTag.FullTagName;
                                if (beginTagName != element.FullTagName)
                                {
                                    element.NodeErrors.Add(string.Format(DothtmlParserErrors.ClosingTagHasNoMatchingOpenTag, beginTagName));
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
                else if (Peek().Type == DothtmlTokenType.OpenBinding)
                {
                    // binding
                    CurrentElementContent.Add(ReadBinding());
                }
                else
                {
                    // text
                    if (CurrentElementContent.Count > 0 && CurrentElementContent[CurrentElementContent.Count - 1].GetType() == typeof (DothtmlLiteralNode))
                    {
                        // append to the previous literal
                        var lastLiteral = (DothtmlLiteralNode)CurrentElementContent[CurrentElementContent.Count - 1];
                        lastLiteral.Value += Peek().Text;
                        lastLiteral.Tokens.Add(Peek());
                    }
                    else
                    {
                        CurrentElementContent.Add(new DothtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition });
                    }
                    Read();
                }
            }

            // check element hierarchy
            if (ElementHierarchy.Count > 1)
            {
                root.NodeErrors.Add(string.Format(DothtmlParserErrors.UnexpectedEndOfInputTagNotClosed, ElementHierarchy.Peek()));
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
        private DothtmlElementNode ReadElement()
        {
            var startIndex = CurrentIndex;
            var node = new DothtmlElementNode() { StartPosition = Peek().StartPosition };

            Assert(DothtmlTokenType.OpenTag);
            Read();
            SkipWhitespace();

            if (Peek().Type == DothtmlTokenType.Slash)
            {
                Read();
                SkipWhitespace();
                node.IsClosingTag = true;
            }

            // element name
            Assert(DothtmlTokenType.Text);
            node.TagNameToken = Read();
            node.TagName = node.TagNameToken.Text;
            if (Peek().Type == DothtmlTokenType.Colon)
            {
                Read();

                node.TagPrefix = node.TagName;
                node.TagPrefixToken = node.TagNameToken;
                Assert(DothtmlTokenType.Text);
                node.TagNameToken = Read();
                node.TagName = node.TagNameToken.Text;
            }
            SkipWhitespace();

            // attributes
            if (!node.IsClosingTag)
            {
                while (Peek().Type == DothtmlTokenType.Text)
                {
                    var attribute = ReadAttribute();
                    attribute.ParentElement = node;
                    node.Attributes.Add(attribute);
                    SkipWhitespace();
                }

                if (Peek().Type == DothtmlTokenType.Slash)
                {
                    Read();
                    SkipWhitespace();
                    node.IsSelfClosingTag = true;
                }
            }

            Assert(DothtmlTokenType.CloseTag);
            Read();
            SkipWhitespace();

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        /// <summary>
        /// Reads the attribute.
        /// </summary>
        private DothtmlAttributeNode ReadAttribute()
        {
            var startIndex = CurrentIndex;
            var attribute = new DothtmlAttributeNode() { StartPosition = Peek().StartPosition };

            // attribute name
            Assert(DothtmlTokenType.Text);
            attribute.AttributeNameToken = Read();
            attribute.AttributeName = attribute.AttributeNameToken.Text;
            if (Peek().Type == DothtmlTokenType.Colon)
            {
                Read();

                attribute.AttributePrefix = attribute.AttributeName;
                attribute.AttributePrefixToken = attribute.AttributeNameToken;
                Assert(DothtmlTokenType.Text);
                attribute.AttributeNameToken = Read();
                attribute.AttributeName = attribute.AttributeNameToken.Text;
            }
            SkipWhitespace();

            if (Peek().Type == DothtmlTokenType.Equals)
            {
                Read();
                SkipWhitespace();

                // attribute value
                if (Peek().Type == DothtmlTokenType.SingleQuote || Peek().Type == DothtmlTokenType.DoubleQuote)
                {
                    var quote = Peek().Type;
                    Read();

                    if (Peek().Type == DothtmlTokenType.OpenBinding)
                    {
                        attribute.Literal = ReadBinding();
                    }
                    else
                    {
                        Assert(DothtmlTokenType.Text);
                        attribute.Literal = new DothtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition };
                        Read();
                    }

                    Assert(quote);
                    Read();
                }
                else
                {
                    Assert(DothtmlTokenType.Text);
                    attribute.Literal = new DothtmlLiteralNode() { Value = Peek().Text, Tokens = { Peek() }, StartPosition = Peek().StartPosition };
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
        private DothtmlLiteralNode ReadBinding()
        {
            var startIndex = CurrentIndex;
            var binding = new DothtmlBindingNode() { StartPosition = Peek().StartPosition };

            Assert(DothtmlTokenType.OpenBinding);
            Read();
            SkipWhitespace();

            // binding type
            Assert(DothtmlTokenType.Text);
            binding.Name = Read().Text;
            SkipWhitespace();

            Assert(DothtmlTokenType.Colon);
            Read();
            SkipWhitespace();

            // expression
            Assert(DothtmlTokenType.Text);
            binding.Value = Read().Text;
            SkipWhitespace();

            Assert(DothtmlTokenType.CloseBinding);
            Read();

            binding.Tokens.AddRange(GetTokensFrom(startIndex));
            return binding;
        }


        /// <summary>
        /// Reads the directive.
        /// </summary>
        private DothtmlDirectiveNode ReadDirective()
        {
            var startIndex = CurrentIndex;
            var node = new DothtmlDirectiveNode() { StartPosition = Peek().StartPosition };

            Assert(DothtmlTokenType.DirectiveStart);
            Read();
            SkipWhitespace();

            Assert(DothtmlTokenType.DirectiveName);
            var directiveNameToken = Read();
            node.Name = directiveNameToken.Text.Trim();
            
            SkipWhitespace();

            Assert(DothtmlTokenType.DirectiveValue);
            var directiveValueToken = Read();
            node.Value = directiveValueToken.Text.Trim();
            SkipWhitespace();

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }
        
        /// <summary>
        /// Gets the tokens from.
        /// </summary>
        private IEnumerable<DothtmlToken> GetTokensFrom(int startIndex)
        {
            return Tokens.Skip(startIndex).Take(CurrentIndex - startIndex);
        }

        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        private void Assert(DothtmlTokenType desiredType)
        {
            if (Peek() == null || Peek().Type != desiredType)
            {
                throw new Exception("Assertion failed! This is internal error of the Dothtml parser.");
            }
        }

        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        private List<DothtmlToken> SkipWhitespace()
        {
            return ReadMultiple(t => t.Type == DothtmlTokenType.WhiteSpace).ToList();
        }


        /// <summary>
        /// Peeks the current token.
        /// </summary>
        public DothtmlToken Peek()
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
        public DothtmlToken Read()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex++];
            }
            throw new ParserException(Parser_Dothtml.UnexpectedEndOfInput);
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public IEnumerable<DothtmlToken> ReadMultiple(Func<DothtmlToken, bool> filter)
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
