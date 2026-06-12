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

        [TestMethod]
        public void AttributeMerging_SameCasingAppend()
        {
            // Two "class" attributes (same casing) with append=true should be joined with a space
            var text = WriteHtml(a => {
                a.AddAttribute("class", "foo", append: true);
                a.AddAttribute("class", "bar", append: true);
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div class=\"foo bar\"></div>", text);
        }

        [TestMethod]
        public void AttributeMerging_DifferentCasingAppend()
        {
            // "class" and "Class" (different casing) with append=true must be treated as the same attribute and joined
            var text = WriteHtml(a => {
                a.AddAttribute("class", "foo", append: true);
                a.AddAttribute("Class", "bar", append: true);
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div class=\"foo bar\"></div>", text);
        }

        [TestMethod]
        public void AttributeMerging_DifferentCasingAppend_ThreeVariants()
        {
            // All three casing variants of "class" must collapse into one joined attribute
            var text = WriteHtml(a => {
                a.AddAttribute("class", "a", append: true);
                a.AddAttribute("CLASS", "b", append: true);
                a.AddAttribute("Class", "c", append: true);
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div class=\"a b c\"></div>", text);
        }

        [TestMethod]
        public void AttributeMerging_DifferentCasingOverwrite()
        {
            // When append=false (overwrite), the last value for the case-insensitive attribute name wins
            var text = WriteHtml(a => {
                a.AddAttribute("class", "first", append: false);
                a.AddAttribute("Class", "second", append: false);
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div class=second></div>", text);
        }

        [TestMethod]
        public void AttributeMerging_DifferentCasingStyle()
        {
            // "style" and "Style" (different casing) with append=true must be joined with ";"
            var text = WriteHtml(a => {
                a.AddAttribute("style", "color:red", append: true, appendSeparator: ";");
                a.AddAttribute("Style", "font-size:12px", append: true, appendSeparator: ";");
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div style=\"color:red;font-size:12px\"></div>", text);
        }

        [TestMethod]
        public void AttributeMerging_DifferentCasingNonAppendedAndAppended()
        {
            // mix of overwrite and append with different casing: first sets "foo", second appends "bar"
            var text = WriteHtml(a => {
                a.AddAttribute("class", "foo", append: false);
                a.AddAttribute("Class", "bar", append: true);
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual("<div class=\"foo bar\"></div>", text);
        }
    }
}
