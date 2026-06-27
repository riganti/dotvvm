using System.IO;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
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

        [TestMethod]
        public void ResourceRenderer_ExpandsVirtualScriptAndStylesheetUrls()
        {
            var script = new NamedResource("test-script", new ScriptResource(new UrlResourceLocation("~/Scripts/test.js")));
            var stylesheet = new NamedResource("test-stylesheet", new StylesheetResource(new UrlResourceLocation("~/Styles/test.css")));
            var firstContext = DotvvmTestHelper.CreateContext();
            ((TestHttpContext)firstContext.HttpContext).Request.PathBase = "";
            var secondContext = DotvvmTestHelper.CreateContext();
            ((TestHttpContext)secondContext.HttpContext).Request.PathBase = "/app";

            var firstScriptOutput = script.RenderToString(firstContext);
            var secondScriptOutput = script.RenderToString(secondContext);
            var firstStylesheetOutput = stylesheet.RenderToString(firstContext);
            var secondStylesheetOutput = stylesheet.RenderToString(secondContext);

            Assert.IsFalse(firstScriptOutput.Contains("~/"));
            Assert.IsFalse(secondScriptOutput.Contains("~/"));
            Assert.IsFalse(firstStylesheetOutput.Contains("~/"));
            Assert.IsFalse(secondStylesheetOutput.Contains("~/"));
            StringAssert.Contains(firstScriptOutput, "src=/Scripts/test.js");
            StringAssert.Contains(secondScriptOutput, "src=/app/Scripts/test.js");
            StringAssert.Contains(firstStylesheetOutput, "href=/Styles/test.css");
            StringAssert.Contains(secondStylesheetOutput, "href=/app/Styles/test.css");
        }

        [TestMethod]
        public void ResourceRenderer_ExpandsVirtualPreloadUrls()
        {
            var resourceManager = new ResourceManager(DotvvmTestHelper.DefaultConfig.Resources);
            var context = DotvvmTestHelper.CreateContext();
            ((TestHttpContext)context.HttpContext).Request.PathBase = "/app";
#pragma warning disable CS0618 // Test the preload path for resources rendered in the body.
            var script = new NamedResource("test-script", new ScriptResource(new UrlResourceLocation("~/Scripts/test.js"), defer: false));
            var scriptModule = new NamedResource("test-module", new ScriptModuleResource(new UrlResourceLocation("~/Modules/test.js"), defer: false));
#pragma warning restore CS0618

            using var text = new StringWriter();
            var writer = new HtmlWriter(text, context);
            ResourcesRenderer.RenderResources(resourceManager, [ script, scriptModule ], writer, context, ResourceRenderPosition.Head);
            var output = text.ToString();

            Assert.IsFalse(output.Contains("~/"));
            StringAssert.Contains(output, "href=/app/Scripts/test.js");
            StringAssert.Contains(output, "href=/app/Modules/test.js");
        }

        [TestMethod]
        public void LocalResourceLocation_CachedFileResourceUrlReturnsVirtualUrl()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.Runtime.AllowResourceVersionHash.Disable();
            configuration.Freeze();
            var resource = new FileResourceLocation("~/Scripts/test.js");
            var firstContext = DotvvmTestHelper.CreateContext(configuration);
            ((TestHttpContext)firstContext.HttpContext).Request.PathBase = "";
            var secondContext = DotvvmTestHelper.CreateContext(configuration);
            ((TestHttpContext)secondContext.HttpContext).Request.PathBase = "/app";

            var firstUrl = ((IResourceLocation)resource).GetUrl(firstContext, "test");
            var secondUrl = ((IResourceLocation)resource).GetUrl(secondContext, "test");
            var virtualUrl = firstContext.Services.GetRequiredService<ILocalResourceUrlManager>().GetResourceUrl(resource, firstContext, "test");
            var renderedScript = new NamedResource("test", new ScriptResource(resource)).RenderToString(secondContext);

            Assert.AreEqual("~/_dotvvm/resource-test/test", firstUrl);
            Assert.AreEqual("~/_dotvvm/resource-test/test", secondUrl);
            Assert.AreEqual("~/_dotvvm/resource-test/test", virtualUrl);
            StringAssert.Contains(renderedScript, "src=/app/_dotvvm/resource-test/test");
        }

        [TestMethod]
        public void LocalResourceLocation_CachedEmbeddedResourceUrlWithHashReturnsVirtualUrl()
        {
            var resource = new EmbeddedResourceLocation(
                typeof(DotvvmConfiguration).Assembly,
                "DotVVM.Framework.Resources.Scripts.knockout-latest.js");
            var firstContext = DotvvmTestHelper.CreateContext();
            ((TestHttpContext)firstContext.HttpContext).Request.PathBase = "";
            var secondContext = DotvvmTestHelper.CreateContext();
            ((TestHttpContext)secondContext.HttpContext).Request.PathBase = "/app";

            var firstUrl = ((IResourceLocation)resource).GetUrl(firstContext, "knockout");
            var secondUrl = ((IResourceLocation)resource).GetUrl(secondContext, "knockout");
            var virtualUrl = firstContext.Services.GetRequiredService<ILocalResourceUrlManager>().GetResourceUrl(resource, firstContext, "knockout");
            var renderedScript = new NamedResource("knockout", new ScriptResource(resource)).RenderToString(secondContext);

            StringAssert.StartsWith(firstUrl, "~/_dotvvm/resource-knockout/knockout?v=");
            StringAssert.StartsWith(secondUrl, "~/_dotvvm/resource-knockout/knockout?v=");
            StringAssert.StartsWith(virtualUrl, "~/_dotvvm/resource-knockout/knockout?v=");
            StringAssert.Contains(renderedScript, "src=\"/app/_dotvvm/resource-knockout/knockout?v=");
        }
    }
}
