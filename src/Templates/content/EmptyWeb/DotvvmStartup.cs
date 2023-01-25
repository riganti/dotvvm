using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotvvmApplication1;
public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
{
    // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
    public void Configure(DotvvmConfiguration config, string applicationPath)
    {
        ConfigureRoutes(config, applicationPath);
        ConfigureControls(config, applicationPath);
        ConfigureResources(config, applicationPath);

        // https://www.dotvvm.com/docs/4.0/pages/concepts/configuration/explicit-assembly-loading
        config.ExperimentalFeatures.ExplicitAssemblyLoading.Enable();

        // Use this for command heavy applications
        // - DotVVM will store the viewmodels on the server, and client will only have to send back diffs
        // https://www.dotvvm.com/docs/4.0/pages/concepts/viewmodels/server-side-viewmodel-cache
        // config.ExperimentalFeatures.ServerSideViewModelCache.EnableForAllRoutes();

        // Use this if you are deploying to containers or slots
        //  - DotVVM will precompile all views before it appears as ready
        // https://www.dotvvm.com/docs/4.0/pages/concepts/configuration/view-compilation-modes
        // config.Markup.ViewCompilation.Mode = ViewCompilationMode.DuringApplicationStart;
    }

    private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
    {
        config.RouteTable.Add("Default", "", "Views/Default/default.dothtml");
        config.RouteTable.Add("Error", "error", "Views/Error/error.dothtml");

        // Uncomment the following line to auto-register all dothtml files in the Views folder
        // config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));    
    }

    private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
    {
        // register code-only controls and markup controls
    }

    private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
    {
        // register custom resources and adjust paths to the built-in resources
    }

    public void ConfigureServices(IDotvvmServiceCollection options)
    {
        //register only services that are supported by DotVVM (otherwise, register your services in Startup.cs)
        options.AddDefaultTempStorages("Temp");
    }
}
