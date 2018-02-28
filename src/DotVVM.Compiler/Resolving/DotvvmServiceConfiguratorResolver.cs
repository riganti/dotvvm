using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Compiler.Exceptions;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public class DotvvmServiceConfiguratorResolver : IDotvvmServiceConfiguratorResolver
    {
        private Type ResolveIDotvvmServiceConfiguratorClassType(Assembly assembly)
        {
            var interfaceType = typeof(IDotvvmServiceConfigurator);
            var resultTypes = assembly.GetLoadableTypes().Where(s => s.GetTypeInfo().ImplementedInterfaces.Any(i => i.Name == interfaceType.Name)).Where(s => s != null).ToList();
            if (resultTypes.Count > 1)
            {
                throw new ConfigurationInitializationException(
                    $"Assembly '{assembly.FullName}' contains more the one implementaion of IDotvvmServiceConfigurator.");
            }

            return resultTypes.Single();
        }

        private List<MethodInfo> ResolveConfigureServicesMethods(Type startupType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            methods.Add(startupType.GetMethod("ConfigureServices", new[] { typeof(IDotvvmServiceCollection) }));
            methods = methods.Where(s => s != null).ToList();

            if (!methods.Any())
            {
                throw new ConfigurationInitializationException("Missing method 'void ConfigureServices(IDotvvmServiceCollection serviceCollection)'.");
            }
            return methods;
        }

        public IServicesConfiguratorExecutor GetServiceConfiguratorExecutor(Assembly assembly)
        {
            var startupType = ResolveIDotvvmServiceConfiguratorClassType(assembly);
            if (startupType == null)
            {
                return new NoServicesConfiguratorExecutor();
            }
            var resolvedMethods = ResolveConfigureServicesMethods(startupType);

            return new ServicesConfiguratorExecutor(resolvedMethods.FirstOrDefault(), startupType);
        }

    }
}
