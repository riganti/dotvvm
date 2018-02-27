using System;
using System.Linq;
using System.Reflection;
using DotVVM.Compiler.Compilation;
using DotVVM.Compiler.Exceptions;
using DotVVM.Compiler.Fakes;
using DotVVM.Compiler.Resolving;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotVVM.Compiler.Initialization
{
    public class ConfigurationInitializer
    {
        internal static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, ViewStaticCompiler viewStaticCompiler, Action<IServiceCollection> additionalServices = null)
        {
            var dotvvmStartup = new DotvvmStartupClassResolver().GetDotvvmStartupInstance(webSiteAssembly);
            var startupClass = new DotvvmServiceConfiguratorResolver().GetServiceConfigureExecutor(webSiteAssembly);

            var config = DotvvmConfiguration.CreateDefault(services => {

                if (viewStaticCompiler != null)
                {
                    services.AddSingleton<ViewStaticCompiler>(viewStaticCompiler);
                    services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                    services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                    services.AddSingleton(new RefObjectSerializer());
                }

                startupClass.ConfigureServices(services);
                additionalServices?.Invoke(services);
            });

            config.ApplicationPhysicalPath = webSitePath;
            config.CompiledViewsAssemblies = null;

            //configure dotvvm startup
            dotvvmStartup?.Configure(config, webSitePath);

            return config;
        }

    }
}
