using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void DothtmlParser_Valid_UnquotedAttributeWithWhitespace()
        {
            var markup = @"this <b>is<a href   =  test>test</a></b> a test";
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
        public void DothtmlParser_Valid_BindingInUnquotedAttributeValue()
        {
            var markup = @"this <a href={value: test}/>";
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
            Assert.AreEqual("\nthis is a content", ((DothtmlLiteralNode)result.Content[0]).Value);
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
            Assert.AreEqual("\n<!DOCTYPE html>\ntest", ((DothtmlLiteralNode)result.Content[0]).Value);
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
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).NodeWarnings.Any());

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
            Assert.IsTrue(((DothtmlElementNode)nodes[0]).NodeWarnings.Any());
        }

        [TestMethod]
        public void DothtmlParser_SlashAttributeValue()
        {
            var markup = "<a href='/'>Test</a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);
            var ael = (DothtmlElementNode)nodes[0];
            Assert.AreEqual("a", ael.FullTagName);
            Assert.AreEqual(1, ael.Attributes.Count);
            Assert.AreEqual("href", ael.Attributes[0].AttributeName);
            Assert.AreEqual("/", (ael.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
        }

        [TestMethod]
        public void DothtmlParser_UnquotedSlashAttributeValue()
        {
            var markup = "<a href=/>Test</a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(1, nodes.Count);
            var ael = (DothtmlElementNode)nodes[0];
            Assert.AreEqual("a", ael.FullTagName);
            Assert.AreEqual(1, ael.Attributes.Count);
            Assert.AreEqual("href", ael.Attributes[0].AttributeName);
            Assert.AreEqual("", (ael.Attributes[0].ValueNode as DothtmlValueTextNode).Text);
            Assert.IsTrue(ael.Tokens.Any(n => n.Error is object));
        }

        [TestMethod]
        public void DothtmlParser_Invalid_ClosingTags()
        {
            var markup = @"</a></b>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(2, nodes.Count);

            Assert.IsTrue(((DothtmlElementNode)nodes[0]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[0]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[0]).NodeWarnings.Any());

            Assert.IsTrue(((DothtmlElementNode)nodes[1]).IsClosingTag);
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).NodeWarnings.Any());
        }

        [TestMethod]
        public void DothtmlParser_Invalid_ClosingTagInsideElement()
        {
            var markup = @"<a></b></a>";
            var nodes = ParseMarkup(markup).Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsFalse(((DothtmlElementNode)nodes[0]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[0]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[0]).NodeWarnings.Any());

            Assert.IsTrue(((DothtmlElementNode)nodes[1]).IsClosingTag);
            Assert.AreEqual("b", ((DothtmlElementNode)nodes[1]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[1]).NodeWarnings.Any());

            Assert.IsTrue(((DothtmlElementNode)nodes[2]).IsClosingTag);
            Assert.AreEqual("a", ((DothtmlElementNode)nodes[2]).FullTagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[2]).NodeWarnings.Any());
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

            Assert.IsInstanceOfType(nodes[1], typeof(DotHtmlCommentNode));
            Assert.AreEqual(@"<a href=""test1"">test2</a>", ((DotHtmlCommentNode)nodes[1]).Value);
           
            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" test3 ", ((DothtmlLiteralNode)nodes[2]).Value);

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

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"<a href=""test1"">test2</a>", ((DothtmlLiteralNode)nodes[1]).Value);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(" test3 ", ((DothtmlLiteralNode)nodes[2]).Value);

            Assert.IsInstanceOfType(nodes[3], typeof(DothtmlElementNode));
            Assert.AreEqual("img", ((DothtmlElementNode)nodes[3]).TagName);
            Assert.IsTrue(((DothtmlElementNode)nodes[3]).IsSelfClosingTag);
        }

        [TestMethod]
        public void DothtmlParser_Valid_CommentBeforeDirective()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\nTest";
            var root = ParseMarkup(markup);
            var nodes = root.Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DotHtmlCommentNode));
            Assert.AreEqual(" my comment ", ((DotHtmlCommentNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@" ", ((DothtmlLiteralNode)nodes[1]).Value);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"Test", ((DothtmlLiteralNode)nodes[2]).Value);

            Assert.AreEqual(1, root.Directives.Count);
            Assert.AreEqual("viewModel", root.Directives[0].Name);
            Assert.AreEqual("TestDirective", root.Directives[0].Value);
        }

        [TestMethod]
        public void DothtmlParser_Valid_CommentInsideDirectives()
        {
            var markup = "@masterPage hello\n<!-- my comment --> @viewModel TestDirective\nTest";
            var root = ParseMarkup(markup);
            var nodes = root.Content;

            Assert.AreEqual(3, nodes.Count);

            Assert.IsInstanceOfType(nodes[0], typeof(DotHtmlCommentNode));
            Assert.AreEqual(" my comment ", ((DotHtmlCommentNode)nodes[0]).Value);

            Assert.IsInstanceOfType(nodes[1], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@" ", ((DothtmlLiteralNode)nodes[1]).Value);

            Assert.IsInstanceOfType(nodes[2], typeof(DothtmlLiteralNode));
            Assert.AreEqual(@"Test", ((DothtmlLiteralNode)nodes[2]).Value);

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

        [TestMethod]
        public void DothtmlParser_HierarchyBuildingVisitor_Element_InvalidTag()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\n<div><div><ul><li>item</li><ul><a href='lol'>link</a></div></div>";
            var root = ParseMarkup(markup);

            var visitor = new HierarchyBuildingVisitor {
                CursorPosition = 61
            };

            root.Accept(visitor);

            var cursorNode = visitor.LastFoundNode;
            var hierarchyList = visitor.GetHierarchy();

            Assert.AreEqual(6, hierarchyList.Count);

            Assert.IsInstanceOfType(cursorNode, typeof(DothtmlNameNode));
            var cursorName = cursorNode as DothtmlNameNode;

            var parentNode = cursorNode.ParentNode;
            Assert.IsInstanceOfType(parentNode, typeof(DothtmlElementNode));

            var parentElement = parentNode as DothtmlElementNode;
            Assert.AreEqual(parentElement.TagName, cursorName.Text);
            Assert.AreEqual(parentElement.TagName, "li");

        }

        [TestMethod]
        public void DothtmlParser_HierarchyBuildingVisitor_Element_UnclosedTagContent()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\n<div><div><ul><li>\n\t\t\t\t<a href='lol'></a></li></ul>\n</div></div>";
            var root = ParseMarkup(markup);

            var visitor = new HierarchyBuildingVisitor
            {
                CursorPosition = 81
            };

            root.Accept(visitor);

            var hierarchyList = visitor.GetHierarchy();
            var lastElement = hierarchyList.Where( node => node is DothtmlElementNode).First() as DothtmlElementNode;

            Assert.AreEqual(6, hierarchyList.Count);

            Assert.AreEqual(lastElement.FullTagName, "a");

            Assert.IsInstanceOfType(lastElement.ParentNode, typeof(DothtmlElementNode));
            var parentLiElement = lastElement.ParentNode as DothtmlElementNode;
            Assert.AreEqual(parentLiElement.FullTagName, "li");

            Assert.IsInstanceOfType(parentLiElement.ParentNode, typeof(DothtmlElementNode));
            var parentUlElement = parentLiElement.ParentNode as DothtmlElementNode;
            Assert.AreEqual(parentUlElement.FullTagName, "ul");

            Assert.AreEqual((hierarchyList[0] as DothtmlElementNode)?.FullTagName,"a");
            Assert.AreEqual((hierarchyList[1] as DothtmlElementNode)?.FullTagName, "li");
            Assert.AreEqual((hierarchyList[2] as DothtmlElementNode)?.FullTagName, "ul");
            Assert.AreEqual((hierarchyList[3] as DothtmlElementNode)?.FullTagName, "div");
            Assert.AreEqual((hierarchyList[4] as DothtmlElementNode)?.FullTagName, "div");
            Assert.IsInstanceOfType(hierarchyList[5], typeof(DothtmlRootNode));
        }

        [TestMethod]
        public void DothtmlParser_HierarchyBuildingVisitor_Element_Valid()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\n<div><div><ul><li>item</li></ul><a href='lol'>link</a></div></div>";
            var root = ParseMarkup(markup);

            var visitor = new HierarchyBuildingVisitor
            {
                CursorPosition = 61
            };

            root.Accept(visitor);

            var cursorNode = visitor.LastFoundNode;
            var hierarchyList = visitor.GetHierarchy();

            Assert.AreEqual(6, hierarchyList.Count);

            Assert.IsInstanceOfType(cursorNode, typeof(DothtmlNameNode));
            var cursorName = cursorNode as DothtmlNameNode;

            var parentNode = cursorNode.ParentNode;
            Assert.IsInstanceOfType(parentNode, typeof(DothtmlElementNode));

            var parentElement = parentNode as DothtmlElementNode;
            Assert.AreEqual(parentElement.TagName, cursorName.Text);
            Assert.AreEqual(parentElement.TagName, "li");

        }

        [TestMethod]
        public void DothtmlParser_HierarchyBuildingVisitor_Attribute_TextValue()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\n<div><div><ul><li>item</li></ul><a href='lol'>link</a></div></div>";
            var root = ParseMarkup(markup);

            var visitor = new HierarchyBuildingVisitor
            {
                CursorPosition = 87
            };

            root.Accept(visitor);

            var cursorNode = visitor.LastFoundNode;
            var hierarchyList = visitor.GetHierarchy();

            Assert.AreEqual(6, hierarchyList.Count);

            Assert.IsInstanceOfType(cursorNode, typeof(DothtmlValueTextNode));
            var cursorValue = cursorNode as DothtmlValueTextNode;

            var parentNode = cursorNode.ParentNode;
            Assert.IsInstanceOfType(parentNode, typeof(DothtmlAttributeNode));
            var parentAttribute = parentNode as DothtmlAttributeNode;
            Assert.AreEqual(parentAttribute.AttributeName, "href");

            var parentParentNode = parentAttribute.ParentNode;
            Assert.IsInstanceOfType(parentParentNode, typeof(DothtmlElementNode));
            var parentElement = parentParentNode as DothtmlElementNode;
            Assert.AreEqual(parentElement.TagName, "a");
        }

        [TestMethod]
        public void DothtmlParser_HierarchyBuildingVisitor_Attribute_BindingValue()
        {
            var markup = "<!-- my comment --> @viewModel TestDirective\n<div><div><ul><li>item</li></ul><a href='{value: lol}'>link</a></div></div>";
            var root = ParseMarkup(markup);

            var visitor = new HierarchyBuildingVisitor
            {
                CursorPosition = 95
            };

            root.Accept(visitor);

            var cursorNode = visitor.LastFoundNode;
            var hierarchyList = visitor.GetHierarchy();

            Assert.AreEqual(8, hierarchyList.Count);

            Assert.IsInstanceOfType(cursorNode, typeof(DothtmlValueTextNode));
            var bindingValue = cursorNode as DothtmlValueTextNode;
            Assert.AreEqual(bindingValue.Text, "lol");
            Assert.AreEqual(bindingValue.WhitespacesBefore.Count(), 1);
            Assert.AreEqual(bindingValue.WhitespacesAfter.Count(), 0);

            Assert.IsInstanceOfType(bindingValue.ParentNode, typeof(DothtmlBindingNode));
            var binding = bindingValue.ParentNode as DothtmlBindingNode;
            Assert.AreEqual(binding.Name, "value");
            Assert.AreEqual(binding.Value, bindingValue.Text);

            Assert.IsInstanceOfType(binding.ParentNode, typeof(DothtmlValueBindingNode));
            var cursorValue = binding.ParentNode as DothtmlValueBindingNode;

            Assert.IsInstanceOfType(cursorValue.ParentNode, typeof(DothtmlAttributeNode));
            var parentAttribute = cursorValue.ParentNode as DothtmlAttributeNode;
            Assert.AreEqual(parentAttribute.AttributeName, "href");

            var parentParentNode = parentAttribute.ParentNode;
            Assert.IsInstanceOfType(parentParentNode, typeof(DothtmlElementNode));
            var parentElement = parentParentNode as DothtmlElementNode;
            Assert.AreEqual(parentElement.TagName, "a");
        }

        [TestMethod]
        public void DothtmlParser_EmptyText()
        {
            var markup = string.Empty;
            var root = ParseMarkup(markup);

            Assert.IsTrue(root.StartPosition == 0);
            Assert.IsTrue(root.Length == 0);
            Assert.IsTrue(root.Tokens.Count == 0);
            Assert.IsTrue(root.Content.Count == 0);
            Assert.IsTrue(root.Directives.Count == 0);
            Assert.IsTrue(root.Content.Count == 0);
        }

        [TestMethod]
        public void DothtmlParser_CompletelyUnclosedTag_WarningOnNode()
        {
            var markup = "<div><p>Something</div>";
            var root = ParseMarkup(markup);

            var pNode = root
                .Content[0].CastTo<DothtmlNodeWithContent>()
                .Content[0].CastTo<DothtmlElementNode>();

            Assert.AreEqual("p", pNode.TagName, "Tree is differen as expected, second tier should be p.");
            Assert.AreEqual(1, pNode.NodeWarnings.Count(), "There should have been a warning about unclosed element.");
            Assert.AreEqual(true, pNode.NodeWarnings.Any(w => w.Contains("implicitly closed")));
        }

        [TestMethod]
        public void DothtmlParser_UnclosedTagImplicitlyClosedEndOfFile_WarningOnNode()
        {
            var markup = "<div><p>";
            var root = ParseMarkup(markup);

            var pNode = root
                .Content[0].CastTo<DothtmlNodeWithContent>()
                .Content[0].CastTo<DothtmlElementNode>();

            Assert.AreEqual("p", pNode.TagName, "Tree is different as expected, second tier should be p.");
            Assert.AreEqual(1, pNode.NodeErrors.Count(), "There should have been an error about file ending");
            Assert.AreEqual(true, pNode.NodeErrors.Any(w => w.Contains("not closed")));
        }

        [TestMethod]
        public void DothtmlParser_UnclosedTagImplicitlyClosed_WarningOnNode()
        {
            var markup = "<div><p>Something<p>Something else</p></div>";
            var root = ParseMarkup(markup);

            var pNode = root
                .Content[0].CastTo<DothtmlNodeWithContent>()
                .Content[0].CastTo<DothtmlElementNode>();

            Assert.AreEqual("p", pNode.TagName, "Tree is different as expected, second tier should be p.");
            Assert.AreEqual(1, pNode.NodeWarnings.Count(), "There should have been a warning about implicitly closing p element.");
            Assert.AreEqual(true, pNode.NodeWarnings.Any(w=> w.Contains("implicitly closed")));
        }

        [TestMethod]
        public void DothtmlParser_BindingInnerInterpolatedExpression_BindingCorrectlyClosed()
        {
            var markup = "{{value: $'Hello {$'Innner {'Another interpolation'}'}'}}";
            var root = ParseMarkup(markup);

            Assert.AreEqual(1, root.Content.Count);
        }

        [TestMethod]
        public void DothtmlParser_AngleCharsInsideBinding()
        {
            var markup = "<div class-active='{value: Activity > 3 && Activity < 100}' />";
            var root = ParseMarkup(markup);

            var binding = root.EnumerateNodes().OfType<DothtmlBindingNode>().Single();

            Assert.AreEqual("Activity > 3 && Activity < 100", binding.Value);
        }

        [TestMethod]
        public void DothtmlParser_HtmlCommentInsideBinding()
        {
            var markup = "<div class-active='{value: \"<!-- comment -->\"}' />";
            var root = ParseMarkup(markup);

            var binding = root.EnumerateNodes().OfType<DothtmlBindingNode>().Single();

            Assert.AreEqual("\"<!-- comment -->\"", binding.Value);
        }

        public static DothtmlRootNode ParseMarkup(string markup)
        {
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(markup.Replace("\r\n", "\n"));
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);
            return node;
        }

    }
}
