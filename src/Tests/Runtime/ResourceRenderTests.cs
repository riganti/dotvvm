using System.IO;
using CheckTestOutput;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ResourceRenderTests
    {
        private readonly OutputChecker check = new OutputChecker("testoutputs");

        private static string Render(LinkResourceBase resource, string resourceName = "r")
        {
            var context = DotvvmTestHelper.CreateContext();

            using var text = new StringWriter();
            var html = new HtmlWriter(text, context);
            resource.Render(html, context, resourceName);
            return text.ToString();
        }

        [TestMethod]
        public void Script_WithFallback_OnlyPrimaryHasFetchPriority()
        {
            var res = new ScriptResource(new UrlResourceLocation("http://primary.example.com/app.js")) {
                FetchPriority = ResourceFetchPriority.High,
                LocationFallback = new ResourceLocationFallback(
                    javascriptCondition: "true",
                    new UrlResourceLocation("http://fallback.example.com/app.js")
                )
            };

            var output = Render(res);
            check.CheckString(output, fileExtension: "html");
        }

        [TestMethod]
        public void ScriptModule_FetchPriority_High()
        {
            var res = new ScriptModuleResource(new UrlResourceLocation("http://example.com/module.js")) {
                FetchPriority = ResourceFetchPriority.Low
            };

            var output = Render(res);
            check.CheckString(output, fileExtension: "html");
        }

        [TestMethod]
        public void Stylesheet_FetchPriority_High()
        {
            var res = new StylesheetResource(new UrlResourceLocation("http://example.com/style.css")) {
                FetchPriority = ResourceFetchPriority.High
            };

            var output = Render(res);
            check.CheckString(output, fileExtension: "html");
        }
    }
}
