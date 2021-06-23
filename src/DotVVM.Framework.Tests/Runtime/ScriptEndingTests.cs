using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ResourceScriptInjectionTests
    {
        // tests various forms of `</script>` inserted into a inline script or template
        // it can't tests that it's not XSS proof, but can at least stop someone from accidentally removing these checks

        [DataTestMethod]
        [DataRow("</script>   ")]
        [DataRow("djsfkjdsfksdhfk</script  ")]
        [DataRow("fjhdsfkjdskjfh</scrip</SCriPT  ")]
        [DataRow("</sc</script>ript>")] // none of these is somehow special, just want to take a bit more of them
        public void TestResources(string forbiddenString)
        {
            var cx = DotvvmTestHelper.CreateContext(DotvvmTestHelper.DefaultConfig);
            var output = new StringWriter();
            var writer = new HtmlWriter(output, cx);

            var template = new TemplateResource(forbiddenString);
            var inlineScript = new InlineScriptResource(forbiddenString);

            template.Render(writer, cx, "a");
            inlineScript.Render(writer, cx, "b");
            Assert.IsFalse(output.ToString().Contains(forbiddenString));
        }

        [DataTestMethod]
        [DataRow("</style>")]
        [DataRow("djsfkjdsfksdhfk</style  ")]
        [DataRow("fjhdsfkjdskjfh</styl</STylE  ")]
        [DataRow("</st</style>yle>")]
        public void TestStyleResources(string forbiddenString)
        {
            Assert.ThrowsException<Exception>(() => new InlineStylesheetResource(forbiddenString));
        }

        [DataTestMethod]
        [DataRow("<div")]
        [DataRow("div>")]
        [DataRow("&#x200B;")]
        public void MustEncodeUrls(string forbiddenString)
        {
            var resources = new IResource [] {
                new ScriptResource(new UrlResourceLocation("http://server.com/" + forbiddenString + "somethingelse")),
                new StylesheetResource(new UrlResourceLocation("file:///" + forbiddenString)) {
                    LocationFallback = new ResourceLocationFallback("true", new UrlResourceLocation(forbiddenString), new UrlResourceLocation("http://" + forbiddenString))
                }
            };

            var cx = DotvvmTestHelper.CreateContext(DotvvmTestHelper.DefaultConfig);
            var output = new StringWriter();
            var w = new HtmlWriter(output, cx);
            foreach (var a in resources)
                a.Render(w, cx, forbiddenString);

            Assert.IsFalse(output.ToString().Contains(forbiddenString));
        }
    }
}
