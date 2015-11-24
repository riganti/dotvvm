using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    [TestClass]
    public class DothtmlParserTests
    {

        [TestMethod]
        public void DothtmlParser_Valid_TextOnly()
        {
            var markup = @"this is a test";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);
            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
        }

        [TestMethod]
        public void DothtmlParser_Valid_SingleElement()
        {
            var markup = @"this <b>is</b> a test";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("this ", ((DothtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlElementNode));
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[1]).FullTagName);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" a test", ((DothtmlLiteralNode)nodes[2]).Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_NestedElements()
        {
            var markup = @"this <b>is<a>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerContent = ((DothtmlElementNode)nodes[1]).Content;
            Assert.AreEqual(2, innerContent.Count);

            Assert.IsInstanceOfType(innerContent[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("is", ((DothtmlLiteralNode)innerContent[0]).Value);

            Assert.IsInstanceOfType(innerContent[1], typeof(DothtmlElementNode));
            Assert.AreEqual("a", ((DothtmlElementNode)innerContent[1]).FullTagName);
        }


        [TestMethod]
        public void DothtmlParser_Valid_DoubleQuotedAttribute()
        {
            var markup = @"this <b>is<a href=""test of a test"">test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (DothtmlElementNode)((DothtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].AttributeName);
            Assert.IsNull(innerElement.Attributes[0].AttributePrefix);

            Assert.AreEqual("test of a test", (innerElement.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
        }


        [TestMethod]
        public void DothtmlParser_Valid_SingleQuotedAttribute()
        {
            var markup = @"this <b>is<a href='test of a test'>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (DothtmlElementNode)((DothtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].AttributeName);
            Assert.IsNull(innerElement.Attributes[0].AttributePrefix);
            Assert.AreEqual("test of a test", (innerElement.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
        }


        [TestMethod]
        public void DothtmlParser_Valid_AttributeWithoutValue()
        {
            var markup = @"this <input type=checkbox checked /> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (DothtmlElementNode)nodes[1];
            Assert.AreEqual(2, innerElement.Attributes.Count);

            Assert.AreEqual("type", innerElement.Attributes[0].AttributeName);
            Assert.IsNull(innerElement.Attributes[0].AttributePrefix);

            Assert.AreEqual("checked", innerElement.Attributes[1].AttributeName);
            Assert.IsNull(innerElement.Attributes[1].AttributePrefix);
            Assert.IsNull(innerElement.Attributes[1].ValueNode);
        }

        [TestMethod]
        public void DothtmlParser_Valid_UnquotedAttribute()
        {
            var markup = @"this <b>is<a href=test>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (DothtmlElementNode)((DothtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].AttributeName);
            Assert.IsNull(innerElement.Attributes[0].AttributePrefix);
            Assert.AreEqual("test", (innerElement.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
        }



        [TestMethod]
        public void DothtmlParser_Valid_BindingInText()
        {
            var markup = @"this {{value: test}} <b>is</b>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(4, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("this ", ((DothtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlBindingNode));
            Assert.AreEqual("value", ((DothtmlBindingNode)nodes[1]).Name);
            Assert.AreEqual("test", ((DothtmlBindingNode)nodes[1]).Value);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" ", ((DothtmlLiteralNode)nodes[2]).Value);

            Assert.IsInstanceOfType(nodes[3], typeof(DothtmlElementNode));
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[3]).FullTagName);
            Assert.AreEqual(1, ((DothtmlElementNode)nodes[3]).Content.Count);
        }


        [TestMethod]
        public void DothtmlParser_Valid_BindingInAttributeValue()
        {
            var markup = @"this <a href='{value: test}'/>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("this ", ((DothtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlElementNode));
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.AreEqual(0, ((DothtmlElementNode)nodes[1]).Content.Count);

            Assert.AreEqual("href", (nodes[1] as DothtmlElementNode).Attributes[0].AttributeName);
            Assert.AreEqual("value", ((nodes[1] as DothtmlElementNode).Attributes[0].ValueNode as DothtmlValueBindingNode).BindingNode.Name);
            Assert.AreEqual("test", ((nodes[1] as DothtmlElementNode).Attributes[0].ValueNode as DothtmlValueBindingNode).BindingNode.Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_DoubleBraceBindingInAttributeValue()
        {
            var markup = @"this <a href='{{value: test}}'/>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("this ", ((DothtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlElementNode));
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.AreEqual(0, ((DothtmlElementNode)nodes[1]).Content.Count);

            Assert.AreEqual("href", ((DothtmlElementNode)nodes[1]).Attributes[0].AttributeName);
            Assert.AreEqual("value", (((DothtmlElementNode)nodes[1]).Attributes[0].ValueNode as DothtmlValueBindingNode).BindingNode.Name);
            Assert.AreEqual("test", (((DothtmlElementNode)nodes[1]).Attributes[0].ValueNode as DothtmlValueBindingNode).BindingNode.Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_Directives()
        {
            var markup = @"@viewmodel MyNamespace.TestViewModel, MyAssembly   
@basetype Test

this is a content";
            var result = ParseMarkup(markup);

            Assert.AreEqual(2, result.Directives.Count);
            Assert.AreEqual("viewmodel", result.Directives[0].Name);
            Assert.AreEqual("MyNamespace.TestViewModel, MyAssembly", result.Directives[0].Value);
            Assert.AreEqual("basetype", result.Directives[1].Name);
            Assert.AreEqual("Test", result.Directives[1].Value);

            Assert.AreEqual(1, result.Content.Count);
            Assert.IsInstanceOfType(result.Content[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("\r\nthis is a content", ((DothtmlLiteralNode)result.Content[0]).Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_Doctype()
        {
            var markup = @"@viewmodel MyNamespace.TestViewModel, MyAssembly   

<!DOCTYPE html>
test";
            var result = ParseMarkup(markup);

            Assert.AreEqual(1, result.Directives.Count);
            Assert.AreEqual("viewmodel", result.Directives[0].Name);
            Assert.AreEqual("MyNamespace.TestViewModel, MyAssembly", result.Directives[0].Value);

            Assert.AreEqual(1, result.Content.Count);
            Assert.IsInstanceOfType(result.Content[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("\r\n<!DOCTYPE html>\r\ntest", ((DothtmlLiteralNode)result.Content[0]).Value);
        }



        [TestMethod]
        public void DothtmlParser_Invalid_SpaceInTagName()
        {
            var markup = @"<dot:ContentPlace Holder DataContext=""sdads"">
</ dot:ContentPlaceHolder > ";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.AreEqual("Holder", ((DothtmlElementNode)nodes[0]).Attributes[0].AttributeName);
            Assert.AreEqual(null, ((DothtmlElementNode)nodes[0]).Attributes[0].ValueNode);

            Assert.AreEqual("DataContext", ((DothtmlElementNode)nodes[0]).Attributes[1].AttributeName);
            Assert.AreEqual("sdads", (((DothtmlElementNode)nodes[0]).Attributes[1].ValueNode as DothtmlValueTextNode).Text);

            Assert.IsTrue(((DothtmlElementNode)nodes[1]).IsClosingTag);
            Assert.AreEqual("", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).HasNodeErrors);

            Assert.AreEqual(" dot:ContentPlaceHolder > ", ((DothtmlLiteralNode)nodes[2]).Value);
        }

        [TestMethod]
        public void DothtmlParser_Invalid_ClosingTagOnly()
        {
            var markup = @"</a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);

            Assert.IsTrue(((DothtmlElementNode)nodes[0]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[0]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[0]).HasNodeErrors);
        }

        [TestMethod]
        public void DothtmlParser_SlashAttributeValue()
        {
            var markup = "<a href=/>Test</a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);
            var ael = (DothtmlElementNode)nodes[0];
            Assert.AreEqual("a", ael.FullTagName);
            Assert.AreEqual(1, ael.Attributes.Count);
            Assert.AreEqual("href", ael.Attributes[0].AttributeName);
            Assert.AreEqual("/", (ael.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
        }

        [TestMethod]
        public void DothtmlParser_Invalid_ClosingTags()
        {
            var markup = @"</a></b>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsTrue(((DothtmlElementNode)nodes[0]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[0]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[0]).HasNodeErrors);

            Assert.IsTrue(((DothtmlElementNode)nodes[1]).IsClosingTag);
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).HasNodeErrors);
        }

        [TestMethod]
        public void DothtmlParser_Invalid_ClosingTagInsideElement()
        {
            var markup = @"<a></b></a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsFalse(((DothtmlElementNode)nodes[0]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[0]).FullTagName);
            Assert.IsFalse(((DothtmlElementNode)nodes[0]).HasNodeErrors);

            Assert.IsTrue(((DothtmlElementNode)nodes[1]).IsClosingTag);
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).HasNodeErrors);

            Assert.IsTrue(((DothtmlElementNode)nodes[2]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[2]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[2]).HasNodeErrors);
        }


        [TestMethod]
        public void DothtmlParser_Invalid_UnclosedLinkInHead()
        {
            var markup = @"<html><head><link></head><body></body></html>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);

            var html = ((DothtmlElementNode)nodes[0]);
            Assert.IsFalse(html.IsClosingTag);
            Assert.AreEqual("html", html.FullTagName);
            Assert.IsFalse(html.HasNodeErrors);
            Assert.AreEqual(2, html.Content.Count);

            var head = ((DothtmlElementNode)html.Content[0]);
            Assert.IsFalse(head.IsClosingTag);
            Assert.AreEqual("head", head.FullTagName);
            Assert.IsFalse(head.HasNodeErrors);
            Assert.AreEqual(1, head.Content.Count);

            var link = ((DothtmlElementNode)head.Content[0]);
            Assert.IsFalse(link.IsClosingTag);
            Assert.AreEqual("link", link.FullTagName);
            Assert.IsFalse(link.HasNodeErrors);
            Assert.AreEqual(0, link.Content.Count);

            var body = ((DothtmlElementNode)html.Content[1]);
            Assert.IsFalse(body.IsClosingTag);
            Assert.AreEqual("body", body.FullTagName);
            Assert.IsFalse(body.HasNodeErrors);
            Assert.AreEqual(0, body.Content.Count);
        }

        [TestMethod]
        public void DothtmlParser_Valid_Comment()
        {
            var markup = @"test <!--<a href=""test1"">test2</a>--> test3 <img />";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(4, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("test ", ((DothtmlLiteralNode)nodes[0]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[0]).IsComment);

            Assert.IsInstanceOfType(nodes[1], typeof(DotHtmlCommentNode));
            Assert.AreEqual(@"<a href=""test1"">test2</a>", ((DotHtmlCommentNode)nodes[1]).Value);
           
            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" test3 ", ((DothtmlLiteralNode)nodes[2]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[2]).IsComment);

            Assert.IsInstanceOfType(nodes[3], typeof(DothtmlElementNode));
            Assert.AreEqual("img", ((DothtmlElementNode)nodes[3]).TagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[3]).IsSelfClosingTag);
        }

        [TestMethod]
        public void DothtmlParser_Valid_CData()
        {
            var markup = @"test <![CDATA[<a href=""test1"">test2</a>]]> test3 <img />";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(4, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DothtmlLiteralNode));
            Assert.AreEqual("test ", ((DothtmlLiteralNode)nodes[0]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[0]).IsComment);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"<![CDATA[<a href=""test1"">test2</a>]]>", ((DothtmlLiteralNode)nodes[1]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[1]).IsComment);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" test3 ", ((DothtmlLiteralNode)nodes[2]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[2]).IsComment);

            Assert.IsInstanceOfType(nodes[3], typeof(DothtmlElementNode));
            Assert.AreEqual("img", ((DothtmlElementNode)nodes[3]).TagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[3]).IsSelfClosingTag);
        }

        [TestMethod]
        public void DothtmlParser_Valid_CommentBeforeDirective()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\r\nTest";
            var root = ParseMarkup(markup);
            var nodes = root.Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DotHtmlCommentNode));
            Assert.AreEqual(" my comment ", ((DotHtmlCommentNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@" ", ((DothtmlLiteralNode)nodes[1]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[1]).IsComment);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"Test", ((DothtmlLiteralNode)nodes[2]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[2]).IsComment);

            Assert.AreEqual(1, root.Directives.Count);
            Assert.AreEqual("viewModel", root.Directives[0].Name);
            Assert.AreEqual("TestDirective", root.Directives[0].Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_CommentInsideDirectives()
        {
            var markup = "@masterPage hello\r\n<!-- my comment --> @viewModel TestDirective\r\nTest";
            var root = ParseMarkup(markup);
            var nodes = root.Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DotHtmlCommentNode));
            Assert.AreEqual(" my comment ", ((DotHtmlCommentNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@" ", ((DothtmlLiteralNode)nodes[1]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[1]).IsComment);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"Test", ((DothtmlLiteralNode)nodes[2]).Value);
            Assert.IsFalse(((DothtmlLiteralNode)nodes[2]).IsComment);

            Assert.AreEqual(2, root.Directives.Count);
            Assert.AreEqual("masterPage", root.Directives[0].Name);
            Assert.AreEqual("hello", root.Directives[0].Value);
            Assert.AreEqual("viewModel", root.Directives[1].Name);
            Assert.AreEqual("TestDirective", root.Directives[1].Value);
        }

        [TestMethod]
        public void BindingParser_TextBinding_Invalid_MissingName()
        {
            var markup = "<a href='#'>{{Property1}}</a>";
            var node = (DothtmlElementNode)ParseMarkup(markup).Content[0];
            Assert.AreEqual("a", (node.FullTagName));
            Assert.IsInstanceOfType(node.Content[0], typeof(DothtmlBindingNode));
            var content = node.Content[0] as DothtmlBindingNode;

        }

        public static DothtmlRootNode ParseMarkup(string markup)
        {
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(markup));
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);
            return node;
        }

    }
}
