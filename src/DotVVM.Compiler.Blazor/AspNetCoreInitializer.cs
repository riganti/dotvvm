using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.Options;

namespace DotVVM.Compiler.Blazor
{
    class AspNetCoreInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string webSitePath, Action<IServiceCollection> registerServices)
        {
            var dependencyContext = DependencyContext.Load(webSiteAssembly);
            var assemblyNames = new Lazy<List<AssemblyData>>(() => ResolveAssemblies(dependencyContext));

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                // find potential assemblies
                var assembly = assemblyNames.Value
                    .Where(a => string.Equals(a.AssemblyFileName, name.Name, StringComparison.CurrentCultureIgnoreCase))
                    .Select(a => new { AssemblyData = a, AssemblyName = AssemblyLoadContext.GetAssemblyName(a.AssemblyFullPath) })
                    .FirstOrDefault(a => a.AssemblyName.Name == name.Name && a.AssemblyName.Version == name.Version);

                if (assembly == null)
                {
                    return null;
                }
                else
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.AssemblyData.AssemblyFullPath);
                }
            };

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
                    registerServices?.Invoke(services);
                    foreach(var cs in configureServices)
                        cs.Invoke(cs.IsStatic ? null : Activator.CreateInstance(cs.DeclaringType), new object[] { services });
                });


            config.ApplicationPhysicalPath = webSitePath;
            startup.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;

            // It should be handled by the DotvvmConfiguration.CreateDefault:

            // var configurers = config.ServiceLocator.GetServiceProvider().GetServices<IConfigureOptions<DotvvmConfiguration>>().ToArray();
            // if (startup == null && configurers.Length == 0) throw new Exception($"Could not find any IConfigureOptions<DotvvmConfiguration> nor a IDotvvmStartup implementation.");
            // foreach (var configurer in configurers)
            // {
            //     configurer.Configure(config);
            // }

            return config;
        }

        private static List<AssemblyData> ResolveAssemblies(DependencyContext dependencyContext)
        {
            return dependencyContext.CompileLibraries
                .SelectMany(l =>
                {
                    try
                    {
                        var paths = l.ResolveReferencePaths();
                        return paths.Select(p => new AssemblyData
                        {
                            Library = l,
                            AssemblyFullPath = p,
                            AssemblyFileName = Path.GetFileNameWithoutExtension(p)
                        });
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<AssemblyData>();
                    }
                })
                .ToList();
        }
    }

    internal class AssemblyData
    {
        public CompilationLibrary Library { get; set; }
        public string AssemblyFullPath { get; set; }
        public string AssemblyFileName { get; set; }
    }
}