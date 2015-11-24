using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Resources;
using System.Diagnostics;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Exceptions;
using System.Net;
using System;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    /// <summary>
    /// Parses the results of the DothtmlTokenizer into a tree structure.
    /// </summary>
    public class DothtmlParser : ParserBase<DothtmlToken, DothtmlTokenType>
    {
        public static readonly HashSet<string> AutomaticClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "html", "head", "body", "p", "dt", "dd", "li", "option", "thead", "th", "tbody", "tr", "td", "tfoot", "colgroup"
        };

        private Stack<DothtmlNodeWithContent> ElementHierarchy { get; set; }

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

            // read content
            var doNotAppend = false;
            while (Peek() != null)
            {
                if (Peek().Type == DothtmlTokenType.DirectiveStart)
                {
                    // directive
                    root.Directives.Add(ReadDirective());
                    doNotAppend = true;
                }
                else if (Peek().Type == DothtmlTokenType.OpenTag)
                {
                    // element - check element hierarchy
                    var element = ReadElement();
                    if (ElementHierarchy.Any())
                    {
                        element.ParentNode = ElementHierarchy.Peek() as DothtmlElementNode;
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
                                element.NodeErrors.Add($"The closing tag '</{element.FullTagName}>' doesn't have a matching opening tag!");
                                CurrentElementContent.Add(element);
                            }
                            else
                            {
                                var beginTag = (DothtmlElementNode)ElementHierarchy.Peek();
                                var beginTagName = beginTag.FullTagName;
                                if (!beginTagName.Equals(element.FullTagName, StringComparison.OrdinalIgnoreCase))
                                {
                                    element.NodeErrors.Add($"The closing tag '</{beginTagName}>' doesn't have a matching opening tag!");
                                    ResolveWrongClosingTag(element);
                                    beginTag = ElementHierarchy.Peek() as DothtmlElementNode;

                                    if (beginTag != null && beginTagName != beginTag.FullTagName)
                                    {
                                        beginTag.CorrespondingEndTag = element;
                                        ElementHierarchy.Pop();
                                    }
                                    else
                                    {
                                        CurrentElementContent.Add(element);
                                    }
                                }
                                else
                                {
                                    ElementHierarchy.Pop();
                                    beginTag.CorrespondingEndTag = element;
                                }
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
                else if (Peek().Type == DothtmlTokenType.OpenCData)
                {
                    CurrentElementContent.Add(ReadCData());
                }
                else if (Peek().Type == DothtmlTokenType.OpenComment)
                {
                    CurrentElementContent.Add(ReadComment());
                }
                else if (Peek().Type == DothtmlTokenType.OpenServerComment)
                {
                    // skip server-side comment
                    SkipComment();
                }
                else
                {
                    // text
                    if (!doNotAppend 
                        && CurrentElementContent.Count > 0 
                        && CurrentElementContent[CurrentElementContent.Count - 1].GetType() == typeof(DothtmlLiteralNode)
                        && !((DothtmlLiteralNode)CurrentElementContent[CurrentElementContent.Count - 1]).IsComment)
                    {
                        // append to the previous literal
                        var lastLiteral = (DothtmlLiteralNode)CurrentElementContent[CurrentElementContent.Count - 1];
                        if (lastLiteral.Escape)
                            CurrentElementContent.Add(new DothtmlLiteralNode() { Tokens = { Peek() }, StartPosition = Peek().StartPosition });
                        else
                        {
                            lastLiteral.Tokens.Add(Peek());
                        }
                    }
                    else
                    {
                        CurrentElementContent.Add(new DothtmlLiteralNode() { Tokens = { Peek() }, StartPosition = Peek().StartPosition });
                    }
                    Read();

                    doNotAppend = false;
                }
            }

            // check element hierarchy
            if (ElementHierarchy.Count > 1)
            {
                root.NodeErrors.Add($"Unexpected end of file! The tag '<{ElementHierarchy.Peek()}>' was not closed!");
            }

            // set lengths to all nodes
            foreach (var node in root.EnumerateNodes())
            {
                node.Length = node.Tokens.Select(t => t.Length).DefaultIfEmpty(0).Sum();
            }

            Root = root;
            return root;
        }

        private void ResolveWrongClosingTag(DothtmlElementNode element)
        {
            Debug.Assert(element.IsClosingTag);
            var startElement = ElementHierarchy.Peek() as DothtmlElementNode;
            Debug.Assert(startElement != null);
            Debug.Assert(startElement.FullTagName != element.FullTagName);

            while (startElement != null && !startElement.FullTagName.Equals(element.FullTagName, StringComparison.OrdinalIgnoreCase))
            {
                ElementHierarchy.Pop();
                if (HtmlWriter.SelfClosingTags.Contains(startElement.FullTagName))
                {
                    // automatic immediate close of the tag (for <img src="">)
                    ElementHierarchy.Peek().Content.AddRange(startElement.Content);
                    startElement.Content.Clear();
                }
                else if (AutomaticClosingTags.Contains(startElement.FullTagName))
                {
                    // elements than can contain itself like <p> are closed on the first occurance of element with the same name
                    var sameElementIndex = startElement.Content.FindIndex(a => (a as DothtmlElementNode)?.FullTagName == startElement.FullTagName);
                    if (sameElementIndex >= 0)
                    {
                        var count = startElement.Content.Count - sameElementIndex;
                        ElementHierarchy.Peek().Content.AddRange(startElement.Content.Skip(sameElementIndex));
                        startElement.Content.RemoveRange(sameElementIndex, count);
                    }
                }

                // otherwise just pop the element
                startElement = ElementHierarchy.Peek() as DothtmlElementNode;
            }
        }

        private DothtmlLiteralNode ReadCData()
        {
            Assert(DothtmlTokenType.OpenCData);
            var node = new DothtmlLiteralNode()
            {
                StartPosition = Peek().StartPosition
            };
            node.Tokens.Add(Peek());
            Read();
            Assert(DothtmlTokenType.CDataBody);
            node.Tokens.Add(Peek());
            node.Escape = true;
            Read();
            Assert(DothtmlTokenType.CloseCData);
            node.Tokens.Add(Peek());
            Read();
            return node;
        }

        private DotHtmlCommentNode ReadComment()
        {
            var node = new DotHtmlCommentNode() { StartPosition = Peek().StartPosition };
            node.Tokens.Add(Peek());
            node.StartToken = Read();
            node.ValeNode = ReadTextValue(false, false, DothtmlTokenType.CommentBody);
            node.Tokens.Add(Peek());
            Assert(DothtmlTokenType.CloseComment);
            node.Tokens.Add(Peek());
            node.EndToken = Read();
            return node;
        }

        void SkipComment()
        {
            Read();
            Assert(DothtmlTokenType.CommentBody);
            Read();
            Assert(DothtmlTokenType.CloseComment);
            Read();
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

            if (Peek().Type == DothtmlTokenType.Slash)
            {
                Read();
                node.IsClosingTag = true;
            }

            // element name
            var nameOrPrefix = ReadName(true, false, DothtmlTokenType.Text);
            if (Peek().Type == DothtmlTokenType.Colon)
            {
                node.TagPrefixNode = nameOrPrefix;
                node.PrefixSeparator = Read();
                node.TagNameNode = ReadName(false, false, DothtmlTokenType.Text);
            }
            else
            {
                node.TagNameNode = nameOrPrefix;
            }
            //no mans whitespaces
            SkipWhiteSpace();

            // attributes
            if (!node.IsClosingTag)
            {
                SkipWhiteSpaceOrComment();
                while (Peek().Type == DothtmlTokenType.Text)
                {
                    var attribute = ReadAttribute();
                    node.Attributes.Add(attribute);
                    SkipWhiteSpaceOrComment();
                }

                if (Peek().Type == DothtmlTokenType.Slash)
                {
                    Read();
                    SkipWhiteSpace();
                    node.IsSelfClosingTag = true;
                }
            }

            Assert(DothtmlTokenType.CloseTag);
            Read();
            //Todo: check this
            SkipWhiteSpace();

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
            DothtmlNameNode nameOrPrefix = ReadName(false, false, DothtmlTokenType.Text);

            if (Peek().Type == DothtmlTokenType.Colon)
            {
                attribute.PrefixSeparatorToken = Read();

                attribute.AttributePrefixNode = nameOrPrefix;

                attribute.AttributeNameNode = ReadName(false, false, DothtmlTokenType.Text); ;
            }
            else
            {
                attribute.AttributeNameNode = nameOrPrefix;
            }
            //spaces before separator belong to name
            attribute.AttributeNameNode.WhitespacesAfter = SkipWhiteSpace();


            if (Peek().Type == DothtmlTokenType.Equals)
            {
                attribute.ValueSeparatorToken = Read();

                attribute.ValueStartTokens = SkipWhiteSpace();
                // attribute value
                if (Peek().Type == DothtmlTokenType.SingleQuote || Peek().Type == DothtmlTokenType.DoubleQuote)
                {
                    var quote = Peek().Type;
                    attribute.ValueStartTokens.Add(Read());

                    var startingWhitespaces = SkipWhiteSpace();

                    if (Peek().Type == DothtmlTokenType.OpenBinding)
                    {
                        attribute.ValueNode = ReadBindingValue(false, true);
                    }
                    else
                    {
                        attribute.ValueNode = ReadTextValue(false, true, DothtmlTokenType.Text);
                    }
                    //we had to jump forward to decide 
                    attribute.ValueNode.WhitespacesBefore = startingWhitespaces;

                    Assert(quote);
                    attribute.ValueEndTokens.Add(Read());
                }
                else
                {
                    attribute.ValueNode = ReadTextValue(false, false, DothtmlTokenType.Text);
                    //these are not part of any attribute or value
                    SkipWhiteSpace();
                }
                attribute.ValueEndTokens.AddRange(SkipWhiteSpace());
            }

            attribute.Tokens.AddRange(GetTokensFrom(startIndex));
            return attribute;
        }

        /// <summary>
        /// Reads the binding.
        /// </summary>
        private DothtmlBindingNode ReadBinding()
        {
            var startIndex = CurrentIndex;
            var binding = new DothtmlBindingNode() { StartPosition = Peek().StartPosition };

            Assert(DothtmlTokenType.OpenBinding);
            binding.StartToken = Read();

            binding.NameNode = ReadName(true, true, DothtmlTokenType.Text);

            Assert(DothtmlTokenType.Colon);
            binding.SeparatorToken = Read();

            binding.ValueNode = ReadTextValue(true, true, DothtmlTokenType.Text);

            Assert(DothtmlTokenType.CloseBinding);
            binding.EndToken = Read();

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
            node.DirectiveStartToken = Read();

            //consume only whitespaces before and after
            node.NameNode = ReadName(true, true, DothtmlTokenType.DirectiveName);

            //consume only whitespaces after
            node.ValueNode = ReadTextValue(false, true, DothtmlTokenType.DirectiveValue);

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlNameNode ReadName( bool whitespacesBefore, bool whiteSpacesAfter, DothtmlTokenType nameTokenType)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlNameNode() { StartPosition = Peek().StartPosition };

            if (whitespacesBefore)
            {
                node.WhitespacesBefore = SkipWhiteSpace();
            }

            Assert(nameTokenType);
            node.NameToken = Read();

            if (whiteSpacesAfter) {
                node.WhitespacesAfter = SkipWhiteSpace();
            }

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlValueTextNode ReadTextValue(bool whitespacesBefore, bool whiteSpacesAfter, DothtmlTokenType valueTokenType)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlValueTextNode() { StartPosition = Peek().StartPosition };

            if (whitespacesBefore)
            {
                node.WhitespacesBefore = SkipWhiteSpace();
            }

            Assert(valueTokenType);
            node.ValueToken = Read();

            if (whiteSpacesAfter)
            {
                node.WhitespacesAfter = SkipWhiteSpace();
            }

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlValueBindingNode ReadBindingValue(bool whitespacesBefore, bool whiteSpacesAfter)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlValueBindingNode() { StartPosition = Peek().StartPosition };

            if (whitespacesBefore)
            {
                node.WhitespacesBefore = SkipWhiteSpace();
            }

            Assert(DothtmlTokenType.OpenBinding);
            node.BindingNode = ReadBinding();
            node.ValueTokens = node.BindingNode.Tokens;

            if (whiteSpacesAfter)
            {
                node.WhitespacesAfter = SkipWhiteSpace();
            }

            node.Tokens.AddRange(GetTokensFrom(startIndex));
            return node;
        }

        void SkipWhiteSpaceOrComment()
        {
            while(true)
            {
                switch (Peek().Type)
                {
                    case DothtmlTokenType.WhiteSpace:
                        Read();
                        break;
                    case DothtmlTokenType.OpenComment:
                    case DothtmlTokenType.OpenServerComment:
                        SkipComment();
                        break;
                    default:
                        return;
                }
            }
        }

        protected override DothtmlTokenType WhiteSpaceToken => DothtmlTokenType.WhiteSpace;

        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        protected bool Assert(DothtmlTokenType desiredType)
        {
            if (Peek() == null || !Peek().Type.Equals(desiredType))
            {
                throw new DotvvmInternalException($"DotVVM parser internal error! The token {desiredType} was expected!");
            }
            return true;
        }
    }
}
