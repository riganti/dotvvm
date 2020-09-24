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
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Properties;
using Newtonsoft.Json;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    public class ControlTestHelper
    {
        private readonly DotvvmConfiguration configuration;
        private readonly FakeMarkupFileLoader fileLoader;
        private readonly DotvvmPresenter presenter;

        IControlBuilderFactory controlBuilderFactory => configuration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();

        public ControlTestHelper(bool debug = true, Action<DotvvmConfiguration> config = null, Action<IServiceCollection> services = null)
        {
            fileLoader = new FakeMarkupFileLoader(null);
            this.configuration = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IMarkupFileLoader>(fileLoader);
                services?.Invoke(s);
            });
            this.configuration.Markup.AddCodeControls("tc", exampleControl: typeof(FakeHeadResourceLink));
            this.configuration.ApplicationPhysicalPath = Path.GetTempPath();
            this.configuration.Debug = debug;
            config?.Invoke(this.configuration);
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
            string fileName,
            PostbackRequestModel postback = null
        )
        {
            var context = DotvvmTestHelper.CreateContext(configuration);
            context.CsrfToken = null;
            context.Route = new Framework.Routing.DotvvmRoute(
                "testpage",
                fileName,
                null,
                null,
                configuration);
            var httpContext = (TestHttpContext)context.HttpContext;

            if (postback is object)
            {
                httpContext.Request.Method = "POST";
                httpContext.Request.Headers["X-DotVVM-PostBack"] = new[] { "true" };
                httpContext.Request.Body = new MemoryStream(
                    new UTF8Encoding(false).GetBytes(
                        JsonConvert.SerializeObject(postback)
                    )
                );
            }

            return context;
        }

        private TestDotvvmRequestContext PreparePage(
            string markup,
            Dictionary<string, string> markupFiles,
            string fileName
        )
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            configuration.Freeze();
            fileName = (fileName ?? "testpage") + ".dothtml";
            var (_, controlBuilder) = CompilePage(markup, fileName, markupFiles);
            return PrepareRequest(fileName);
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
            var request = PreparePage(markup, markupFiles, fileName);
            await presenter.ProcessRequest(request);
            return CreatePageResult(request);
        }

        public async Task<CommandRunResult> RunCommand(
            string filePath,
            PostbackRequestModel model)
        {
            var request = PrepareRequest(filePath, model);
            await presenter.ProcessRequest(request);
            return CreateCommandResult(request);
        }

        private CommandRunResult CreateCommandResult(TestDotvvmRequestContext request)
        {
            return new CommandRunResult(
                request.ViewModelJson
            );
        }

        private IEnumerable<(DotvvmControl, DotvvmProperty, ICommandBinding)> FindCommands(DotvvmControl view) =>
            from control in view.GetThisAndAllDescendants()
            from property in control.Properties
            let binding = property.Value as ICommandBinding
            where binding != null
            select (view, property.Key, binding);

        private PageRunResult CreatePageResult(TestDotvvmRequestContext context)
        {
            var htmlOutput = System.Text.Encoding.UTF8.GetString(context.HttpContext.CastTo<TestHttpContext>().Response.Body.ToArray());
            var commands = FindCommands(context.View).ToArray();
            var headResources = context.View.GetAllDescendants().OfType<FakeHeadResourceLink>().FirstOrDefault()?.CapturedHtml;
            var bodyResources = context.View.GetAllDescendants().OfType<FakeBodyResourceLink>().FirstOrDefault()?.CapturedHtml;

            var p = new HtmlParser();
            var htmlDocument = p.ParseDocument(htmlOutput);
            return new PageRunResult(
                this,
                context.Route.VirtualPath,
                context.ViewModelJson,
                htmlOutput,
                headResources,
                bodyResources,
                htmlDocument,
                commands
            );
        }
    }

    public class CommandRunResult
    {
        public CommandRunResult(JObject resultJson)
        {
            this.ResultJson = resultJson;

        }
        public JObject ResultJson { get; }
        public JObject ViewModelJson => ResultJson["viewModel"] as JObject ?? ResultJson["viewModelDiff"] as JObject;
    }

    public class PostbackRequestModel
    {
        public PostbackRequestModel(
            JObject viewModel,
            string[] currentPath,
            string command,
            string controlUniqueId,
            object[] commandArgs,
            string validationTargetPath
        )
        {
            ViewModel = viewModel;
            CurrentPath = currentPath;
            Command = command;
            ControlUniqueId = controlUniqueId;
            CommandArgs = commandArgs;
            AdditionalData = new AdditionalDataClass(validationTargetPath);
        }

        [JsonProperty("viewModel")]
        public JObject ViewModel { get; }
        [JsonProperty("currentPath")]
        public string[] CurrentPath { get; }
        [JsonProperty("command")]
        public string Command { get; }
        [JsonProperty("controlUniqueId")]
        public string ControlUniqueId { get; }
        [JsonProperty("commandArgs")]
        public object[] CommandArgs { get; }
        [JsonProperty("additionalData")]
        public AdditionalDataClass AdditionalData { get; }

        public class AdditionalDataClass
        {
            public AdditionalDataClass(string validationTargetPath)
            {
                this.ValidationTargetPath = validationTargetPath;
            }
            [JsonProperty("validationTargetPath")]
            public string ValidationTargetPath { get; }
        }
    }

    public class PageRunResult
    {
        public PageRunResult(
            ControlTestHelper testHelper,
            string filePath,
            JObject resultJson,
            string outputString,
            string headResources,
            string bodyResources,
            IHtmlDocument html,
            (DotvvmControl, DotvvmProperty, ICommandBinding)[] commands
        )
        {
            TestHelper = testHelper;
            FilePath = filePath;
            this.ResultJson = resultJson;
            this.OutputString = outputString;
            this.HeadResources = headResources;
            this.BodyResources = bodyResources;
            this.Html = html;
            this.Commands = commands;
        }

        public ControlTestHelper TestHelper { get; }
        public string FilePath { get; }
        public JObject ResultJson { get; }
        public JObject ViewModelJson => (JObject)ResultJson["viewModel"];
        public dynamic ViewModel => ViewModelJson;
        public string OutputString { get; }
        public string HeadResources { get; }
        public string BodyResources { get; }
        public IHtmlDocument Html { get; }
        public (DotvvmControl control, DotvvmProperty property, ICommandBinding command)[] Commands { get; }

        public string FormattedHtml
        {
            get
            {
                var str = new StringWriter();
                Html.ToHtml(str, new PrettyMarkupFormatter() { Indentation = "\t", NewLine = "\n" });
                return str.ToString();
            }
        }

        public (DotvvmControl, DotvvmProperty, ICommandBinding) FindCommand(string text, object viewModel = null)
        {
            var filtered =
                this.Commands
                    .Where(c => c.command.GetProperty<OriginalStringBindingProperty>(ErrorHandlingMode.ReturnNull)?.Code?.Trim() == text.Trim()
                             && (viewModel is null || viewModel.Equals(c.control.DataContext)))
                    .ToArray();
            if (filtered.Length == 0)
                throw new Exception($"Command '{text}' was not found" + (viewModel is null ? "" : $" on viewModel={viewModel}"));
            if (filtered.Length > 1)
                throw new Exception($"Multiple commands '{text}' were found" + (viewModel is null ? "" : $" on viewModel={viewModel}") + $": " + string.Join(", ", filtered.Select(c => c.command)));

            return filtered.Single();
        }

        public async Task<CommandRunResult> RunCommand(string text, object viewModel = null, bool applyChanges = true, object[] args = null)
        {
            var (control, property, binding) = FindCommand(text, viewModel);
            if (binding is CommandBindingExpression command)
            {
                var path = control
                    .GetAllAncestors(true)
                    .Select(a => a.GetDataContextPathFragment())
                    .Where(x => x != null)
                    .Reverse()
                    .ToArray();
                var viewModelJson = this.ViewModelJson; // TODO: process as on client-side
                var r = await this.TestHelper.RunCommand(this.FilePath, new PostbackRequestModel(
                    viewModelJson,
                    path,
                    command.BindingId,
                    null,
                    args ?? new object[0],
                    KnockoutHelper.GetValidationTargetExpression(control)
                ));

                if (applyChanges)
                {
                    JsonUtils.Patch(
                        (JObject)this.ResultJson["viewModel"],
                        r.ViewModelJson
                    );
                }
                return r;
            }
            else
            {
                throw new NotSupportedException($"{binding} is not supported.");
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

            ResourcesRenderer.RenderResources(
                context.ResourceManager.GetNamedResourcesInOrder().Where(r => r.Resource is TemplateResource),
                writer, context, ResourceRenderPosition.Body);
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
