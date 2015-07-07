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
            Assert.AreEqual("test of a test", innerElement.Attributes[0].Literal.Value);
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
            Assert.AreEqual("test of a test", innerElement.Attributes[0].Literal.Value);
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
            Assert.AreEqual("test", innerElement.Attributes[0].Literal.Value);
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

            Assert.AreEqual("href", ((DothtmlElementNode)nodes[1]).Attributes[0].AttributeName);
            Assert.AreEqual("value", ((DothtmlBindingNode)((DothtmlElementNode)nodes[1]).Attributes[0].Literal).Name);
            Assert.AreEqual("test", ((DothtmlElementNode)nodes[1]).Attributes[0].Literal.Value);
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
            Assert.AreEqual("value", ((DothtmlBindingNode)((DothtmlElementNode)nodes[1]).Attributes[0].Literal).Name);
            Assert.AreEqual("test", ((DothtmlElementNode)nodes[1]).Attributes[0].Literal.Value);
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
            Assert.AreEqual("this is a content", ((DothtmlLiteralNode)result.Content[0]).Value);
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
            Assert.AreEqual("<!DOCTYPE html>\r\ntest", ((DothtmlLiteralNode)result.Content[0]).Value);
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
