using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Utils.ConfigurationHost;
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

            return resultTypes.SingleOrDefault();
        }

        private MethodInfo ResolveConfigureServicesMethods(Type startupType)
        {
            var method = startupType.GetMethod("ConfigureServices", new[] {typeof(IDotvvmServiceCollection)});
           if (method == null)
            {
                throw new ConfigurationInitializationException("Missing method 'void ConfigureServices(IDotvvmServiceCollection services)'.");
            }
            return method;
        }

        public IServiceConfiguratorExecutor GetServiceConfiguratorExecutor(Assembly assembly)
        {
            var startupType = ResolveIDotvvmServiceConfiguratorClassType(assembly);
            if (startupType == null)
            {
                return new NoServiceConfiguratorExecutor();
            }
            var resolvedMethod = ResolveConfigureServicesMethods(startupType);

            return new ServiceConfiguratorExecutor(resolvedMethod, startupType);
        }

    }
}
