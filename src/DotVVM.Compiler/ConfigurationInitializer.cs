using System;
using System.Reflection;
using DotVVM.Cli;
using DotVVM.Compiler.Compilation;
using DotVVM.Compiler.Resolving;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Compiler
{
    internal class ConfigurationInitializer
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
                    services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                    services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                    services.AddSingleton(new RefObjectSerializer());
                    services.AddSingleton<IDotvvmCacheAdapter, SimpleDictionaryCacheAdapter>();
                }

                additionalServices?.Invoke(services);
            });
        }
    }
}
