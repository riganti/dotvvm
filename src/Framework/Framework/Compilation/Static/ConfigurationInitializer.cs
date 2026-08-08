
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotVVM.Framework.Compilation.Static
{
    internal static class ConfigurationInitializer
    {
        public static DotvvmConfiguration GetConfiguration(
            Assembly webSiteAssembly,
            string webSitePath)
        {
            var dotvvmStartup = GetDotvvmStartup(webSiteAssembly);
            var configurator = GetDotvvmServiceConfigurator(webSiteAssembly);
            var configureServicesMethod = configurator is { }
                ? GetConfigureServicesMethod(configurator.GetType())
                : null;

            var config = DotvvmConfiguration.CreateInternal(collection => {
                if (configurator is { } && configureServicesMethod is { })
                {
                    configureServicesMethod.Invoke(configurator, new[] { new DotvvmServiceCollection(collection, isDotvvmCompiler: true) });
                }

                collection.Configure<LoggerFilterOptions>(options => options.Rules.Add(
                    new LoggerFilterRule(
                        providerName: null,
                        categoryName: typeof(DefaultViewCompiler).FullName,
                        logLevel: LogLevel.None,
                        filter: null)));
            });

            config.ApplicationPhysicalPath = webSitePath;
            dotvvmStartup?.Configure(config, webSitePath);
            // The standalone compiler explicitly compiles every view below. Prevent the application's
            // startup compilation configuration from scheduling the same work in the background.
            config.Markup.ViewCompilation.Mode = ViewCompilationMode.Lazy;
            return config;
        }

        public static IDotvvmStartup GetDotvvmStartup(Assembly assembly)
        {
            //find all implementations of IDotvvmStartup
            var dotvvmStartupType = GetDotvvmStartupType(assembly);
            if (dotvvmStartupType is null)
            {
                throw new ArgumentException("Could not find an implementation of IDotvvmStartup "
                    + $"in '{assembly.FullName}'.");
            }

            return dotvvmStartupType.Apply(Activator.CreateInstance)!.CastTo<IDotvvmStartup>();
        }

        private static Type? GetDotvvmStartupType(Assembly assembly)
        {
            var dotvvmStartups = assembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (dotvvmStartups.Length > 1)
            {
                var startupNames = string.Join(", ", dotvvmStartups.Select(s => $"'{s.Name}'"));
                throw new ArgumentException("Found more than one IDotvvmStartup implementation in "
                    + $"'{assembly.FullName}': {startupNames}.");
            }
            return dotvvmStartups.SingleOrDefault();
        }

        private static IDotvvmServiceConfigurator? GetDotvvmServiceConfigurator(Assembly assembly)
        {
            //find all implementations of IDotvvmServiceConfigurator
            var dotvvmServiceConfiguratorType = GetDotvvmServiceConfiguratorType(assembly);
            if (dotvvmServiceConfiguratorType is null)
            {
                throw new ArgumentException("Could not find an implementation of IDotvvmServiceConfigurator "
                    + $"in '{assembly.FullName}'.");
            }

            return dotvvmServiceConfiguratorType.Apply(Activator.CreateInstance)!.CastTo<IDotvvmServiceConfigurator>();

        }

        private static Type? GetDotvvmServiceConfiguratorType(Assembly assembly)
        {
            var interfaceType = typeof(IDotvvmServiceConfigurator);
            var resultTypes = assembly.GetLoadableTypes()
                .Where(s => s.GetTypeInfo().ImplementedInterfaces
                    .Any(i => i.Name == interfaceType.Name))
                    .Where(s => s != null)
                .ToArray();
            if (resultTypes.Length > 1)
            {
                throw new ArgumentException("Found more than one implementation of IDotvvmServiceConfigurator in "
                    + $"'{assembly.FullName}'.");
            }

            return resultTypes.SingleOrDefault();
        }

        private static MethodInfo GetConfigureServicesMethod(Type type)
        {
            var method = type.GetMethod("ConfigureServices", new[] { typeof(IDotvvmServiceCollection) });
            if (method == null)
            {
                throw new ArgumentException($"Type '{type}' is missing the "
                    + "'void ConfigureServices IDotvvmServiceCollection services)'.");
            }
            return method;
        }

        private static void InvokeConfigureServices(MethodInfo method, IServiceCollection collection)
        {
            if (method.IsStatic)
            {
                method.Invoke(null, new object[] { new DotvvmServiceCollection(collection) });
            }
            else
            {
                var instance = Activator.CreateInstance(method.DeclaringType!);
                method.Invoke(instance, new object[] { new DotvvmServiceCollection(collection) });
            }
        }
    }
}
