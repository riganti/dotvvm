using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
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
        public DothtmlRootNode Parse(List<DothtmlToken> tokens)
        {
            Root = null;
            Tokens = tokens;
            CurrentIndex = 0;
            ElementHierarchy = new Stack<DothtmlNodeWithContent>();

            // read file
            var root = new DothtmlRootNode();
            root.Tokens.Add(Tokens);
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
                                element.AddWarning($"The closing tag '</{element.FullTagName}>' doesn't have a matching opening tag!");
                                CurrentElementContent.Add(element);
                            }
                            else
                            {
                                var beginTag = (DothtmlElementNode)ElementHierarchy.Peek();
                                var beginTagName = beginTag.FullTagName;
                                if (!beginTagName.Equals(element.FullTagName, StringComparison.OrdinalIgnoreCase))
                                {
                                    element.AddWarning($"The closing tag '</{element.FullTagName}>' doesn't have a matching opening tag!");
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
                    CurrentElementContent.Add(ReadServerComment());
                }
                else
                {
                    // text
                    if (!doNotAppend
                        && CurrentElementContent.Count > 0
                        && CurrentElementContent[CurrentElementContent.Count - 1].GetType() == typeof(DothtmlLiteralNode)
                        && !(CurrentElementContent[CurrentElementContent.Count - 1] is DotHtmlCommentNode))
                    {
                        // append to the previous literal
                        var lastLiteral = (DothtmlLiteralNode)CurrentElementContent[CurrentElementContent.Count - 1];
                        if (lastLiteral.Escape)
                            CurrentElementContent.Add(new DothtmlLiteralNode() { Tokens = { PeekPart() } });
                        else
                        {
                            lastLiteral.Tokens.Add(PeekPart());
                        }
                    }
                    else
                    {
                        CurrentElementContent.Add(new DothtmlLiteralNode() { Tokens = { PeekPart() } });
                    }
                    Read();

                    doNotAppend = false;
                }
            }

            // check element hierarchy
            if (ElementHierarchy.Count > 1)
            {
                ElementHierarchy.Peek().AddError($"Unexpected end of file! The tag '<{ElementHierarchy.Peek().CastTo<DothtmlElementNode>().TagName}>' was not closed!");
            }

            ResolveParents(root);

            Root = root;
            return root;
        }

        private void ResolveParents(DothtmlRootNode root)
        {
            var parentResolver = new ParentResolvingVisitor();
            root.Accept(parentResolver);
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
                if (HtmlWriter.IsSelfClosing(startElement.FullTagName))
                {
                    // automatic immediate close of the tag (for <img src="">)
                    ElementHierarchy.Peek().Content.AddRange(startElement.Content);
                    startElement.Content.Clear();
                    startElement.AddWarning("End tag is missing, the element is implicitly self-closed.");
                }
                else if (AutomaticClosingTags.Contains(startElement.FullTagName))
                {
                    // elements than can contain itself like <p> are closed on the first occurrence of element with the same name
                    var sameElementIndex = startElement.Content.FindIndex(a => (a as DothtmlElementNode)?.FullTagName == startElement.FullTagName);
                    
                    if(sameElementIndex < 0)
                    {
                        startElement.AddWarning($"End tag is missing, the element is implicitly closed with its parent tag or by the end of file.");
                    }
                    else if (sameElementIndex >= 0)
                    {
                        startElement.AddWarning($"End tag is missing, the element is implicitly closed by following <{startElement.Content[sameElementIndex].As<DothtmlElementNode>()?.FullTagName}> tag.");
                        startElement.Content[sameElementIndex].AddWarning($"Previous <{startElement.FullTagName}> is implicitly closed here.");

                        var count = startElement.Content.Count - sameElementIndex;
                        ElementHierarchy.Peek().Content.AddRange(startElement.Content.Skip(sameElementIndex));
                        startElement.Content.RemoveRange(sameElementIndex, count);
                    }
                }
                else
                {
                    startElement.AddWarning($"End tag is missing, the element is implicitly closed by </{element.FullTagName}>.");
                    element.AddWarning($"Element <{startElement.FullTagName}> is implicitly closed here.");
                }

                // otherwise just pop the element
                startElement = ElementHierarchy.Peek() as DothtmlElementNode;
            }
        }

        private DothtmlLiteralNode ReadCData()
        {
            Assert(DothtmlTokenType.OpenCData);
            var node = new DothtmlLiteralNode();
            node.Tokens.Add(PeekPart());
            Read();
            Assert(DothtmlTokenType.CDataBody);
            node.Tokens.Add(PeekPart());
            node.Escape = true;
            Read();
            Assert(DothtmlTokenType.CloseCData);
            node.Tokens.Add(PeekPart());
            Read();
            return node;
        }

        private DotHtmlCommentNode ReadComment()
        {
            var startIndex = CurrentIndex;
            var node = new DotHtmlCommentNode();

            node.StartToken = Read();
            node.ValueNode = ReadTextValue(false, false, DothtmlTokenType.CommentBody);
            Assert(DothtmlTokenType.CloseComment);
            node.EndToken = Read();

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        private DotHtmlCommentNode ReadServerComment()
        {
            var startIndex = CurrentIndex;
            var node = new DotHtmlCommentNode() { IsServerSide = true };

            Assert(DothtmlTokenType.OpenServerComment);
            node.StartToken = Read();
            node.ValueNode = ReadTextValue(false, false, DothtmlTokenType.CommentBody);
            Assert(DothtmlTokenType.CloseComment);
            node.EndToken = Read();

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        /// <summary>
        /// Reads the element.
        /// </summary>
        private DothtmlElementNode ReadElement()
        {
            var startIndex = CurrentIndex;
            var node = new DothtmlElementNode();

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
                ReadWhiteSpaceOrComment(node);
                while (Peek().Type == DothtmlTokenType.Text)
                {
                    var attribute = ReadAttribute();
                    node.Attributes.Add(attribute);

                    ReadWhiteSpaceOrComment(node);
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

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        /// <summary>
        /// Reads the attribute.
        /// </summary>
        private DothtmlAttributeNode ReadAttribute()
        {
            var startIndex = CurrentIndex;
            var attribute = new DothtmlAttributeNode();

            // attribute name
            DothtmlNameNode nameOrPrefix = ReadName(false, false, DothtmlTokenType.Text);

            if (Peek().Type == DothtmlTokenType.Colon)
            {
                attribute.PrefixSeparatorToken = Read();

                attribute.AttributePrefixNode = nameOrPrefix;

                attribute.AttributeNameNode = ReadName(false, false, DothtmlTokenType.Text);
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

                var valueStartTokens = SkipWhiteSpace();
                var valueEndTokens = new AggregateList<DothtmlToken>();
                // attribute value
                if (Peek().Type == DothtmlTokenType.SingleQuote || Peek().Type == DothtmlTokenType.DoubleQuote)
                {
                    var quote = Peek().Type;
                    Read();
                    valueStartTokens = valueStartTokens.AddLen(1);

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
                    valueEndTokens.Add(PeekPart());
                    Read();
                }
                else
                {
                    attribute.ValueNode = ReadTextValue(false, false, DothtmlTokenType.Text);
                    //these are not part of any attribute or value
                    SkipWhiteSpace();
                }
                attribute.ValueStartTokens = valueStartTokens;
                valueEndTokens.Add(SkipWhiteSpace());
                attribute.ValueEndTokens = valueEndTokens;
            }

            attribute.Tokens.Add(GetTokensFrom(startIndex));
            return attribute;
        }

        /// <summary>
        /// Reads the binding.
        /// </summary>
        private DothtmlBindingNode ReadBinding()
        {
            var startIndex = CurrentIndex;
            var binding = new DothtmlBindingNode();

            Assert(DothtmlTokenType.OpenBinding);
            binding.StartToken = Read();

            binding.NameNode = ReadName(true, true, DothtmlTokenType.Text);

            Assert(DothtmlTokenType.Colon);
            binding.SeparatorToken = Read();

            binding.ValueNode = ReadTextValue(true, true, DothtmlTokenType.Text);

            Assert(DothtmlTokenType.CloseBinding);
            binding.EndToken = Read();

            binding.Tokens.Add(GetTokensFrom(startIndex));
            return binding;
        }


        /// <summary>
        /// Reads the directive.
        /// </summary>
        private DothtmlDirectiveNode ReadDirective()
        {
            var startIndex = CurrentIndex;
            var node = new DothtmlDirectiveNode();

            Assert(DothtmlTokenType.DirectiveStart);
            node.DirectiveStartToken = Read();

            //consume only whitespaces before and after
            node.NameNode = ReadName(true, true, DothtmlTokenType.DirectiveName);

            //consume only whitespaces after
            node.ValueNode = ReadTextValue(false, true, DothtmlTokenType.DirectiveValue);

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlNameNode ReadName(bool whitespacesBefore, bool whiteSpacesAfter, DothtmlTokenType nameTokenType)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlNameNode();

            if (whitespacesBefore)
            {
                node.WhitespacesBefore = SkipWhiteSpace();
            }

            Assert(nameTokenType);
            node.NameToken = Read();

            if (whiteSpacesAfter)
            {
                node.WhitespacesAfter = SkipWhiteSpace();
            }

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlValueTextNode ReadTextValue(bool whitespacesBefore, bool whiteSpacesAfter, DothtmlTokenType valueTokenType)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlValueTextNode();

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

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        private DothtmlValueBindingNode ReadBindingValue(bool whitespacesBefore, bool whiteSpacesAfter)
        {
            var startIndex = CurrentIndex;

            var node = new DothtmlValueBindingNode();

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

            node.Tokens.Add(GetTokensFrom(startIndex));
            return node;
        }

        private void ReadWhiteSpaceOrComment(DothtmlElementNode node)
        {
            while (true)
            {
                switch (Peek().Type)
                {
                    case DothtmlTokenType.WhiteSpace:
                        if (node.AttributeSeparators == null) node.AttributeSeparators = new List<DothtmlToken>();
                        node.AttributeSeparators.Add(Read());
                        break;
                    case DothtmlTokenType.OpenComment:
                        if (node.InnerComments == null) node.InnerComments = new List<DotHtmlCommentNode>();
                        node.InnerComments.Add(ReadComment());
                        break;
                    case DothtmlTokenType.OpenServerComment:
                        if (node.InnerComments == null) node.InnerComments = new List<DotHtmlCommentNode>();
                        node.InnerComments.Add(ReadServerComment());
                        break;
                    default:
                        return;
                }
            }
        }

        protected override bool IsWhiteSpace(DothtmlToken token) => token.Type == DothtmlTokenType.WhiteSpace;

        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        protected bool Assert(DothtmlTokenType desiredType)
        {
            if (Peek() == null || Peek().Type != desiredType)
            {
                throw new Exception($"DotVVM parser internal error! The token {desiredType} was expected!");
            }
            return true;
        }
    }
}
