using System.Net.Http;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Samples.BasicSamples;
using DotVVM.Samples.BasicSamples.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand;
using DotVVM.Samples.Common.Api.AspNetCore;
using DotVVM.Samples.Common.Api.Owin;
using DotVVM.Samples.Common.Utilities;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.BindingVariables;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBack;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBackSpaNavigation;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.Common
{
    public static class CommonConfiguration
    {
        public static void ConfigureServices(IDotvvmServiceCollection dotvvmServices)
        {
            var services = dotvvmServices.Services;
            // normally, you'd put that to DotvvmStartup, but I need to test both options
            services.Configure<DotvvmMarkupConfiguration>(config => {
                // import namespaces
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace1", "TestNamespaceAlias"));
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace2"));
            });
            services.AddScoped<DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect.RedirectService>();
            services.Configure<DotvvmResourceRepository>(RegisterResources);

            services.Configure<JavascriptTranslatorConfiguration>(c => {
                c.MethodCollection.AddMethodTranslator(typeof(JavaScriptUtils),
                   nameof(JavaScriptUtils.LimitLength),
                   new GenericMethodCompiler((a) => new JsIdentifierExpression("limitLength").Invoke(a)));
            });

            dotvvmServices.AddDefaultTempStorages("Temp");
            services.AddScoped<ViewModelScopedDependency>();
            services.AddSingleton<IGreetingComputationService, HelloGreetingComputationService>();
            services.AddSingleton<FoodService>();

            services.AddSingleton<IViewModelServerStore, TestingInMemoryViewModelServerStore>();

            services.AddTransient<HttpClient>();
            services.AddSingleton<ResetClient>();
            services.AddSingleton<OrdersClient>();
            services.AddSingleton<CompaniesClient>();
            services.AddSingleton<TestWebApiClientOwin>();
            services.AddSingleton<TestWebApiClientAspNetCore>();

            services.AddSingleton<DenyPostbacksOnSpaNavigationService>();

            services.AddSingleton<IDiagnosticsInformationSender, TextFileDiagnosticsInformationSender>();
            services.AddTransient<VariablesStaticCommand>();

            services.AddSingleton<ViewModels.ControlSamples.NamedCommand.TestService>();
        }

        private static void RegisterResources(DotvvmResourceRepository resources)
        {
            resources.RegisterStylesheetFile("ControlSamples_SpaContentPlaceHolder_testCss", "Content/testResource.css");
            resources.RegisterScriptFile("ControlSamples_SpaContentPlaceHolder_testJs", "Scripts/testResource.js");
            resources.RegisterScriptFile("ControlSamples_SpaContentPlaceHolder_MasterPageResource", "Scripts/testResource2.js");

            resources.Register("FeatureSamples_Resources_CdnUnavailableResourceLoad", new ScriptResource() {
                Location = new UrlResourceLocation("~/nonexistentResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            resources.Register("FeatureSamples_Resources_CdnScriptPriority", new ScriptResource {
                Location = new UrlResourceLocation("/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            resources.Register("FeatureSamples_Resources_RequiredOnPostback", new ScriptResource() {
                Location = new UrlResourceLocation("~/nonexistentResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            resources.Register("Errors_InvalidLocationFallback", new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            resources.RegisterScriptModuleFile("FeatureSamples_Resources_TestViewModule", "Scripts/testViewModule.js");
            resources.RegisterScriptModuleFile("FeatureSamples_Resources_TestViewModule2", "Scripts/testViewModule2.js");
            resources.RegisterScriptModuleFile("FeatureSamples_Resources_TestViewModule3", "Scripts/testViewModule3.js");
            
            resources.Register("FeatureSamples_Resources_Incrementer", new ScriptModuleResource(new FileResourceLocation("~/Scripts/incrementerModule.js")));

            resources.RegisterScript("testJsModule", new InlineResourceLocation("export const commands = { myCommand() { console.info(\"Hello from page module\") } }"), module: true);

            resources.Register("ControlSamples_NamedCommand_ParameterStaticCommand", new ScriptModuleResource(new FileResourceLocation("~/Scripts/ControlSamples_NamedCommand_ParameterStaticCommand.js")));

            // resource that triggers the circular dependency check in the render phase
            var circular = new ScriptResource { Location = new FileResourceLocation("~/Scripts/testResource.js") };
            resources.Register("Errors_ResourceCircularDependency", circular);
            var circular2 = new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource2.js"),
                Dependencies = new [] { "Errors_ResourceCircularDependency" }
            };
            resources.Register("Errors_ResourceCircularDependency2", circular2);
            circular.Dependencies = new[] { "Errors_ResourceCircularDependency" };


            resources.Register("extenders", new ScriptResource(
                location: new FileResourceLocation("Scripts/ClientExtenders.js")
            ));

            resources.RegisterScriptFile(nameof(StopwatchPostbackHandler), $"~/Scripts/{nameof(StopwatchPostbackHandler)}.js", dependencies: new[] { "dotvvm" });
            resources.RegisterScriptFile(nameof(ErrorCountPostbackHandler), $"~/Scripts/{nameof(ErrorCountPostbackHandler)}.js", dependencies:  new[] { "dotvvm" });

            resources.RegisterScriptFile(nameof(PostBackHandlerCommandTypes), $"~/Scripts/{nameof(PostBackHandlerCommandTypes)}.js", dependencies: new [] { "dotvvm"});

            // dev files
            resources.SetEmbeddedResourceDebugFile("knockout", "../../Framework/Framework/Resources/Scripts/knockout-latest.debug.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.internal", "../../Framework/Framework/obj/javascript/root-only-debug/dotvvm-root.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.internal-spa", "../../Framework/Framework/obj/javascript/root-spa-debug/dotvvm-root.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.debug", "../../Framework/Framework/Resources/Scripts/DotVVM.Debug.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.fileupload-css", "../../Framework/Framework/Resources/Styles/DotVVM.FileUpload.css");
            resources.SetEmbeddedResourceDebugFile("dotvvm.internal-css", "../../Framework/Framework/Resources/Styles/DotVVM.Internal.css");
            resources.SetEmbeddedResourceDebugFile("dotvvm.polyfill.bundle", "../../Framework/Framework/obj/javascript/polyfill.bundle.js");
        }
    }
}
