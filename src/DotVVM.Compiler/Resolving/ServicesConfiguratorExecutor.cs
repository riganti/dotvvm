using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public class ServicesConfiguratorExecutor : IServicesConfiguratorExecutor
    {
        private readonly MethodInfo method;
        private readonly Type startupType;

        public ServicesConfiguratorExecutor(MethodInfo method, Type startupType)
        {
            this.method = method;
            this.startupType = startupType;
        }
        public void ConfigureServices(IServiceCollection collection)
        {
            if (method.IsStatic)
            {
                method.Invoke(null, new object[] {new DotvvmServiceCollection(collection)});
            }
            else
            {
                var instance = Activator.CreateInstance(startupType);
                method.Invoke(instance, new object[] { new DotvvmServiceCollection(collection) });

            }

        }
    }
}
