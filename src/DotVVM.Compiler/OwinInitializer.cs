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

namespace DotVVM.Compiler
{
    class OwinInitializer
    {
		public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, ViewStaticCompilerCompiler viewStaticCompilerCompiler, Action<DotvvmConfiguration, IServiceCollection> registerServices)
        {
            var dotvvmStartups = webSiteAssembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();

            if (dotvvmStartups.Length == 0) throw new Exception("Could not find any implementation of IDotvvmStartup.");
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");

            var startup = (IDotvvmStartup)Activator.CreateInstance(dotvvmStartups[0]);
            IServiceCollection serviceCollection = null;
			var config = DotvvmConfiguration.CreateDefault(
			    services =>
			    {
			        serviceCollection = services;

			        if (viewStaticCompilerCompiler != null)
			        {
			            services.AddSingleton<ViewStaticCompilerCompiler>(viewStaticCompilerCompiler);
			            services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                    }
                });
            registerServices(config, serviceCollection);
            config.ApplicationPhysicalPath = webSitePath;
            startup.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;
            return config;
        }
    }
}
