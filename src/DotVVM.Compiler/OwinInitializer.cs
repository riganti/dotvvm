using DotVVM.Framework;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DotVVM.Compiler
{
    class OwinInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, ViewStaticCompilerCompiler viewStaticCompilerCompiler, Action<IServiceCollection> registerServices)
        {
            var dotvvmStartups = webSiteAssembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");
            var startup = dotvvmStartups.SingleOrDefault()?.Apply(Activator.CreateInstance).CastTo<IDotvvmStartup>();

            var configureServices =
                webSiteAssembly.GetLoadableTypes()
                .Where(t => t.Name == "Startup")
                .Select(t => t.GetMethod("ConfigureDotvvmServices", new[] { typeof(IServiceCollection) }) ?? t.GetMethod("ConfigureServices", new[] { typeof(IServiceCollection) }))
                .Where(m => m != null)
                .Where(m => m.IsStatic || m.DeclaringType.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (startup == null && configureServices.Length == 0) throw new Exception($"Could not find ConfigureServices method, nor a IDotvvmStartup implementation.");

            var config = DotvvmConfiguration.CreateDefault(
                services =>
                {
                    if (viewStaticCompilerCompiler != null)
                    {
                        services.AddSingleton<ViewStaticCompilerCompiler>(viewStaticCompilerCompiler);
                        services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                    }
                    registerServices?.Invoke(services);
                    foreach(var cs in configureServices)
                        cs.Invoke(cs.IsStatic ? null : Activator.CreateInstance(cs.DeclaringType), new object[] { services });
                });
            config.ApplicationPhysicalPath = webSitePath;
            startup?.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;
            
            var configurers = config.ServiceProvider.GetServices<IConfigureOptions<DotvvmConfiguration>>().ToArray();
            if (startup == null && configurers.Length == 0) throw new Exception($"Could not find any IConfigureOptions<DotvvmConfiguration> nor a IDotvvmStartup implementation.");
            foreach (var configurer in configurers)
            {
                configurer.Configure(config);
            }

            return config;
        }
    }
}
