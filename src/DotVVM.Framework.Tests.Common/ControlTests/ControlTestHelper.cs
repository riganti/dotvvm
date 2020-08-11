using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Utils;
using System.Text;
using System.Linq;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Html;
using System.Globalization;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    public class ControlTestHelper
    {
        private readonly DotvvmConfiguration configuration;
        private readonly FakeMarkupFileLoader fileLoader;
        private readonly DotvvmPresenter presenter;

        IControlBuilderFactory controlBuilderFactory => configuration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();

        public ControlTestHelper(bool debug = true)
        {
            fileLoader = new FakeMarkupFileLoader(null);
            this.configuration = DotvvmTestHelper.CreateConfiguration(services => {
                services.AddSingleton<IMarkupFileLoader>(fileLoader);
            });
            this.configuration.Markup.AddCodeControls("tc", exampleControl: typeof(FakeHeadResourceLink));
            this.configuration.ApplicationPhysicalPath = Path.GetTempPath();
            this.configuration.Debug = debug;
            presenter = (DotvvmPresenter)this.configuration.ServiceProvider.GetRequiredService<IDotvvmPresenter>();
        }

        private (ControlBuilderDescriptor descriptor, Lazy<IControlBuilder> builder) CompilePage(
            string markup,
            string fileName,
            Dictionary<string, string> markupFiles = null)
        {
            if (!fileLoader.MarkupFiles.TryAdd(fileName, markup))
                throw new Exception($"File {fileName} already exists");

            return controlBuilderFactory.GetControlBuilder(fileName);
        }

        private TestDotvvmRequestContext PrepareRequest(
            string markup,
            Dictionary<string, string> markupFiles = null,
            string fileName = null
        )
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            configuration.Freeze();
            fileName = (fileName ?? "testpage") + ".dothtml";
            var (_, controlBuilder) = CompilePage(markup, fileName, markupFiles);
            var context = DotvvmTestHelper.CreateContext(configuration);
            context.Route = new Framework.Routing.DotvvmRoute(
                "testpage",
                fileName,
                null,
                null,
                configuration);

            return context;
        }

        public async Task<PageRunResult> RunPage(
            Type viewModel,
            string markup,
            Dictionary<string, string> markupFiles = null,
            bool renderResources = false,
            [CallerMemberName] string fileName = null)
        {
            if (!markup.Contains("<body"))
            {
                markup = $"<body>\n{markup}\n{(renderResources ? "" : "<tc:FakeBodyResourceLink />")}\n</body>";
            }
            else if (!renderResources)
            {
                markup = "<tc:FakeBodyResourceLink />" + markup;
            }
            if (!markup.Contains("<head"))
            {
                markup = $"<head></head>\n{markup}";
            }
            if (!renderResources)
            {
                markup = "<tc:FakeHeadResourceLink />" + markup;
            }
            markup = $"@viewModel {viewModel.ToString().Replace("+", ".")}\n\n{markup}";
            var request = PrepareRequest(markup);
            await presenter.ProcessRequestCore(request);
            return CreatePageResult(request);
        }

        private PageRunResult CreatePageResult(TestDotvvmRequestContext context)
        {
            var htmlOutput = System.Text.Encoding.UTF8.GetString(context.HttpContext.CastTo<TestHttpContext>().Response.Body.ToArray());
            var headResources = context.View.GetAllDescendants().OfType<FakeHeadResourceLink>().FirstOrDefault()?.CapturedHtml;
            var bodyResources = context.View.GetAllDescendants().OfType<FakeBodyResourceLink>().FirstOrDefault()?.CapturedHtml;

            var p = new HtmlParser();
            var htmlDocument = p.ParseDocument(htmlOutput);
            return new PageRunResult(
                context.ViewModelJson,
                htmlOutput,
                headResources,
                bodyResources,
                htmlDocument
            );
        }
    }

    public class PageRunResult
    {
        public PageRunResult(JObject viewModel, string outputString, string headResources, string bodyResources, IHtmlDocument html)
        {
            this.ViewModel = viewModel;
            this.OutputString = outputString;
            this.HeadResources = headResources;
            this.BodyResources = bodyResources;
            this.Html = html;
        }

        public JObject ViewModel { get; }
        public string OutputString { get; }
        public string HeadResources { get; }
        public string BodyResources { get; }
        public IHtmlDocument Html { get; }
        public string FormattedHtml
        {
            get
            {
                var str = new StringWriter();
                Html.ToHtml(str, new PrettyMarkupFormatter() { Indentation = "\t", NewLine = "\n" });
                return str.ToString();
            }
        }
    }

    public class FakeBodyResourceLink : BodyResourceLinks
    {
        public string CapturedHtml { get; private set; }
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var str = new StringWriter();
            var fakeWriter = new HtmlWriter(str, context);
            base.RenderControl(fakeWriter, context);
            this.CapturedHtml = str.ToString();
        }
    }

    public class FakeHeadResourceLink : HeadResourceLinks
    {
        public string CapturedHtml { get; private set; }
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var str = new StringWriter();
            var fakeWriter = new HtmlWriter(str, context);
            base.RenderControl(fakeWriter, context);
            this.CapturedHtml = str.ToString();
        }
    }
}
