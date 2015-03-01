using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Tests.Parser.RwHtml
{
    [TestClass]
    public class RwHtmlParserTests
    {

        [TestMethod]
        public void RwHtmlParser_Valid_TextOnly()
        {
            var markup = @"this is a test";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);
            Assert.IsInstanceOfType(nodes[0], typeof(RwHtmlLiteralNode));
        }

        [TestMethod]
        public void RwHtmlParser_Valid_SingleElement()
        {
            var markup = @"this <b>is</b> a test";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("this ", ((RwHtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(RwHtmlElementNode));
            Assert.AreEqual("b", ((RwHtmlElementNode)nodes[1]).FullTagName);

            Assert.IsInstanceOfType(nodes[2], typeof(RwHtmlLiteralNode));
            Assert.AreEqual(" a test", ((RwHtmlLiteralNode)nodes[2]).Value);
        }

        [TestMethod]
        public void RwHtmlParser_Valid_NestedElements()
        {
            var markup = @"this <b>is<a>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;
            
            var innerContent = ((RwHtmlElementNode)nodes[1]).Content;
            Assert.AreEqual(2, innerContent.Count);

            Assert.IsInstanceOfType(innerContent[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("is", ((RwHtmlLiteralNode)innerContent[0]).Value);

            Assert.IsInstanceOfType(innerContent[1], typeof(RwHtmlElementNode));
            Assert.AreEqual("a", ((RwHtmlElementNode)innerContent[1]).FullTagName);
        }


        [TestMethod]
        public void RwHtmlParser_Valid_DoubleQuotedAttribute()
        {
            var markup = @"this <b>is<a href=""test of a test"">test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (RwHtmlElementNode)((RwHtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].Name);
            Assert.IsNull(innerElement.Attributes[0].Prefix);
            Assert.AreEqual("test of a test", innerElement.Attributes[0].Literal.Value);
        }


        [TestMethod]
        public void RwHtmlParser_Valid_SingleQuotedAttribute()
        {
            var markup = @"this <b>is<a href='test of a test'>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (RwHtmlElementNode)((RwHtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].Name);
            Assert.IsNull(innerElement.Attributes[0].Prefix);
            Assert.AreEqual("test of a test", innerElement.Attributes[0].Literal.Value);
        }



        [TestMethod]
        public void RwHtmlParser_Valid_UnquotedAttribute()
        {
            var markup = @"this <b>is<a href=test>test</a></b> a test";
            var nodes = ParseMarkup(markup).Content;

            var innerElement = (RwHtmlElementNode)((RwHtmlElementNode)nodes[1]).Content[1];
            Assert.AreEqual(1, innerElement.Attributes.Count);
            Assert.AreEqual("href", innerElement.Attributes[0].Name);
            Assert.IsNull(innerElement.Attributes[0].Prefix);
            Assert.AreEqual("test", innerElement.Attributes[0].Literal.Value);
        }



        [TestMethod]
        public void RwHtmlParser_Valid_BindingInText()
        {
            var markup = @"this {{value: test}} <b>is</b>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(4, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("this ", ((RwHtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(RwHtmlBindingNode));
            Assert.AreEqual("value", ((RwHtmlBindingNode)nodes[1]).Name);
            Assert.AreEqual("test", ((RwHtmlBindingNode)nodes[1]).Value);

            Assert.IsInstanceOfType(nodes[2], typeof(RwHtmlLiteralNode));
            Assert.AreEqual(" ", ((RwHtmlLiteralNode)nodes[2]).Value);

            Assert.IsInstanceOfType(nodes[3], typeof(RwHtmlElementNode));
            Assert.AreEqual("b", ((RwHtmlElementNode)nodes[3]).FullTagName);
            Assert.AreEqual(1, ((RwHtmlElementNode)nodes[3]).Content.Count);
        }


        [TestMethod]
        public void RwHtmlParser_Valid_BindingInAttributeValue()
        {
            var markup = @"this <a href='{value: test}'/>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("this ", ((RwHtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(RwHtmlElementNode));
            Assert.AreEqual("a", ((RwHtmlElementNode)nodes[1]).FullTagName);
            Assert.AreEqual(0, ((RwHtmlElementNode)nodes[1]).Content.Count);

            Assert.AreEqual("href", ((RwHtmlElementNode)nodes[1]).Attributes[0].Name);
            Assert.AreEqual("value", ((RwHtmlBindingNode)((RwHtmlElementNode)nodes[1]).Attributes[0].Literal).Name);
            Assert.AreEqual("test", ((RwHtmlElementNode)nodes[1]).Attributes[0].Literal.Value);
        }

        [TestMethod]
        public void RwHtmlParser_Valid_DoubleBraceBindingInAttributeValue()
        {
            var markup = @"this <a href='{{value: test}}'/>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("this ", ((RwHtmlLiteralNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(RwHtmlElementNode));
            Assert.AreEqual("a", ((RwHtmlElementNode)nodes[1]).FullTagName);
            Assert.AreEqual(0, ((RwHtmlElementNode)nodes[1]).Content.Count);

            Assert.AreEqual("href", ((RwHtmlElementNode)nodes[1]).Attributes[0].Name);
            Assert.AreEqual("value", ((RwHtmlBindingNode)((RwHtmlElementNode)nodes[1]).Attributes[0].Literal).Name);
            Assert.AreEqual("test", ((RwHtmlElementNode)nodes[1]).Attributes[0].Literal.Value);
        }

        [TestMethod]
        public void RwHtmlParser_Valid_Directives()
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
            Assert.IsInstanceOfType(result.Content[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("this is a content", ((RwHtmlLiteralNode)result.Content[0]).Value);
        }

        [TestMethod]
        public void RwHtmlParser_Valid_Doctype()
        {
            var markup = @"@viewmodel MyNamespace.TestViewModel, MyAssembly   

<!DOCTYPE html>
test";
            var result = ParseMarkup(markup);

            Assert.AreEqual(1, result.Directives.Count);
            Assert.AreEqual("viewmodel", result.Directives[0].Name);
            Assert.AreEqual("MyNamespace.TestViewModel, MyAssembly", result.Directives[0].Value);

            Assert.AreEqual(1, result.Content.Count);
            Assert.IsInstanceOfType(result.Content[0], typeof(RwHtmlLiteralNode));
            Assert.AreEqual("<!DOCTYPE html>\r\ntest", ((RwHtmlLiteralNode)result.Content[0]).Value);
        }


        public static RwHtmlRootNode ParseMarkup(string markup)
        {
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(markup));
            var parser = new RwHtmlParser(tokenizer.Tokens);
            var node = parser.Parse();
            return node;
        }

    }
}
