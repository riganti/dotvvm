using System;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Light
{
    class AspNetCoreInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, Action<DotvvmConfiguration, IServiceCollection> registerServices)
        {
            var dotvvmStartups = webSiteAssembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && TypeExtensions.GetConstructor(t, Type.EmptyTypes) != null).ToArray();

            if (dotvvmStartups.Length == 0) throw new Exception("Could not find any implementation of IDotvvmStartup.");
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");

            var startup = (IDotvvmStartup)Activator.CreateInstance(dotvvmStartups[0]);
            IServiceCollection serviceCollection = null;
            var config = DotvvmConfiguration.CreateDefault(
                services =>
                {
                    serviceCollection = services;
                });
            registerServices(config, serviceCollection);
            config.ApplicationPhysicalPath = webSitePath;
            startup.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;
            return config;
        }
    }
}