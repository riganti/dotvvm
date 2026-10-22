using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AngleSharp.Html.Parser;
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

        void AssertInvalidHtmlName(string name)
        {
            var text = WriteHtml(a => {
                Assert.ThrowsException<ArgumentException>(() => a.RenderBeginTag(name));
                Assert.ThrowsException<ArgumentException>(() => a.RenderSelfClosingTag(name));
                Assert.ThrowsException<ArgumentException>(() => a.WriteHtmlAttribute(name, "value"));
                Assert.ThrowsException<ArgumentException>(() => a.WriteHtmlAttribute(name, ""));
                Assert.ThrowsException<ArgumentException>(() => a.WriteHtmlAttribute(name, null));
            });
            Assert.AreEqual("", text, "An invalid name must be rejected before any part of it is written.");
        }

        [TestMethod]
        public void HtmlNames_RejectEmptyNames()
        {
            AssertInvalidHtmlName("");
        }

        // https://html.spec.whatwg.org/multipage/parsing.html#attribute-name-state
        // https://html.spec.whatwg.org/multipage/parsing.html#tag-name-state
        [DataTestMethod]
        [DataRow('\0')]
        [DataRow('\t')]
        [DataRow('\n')]
        [DataRow('\f')]
        [DataRow('\r')]
        [DataRow(' ')]
        [DataRow('/')]
        [DataRow('>')]
        [DataRow('=')]
        [DataRow('"')]
        [DataRow('\'')]
        [DataRow('<')]
        [DataRow('&')]
        public void HtmlNames_RejectSyntaxCharacters(char character)
        {
            AssertInvalidHtmlName(character + "name");
            AssertInvalidHtmlName("na" + character + "me");
            AssertInvalidHtmlName("name" + character);
        }

        [DataTestMethod]
        [DataRow("title><img src=x onerror=alert(1)>")]
        [DataRow("title onmouseover=alert(1)")]
        [DataRow("title/onmouseover=alert(1)")]
        [DataRow("title=ignored onmouseover=alert(1)")]
        [DataRow("title\r\nonmouseover=alert(1)")]
        public void HtmlAttributes_RejectNameInjectionInAllMergePaths(string name)
        {
            AssertInvalidHtmlName("data-" + name);
        }

        [DataTestMethod]
        [DataRow(":viewBox")]
        [DataRow("data+name")]
        [DataRow("aria-label")]
        [DataRow("xmlns:xlink")]
        [DataRow("name_0.test@[]`")]
        [DataRow("0name")]
        [DataRow("žluťoučký")]
        public void HtmlNames_Valid(string name)
        {
            var tag = WriteHtml(a => {
                a.RenderBeginTag(name);
                a.RenderEndTag();
                a.RenderSelfClosingTag(name);
            });
            Assert.AreEqual($"<{name}></{name}><{name} />", tag);

            var attribute = WriteHtml(a => {
                a.AddAttribute(name, "value");
                a.RenderBeginTag("div");
                a.RenderEndTag();
            });
            Assert.AreEqual($"<div {name}=value></div>", attribute);

            var element = new HtmlParser().ParseDocument(attribute).Body.FirstElementChild;
            Assert.AreEqual(1, element.Attributes.Length);
            Assert.AreEqual("value", element.GetAttribute(name));
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
        public void KnockoutBindings_RejectDeferredNullArguments()
        {
            void Throws(string parameterName, Action action) =>
                Assert.AreEqual(parameterName, Assert.ThrowsException<ArgumentNullException>(action).ParamName);

            var group = new KnockoutBindingGroup();
            Throws("name", () => group.Add(null!, "value"));
            Throws("expression", () => group.Add("name", (string)null!));
            Throws("name", () => new KnockoutBindingGroup.KnockoutBindingInfo(null!, "value"));
            Throws("expression", () => WriteHtml(a => a.AddKnockoutDataBind("name", (string)null!)));
            Throws("bindingGroup", () => WriteHtml(a => a.AddKnockoutDataBind("name", (KnockoutBindingGroup)null!)));
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
