using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DotVVM.Framework.Tests.Parser.Dothtml.DothtmlParserTests;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    //I removed the method but soon will use it for visitor testing
    //[TestClass]
    //public class FindNodeTests
    //{
    //    [TestMethod]
    //    public void FindDotvvmNode_ElementName_1()
    //    {
    //        var m = ParseMarkup("<a>ahoj</a>");
    //        var h = m.FindHierarchyByPosition(2);

    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_ElementName_2()
    //    {
    //        var m = ParseMarkup("<a><b>ahoj<c/></b></a>");
    //        var h = m.FindHierarchyByPosition(13);

    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlElementNode);
    //        Assert.IsTrue(h[3] is DothtmlElementNode);
    //        Assert.IsTrue((h[3] as DothtmlElementNode).IsSelfClosingTag);
    //        Assert.AreEqual("c", (h[3] as DothtmlElementNode).TagName);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_ElementContent_1()
    //    {
    //        var m = ParseMarkup("<a>ahoj</a>");
    //        var h = m.FindHierarchyByPosition(5);

    //        Assert.AreEqual(3, h.Count);
    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlLiteralNode);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_AttributeName_1()
    //    {
    //        var m = ParseMarkup("<a a='b'>ahoj</a>");
    //        var h = m.FindHierarchyByPosition(3);

    //        Assert.AreEqual(3, h.Count);
    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlAttributeNode);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_AttributeValue_1()
    //    {
    //        var m = ParseMarkup("<a a='b'>ahoj</a>");
    //        var h = m.FindHierarchyByPosition(7);

    //        Assert.AreEqual(4, h.Count);
    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlAttributeNode);
    //        Assert.IsTrue(h[3] is DothtmlLiteralNode);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_AttributeBinding_1()
    //    {
    //        var m = ParseMarkup("<a a='{hu:hu}'>ahoj</a>");
    //        var h = m.FindHierarchyByPosition(7);

    //        Assert.AreEqual(4, h.Count);
    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlAttributeNode);
    //        Assert.IsTrue(h[3] is DothtmlBindingNode);
    //    }

    //    [TestMethod]
    //    public void FindDotvvmNode_BindingNode_1()
    //    {
    //        var m = ParseMarkup("<a>{{hu:ahoj}}</a>");
    //        var h = m.FindHierarchyByPosition(7);

    //        Assert.AreEqual(3, h.Count);
    //        Assert.AreSame(m, h[0]);
    //        Assert.IsTrue(h[1] is DothtmlElementNode);
    //        Assert.IsTrue(h[2] is DothtmlBindingNode);
    //    }
    //}
}
