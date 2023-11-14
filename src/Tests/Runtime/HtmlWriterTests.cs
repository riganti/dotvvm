using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class HtmlWriterTests
    {
        string WriteHtml(Action<HtmlWriter> a)
        {
            var text = new StringWriter();
            a(new HtmlWriter(text, new TestDotvvmRequestContext() {
                Configuration = DotvvmTestHelper.DefaultConfig
            }));
            return text.ToString();
        }

        [TestMethod]
        public void HtmlWriter_EmptyKnockoutGroup()
        {
            var text = WriteHtml(a => {
                a.AddKnockoutDataBind("a", new KnockoutBindingGroup());
                a.RenderSelfClosingTag("b");
            });
            Assert.AreEqual("<b data-bind=\"a: {}\" />", text);
        }

        [TestMethod]
        public void ImgTagWithChildren()
        {
            var text = WriteHtml(a => {
                a.RenderBeginTag("img");
                a.RenderBeginTag("div");
                a.RenderEndTag();
                a.RenderEndTag();
            });
            Assert.AreEqual("<img><div></div></img>", text);
        }

        [TestMethod]
        public void EscapingAmpersandStringEnd()
        {
            var text = WriteHtml(a => {
                a.AddAttribute("a", "&");
                a.AddAttribute("b", "abc &");
                a.RenderSelfClosingTag("img");
            });
            Assert.AreEqual("<img a=\"&amp;\" b=\"abc &amp;\" />", text);
        }

        [TestMethod]
        public void EscapingAmpersandAllowedUnescaped()
        {
            var text = WriteHtml(a => {
                a.AddAttribute("a", "a & b");
                a.AddAttribute("b", "a && b");
                a.RenderSelfClosingTag("img");
            });
            Assert.AreEqual("<img a=\"a & b\" b=\"a && b\" />", text);
        }

        [TestMethod]
        public void EscapingAmpersandUnallowed()
        {
            var text = WriteHtml(a => {
                a.AddAttribute("a", "&amp;");
                a.AddAttribute("b", "a&b");
                a.RenderSelfClosingTag("img");
            });
            Assert.AreEqual("<img a=\"&amp;amp;\" b=\"a&amp;b\" />", text);
        }
    }
}
