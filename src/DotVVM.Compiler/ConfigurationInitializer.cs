using System;
using System.Reflection;
using DotVVM.CommandLine;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Compiler
{
    public class ConfigurationInitializer
    {
        public static DotvvmConfiguration InitDotVVM(
            Assembly assembly,
            string webSitePath,
            ViewStaticCompiler viewStaticCompiler,
            Action<IServiceCollection> additionalServices)
        {
            return DotvvmProject.GetConfiguration(assembly, webSitePath, services => {

                if (viewStaticCompiler != null)
                {
                    services.AddSingleton(viewStaticCompiler);
                    services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                    services.AddSingleton(new RefObjectSerializer());
                    // services.AddSingleton<IDotvvmCacheAdapter, SimpleDictionaryCacheAdapter>();
                }

                additionalServices?.Invoke(services);
            });
        }
    }
}
