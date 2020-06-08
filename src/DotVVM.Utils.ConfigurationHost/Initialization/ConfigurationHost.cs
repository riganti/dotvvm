using System;
using System.Reflection;
using DotVVM.Compiler.Resolving;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Utils.ConfigurationHost.Initialization
{
    public class ConfigurationHost
    {
        public  static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, Action< IServiceCollection> servicesRegistration)
        {
            var dotvvmStartup = new DotvvmStartupClassResolver().GetDotvvmStartupInstance(webSiteAssembly);
            var serviceConfiguratorExecutor = new DotvvmServiceConfiguratorResolver().GetServiceConfiguratorExecutor(webSiteAssembly);

            var config = DotvvmConfiguration.CreateDefault(services => {
                serviceConfiguratorExecutor.ConfigureServices(services);
                servicesRegistration?.Invoke(services);
            });

            config.ApplicationPhysicalPath = webSitePath;
            config.CompiledViewsAssemblies = null;

            //configure dotvvm startup
            dotvvmStartup?.Configure(config, webSitePath);

            return config;
        }

    }
}
