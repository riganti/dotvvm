#if  NET461
using DotVVM.Framework;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Compiler.Fakes;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
    
using Microsoft.Owin.Hosting;
using Owin;

namespace DotVVM.Compiler
{
    class OwinInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, ViewStaticCompiler viewStaticCompilerCompiler, Action<DotvvmConfiguration, IServiceCollection> registerServices)
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

            // TODO: run startup class to get configuration from dotvvm middleware

            var startupClass = webSiteAssembly.CustomAttributes?.Where(s => s.AttributeType == typeof(Microsoft.Owin.OwinStartupAttribute)).FirstOrDefault()?.ConstructorArguments[0].Value as Type;
            //if (startupClass != null)
            //{

            //    var configureMethod = startupClass.GetRuntimeMethods().FirstOrDefault(s =>
            //        s.Name == "Configuration" && s.GetParameters().Length == 1 &&
            //        s.GetParameters()[0].ParameterType == typeof(IAppBuilder));
            //    var app = new CompilerAppBuilder();

            //    var startupClassInstance = Activator.CreateInstance(startupClass);

            //    var debug = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            //        .Where(s => s.Name == "AppBuilderExtension").ToList();

            //    configureMethod.Invoke(startupClassInstance, new object[] { app });

            //    var config2 = (DotvvmConfiguration)app.args[0];
            //}



            if (startup == null && configureServices.Length == 0) throw new Exception($"Could not find ConfigureServices method, nor a IDotvvmStartup implementation.");

            IServiceCollection serviceCollection = null;
            var config = DotvvmConfiguration.CreateDefault(
                services => {
                    serviceCollection = services;
                    if (viewStaticCompilerCompiler != null)
                    {
                        services.AddSingleton<ViewStaticCompiler>(viewStaticCompilerCompiler);
                        services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                        services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                        services.AddSingleton(new RefObjectSerializer());
                    }
                    foreach (var cs in configureServices)
                        cs.Invoke(cs.IsStatic ? null : Activator.CreateInstance(cs.DeclaringType), new object[] { services });
                });
            config.ApplicationPhysicalPath = webSitePath;
            registerServices(config, serviceCollection);

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
#endif
