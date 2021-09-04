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
                Configuration = DotvvmTestHelper.CreateConfiguration()
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
    }
}
