using System;
using System.Reflection;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    internal class ConfigurationInitializer
    {
        public static DotvvmConfiguration Init(Assembly assembly, string webSitePath, ViewStaticCompiler viewStaticCompiler, Action<DotvvmConfiguration, IServiceCollection> registerServices)
        {
#if NET461
            return OwinInitializer.InitDotVVM(assembly, webSitePath, viewStaticCompiler, registerServices);
#else

            return  AspNetCoreInitializer.InitDotVVM(assembly, webSitePath, viewStaticCompiler, registerServices);
#endif
        }
    }
}