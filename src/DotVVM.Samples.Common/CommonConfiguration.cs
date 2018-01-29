using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.Common
{
    public static class CommonConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // normally, you'd put that to DotvvmStartup, but I need to test both options
            services.Configure<DotvvmMarkupConfiguration>(config => {
                // import namespaces
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace1", "TestNamespaceAlias"));
                config.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace2"));
            });

            services.Configure<DotvvmResourceRepository>(RegisterResources);
        }

        private static void RegisterResources(DotvvmResourceRepository resources)
        {
            resources.Register("ControlSamples_SpaContentPlaceHolder_testCss", new StylesheetResource(new FileResourceLocation("Content/testResource.css")));
            resources.Register("ControlSamples_SpaContentPlaceHolder_testJs", new ScriptResource(new FileResourceLocation("Scripts/testResource.js")));
            resources.Register("ControlSamples_SpaContentPlaceHolder_MasterPageResource", new ScriptResource(new FileResourceLocation("Scripts/testResource2.js")));

            resources.Register("FeatureSamples_Resources_CdnUnavailableResourceLoad", new ScriptResource() {
                Location = new UrlResourceLocation("http://unavailable.local/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            resources.Register("FeatureSamples_Resources_CdnScriptPriority", new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            resources.Register("extenders", new ScriptResource
            {
                Location = new FileResourceLocation("Scripts/ClientExtenders.js")
            });

            // dev files
            resources.SetEmbeddedResourceDebugFile("dotvvm.internal", "../DotVVM.Framework/Resources/Scripts/DotVVM.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.debug", "../DotVVM.Framework/Resources/Scripts/DotVVM.Debug.js");
            resources.SetEmbeddedResourceDebugFile("dotvvm.fileupload-css", "../DotVVM.Framework/Resources/Scripts/DotVVM.FileUploads.css");

            // test debug version of knockout
            ((ScriptResource)resources.FindResource("knockout"))
               .Location = new FileResourceLocation("../DotVVM.Framework/Resources/Scripts/knockout-latest.debug.js");

        }
    }
}