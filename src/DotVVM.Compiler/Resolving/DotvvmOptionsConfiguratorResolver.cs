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
    public class DotvvmOptionsConfiguratorResolver : IStartupClassResolver
    {
        private Type ResolveOwinStartupClassType(Assembly assembly)
        {
            var interfaceType = typeof(IDotvvmServiceConfigurator);
            var resultTypes = assembly.GetLoadableTypes().Where(s => s.GetTypeInfo().ImplementedInterfaces.Any(i => i.Name == interfaceType.Name)).Where(s => s != null).ToList();
            if (resultTypes.Count > 1)
            {
                throw new ConfigurationInitializationException(
                    $"Assembly '{assembly.FullName}' contains more the one implementaion of IDotvvmServiceConfigurator.");
            }

            return resultTypes.FirstOrDefault();
        }

        private List<MethodInfo> ResolveConfigureServicesMethods(Type startupType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            methods.Add(startupType.GetMethod("ConfigureServices", new[] { typeof(IDotvvmOptions) }));
            methods = methods.Where(s => s != null).ToList();

            if (!methods.Any())
            {
                throw new ConfigurationInitializationException("Method void ConfigureServices(IDotvvmOptions options). Please follow docs https://www.dotvvm.com/docs/owin-compiler.");
            }
            return methods;
        }

        public IServicesStartupClassExecutor GetServiceConfigureExecutor(Assembly assembly)
        {
            var startupType = ResolveOwinStartupClassType(assembly);
            if (startupType == null)
            {
                return new NoServicesStartupClassExecutor();
            }
            var resolvedMethods = ResolveConfigureServicesMethods(startupType);

            return new ServicesStartupClassExecutor(resolvedMethods.FirstOrDefault(), startupType);
        }

    }
}
