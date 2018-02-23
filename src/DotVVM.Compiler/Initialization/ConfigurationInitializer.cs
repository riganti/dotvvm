using System;
using System.Reflection;
using DotVVM.Compiler.Compilation;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Initialization
{
    internal class ConfigurationInitializer
    {
        public static DotvvmConfiguration Init(Assembly assembly, string webSitePath, ViewStaticCompiler viewStaticCompiler, Action<IServiceCollection> additionalServices)
        {
#if NET461
            return OwinInitializer.InitDotVVM(assembly, webSitePath, viewStaticCompiler, additionalServices);
#else

            return AspNetCoreInitializer.InitDotVVM(assembly, webSitePath, viewStaticCompiler, additionalServices);
#endif
        }
    }
}
