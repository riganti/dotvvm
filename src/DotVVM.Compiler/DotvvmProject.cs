using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Compiler
{
    public static class DotvvmProject
    {
        public const string CliDirectoryName = ".dotvvm";

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

        public static DotvvmConfiguration GetConfiguration(string projectName, string projectDirectory)
        {
            return GetConfiguration(
                webSiteAssembly: Assembly.Load(projectName),
                webSitePath: projectDirectory,
                configureServices: c => c.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());
        }

        public static DotvvmConfiguration GetConfiguration(
            Assembly webSiteAssembly,
            string webSitePath,
            Action<IServiceCollection> configureServices)
        {
            var dotvvmStartup = GetDotvvmStartup(webSiteAssembly);
            var configuratorType = GetDotvvmServiceConfiguratorType(webSiteAssembly);
            var configureServicesMethod = configuratorType is object
                ? GetConfigureServicesMethod(configuratorType)
                : null;
            var wut = typeof(IServiceCollection);
            var wat = ((MethodInfo)typeof(DotvvmConfiguration).GetMember("CreateDefault")[0]).GetParameters()[0].ParameterType.GenericTypeArguments[0];

            //var config = DotvvmConfiguration.CreateDefault();

            //var config = DotvvmConfiguration.CreateDefault(services => {
            //    if (configureServicesMethod is object)
            //    {
            //        InvokeConfigureServices(configureServicesMethod, services);
            //    }
            //    configureServices?.Invoke(services);
            //});
            return null;

            //config.ApplicationPhysicalPath = webSitePath;
            //dotvvmStartup?.Configure(config, webSitePath);
            //return config;
        }

        public static DirectoryInfo CreateDotvvmDirectory(FileSystemInfo target)
        {
            if (target is FileInfo file)
            {
                target = file.Directory;
            }

            var dotvvmDir = new DirectoryInfo(Path.Combine(target.FullName, CliDirectoryName));
            if (!dotvvmDir.Exists)
            {
                dotvvmDir.Create();
            }

            return dotvvmDir;
        }

        public static IDotvvmStartup GetDotvvmStartup(Assembly assembly)
        {
            //find all implementations of IDotvvmStartup
            var dotvvmStartupType = GetDotvvmStartupType(assembly);
            if (dotvvmStartupType is null)
            {
                throw new ArgumentException("Could not found an implementation of IDotvvmStartup "
                    + $"in '{assembly.FullName}.");
            }

            return dotvvmStartupType.Apply(Activator.CreateInstance)!.CastTo<IDotvvmStartup>();
        }

        public static FileInfo GetCliFile(FileSystemInfo target, string relativePath)
        {
            var dir = CreateDotvvmDirectory(target);
            return new FileInfo(Path.Combine(dir.FullName, relativePath));
        }

        private static Type? GetDotvvmStartupType(Assembly assembly)
        {
            var dotvvmStartups = assembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (dotvvmStartups.Length > 1)
            {
                var startupNames = string.Join(", ", dotvvmStartups.Select(s => $"'{s.Name}'"));
                throw new ArgumentException("Found more than one IDotvvmStartup implementation in "
                    + $"'{assembly.FullName}': {startupNames}.");
            }
            return dotvvmStartups.SingleOrDefault();
        }

        private static Type? GetDotvvmServiceConfiguratorType(Assembly assembly)
        {
            var interfaceType = typeof(IDotvvmServiceConfigurator);
            var resultTypes = assembly.GetLoadableTypes()
                .Where(s => s.GetTypeInfo().ImplementedInterfaces
                    .Any(i => i.Name == interfaceType.Name))
                    .Where(s => s != null)
                .ToArray();
            if (resultTypes.Length > 1)
            {
                throw new ArgumentException("Found more than one implementation of IDotvvmServiceConfiguration in "
                    + $"'{assembly.FullName}'.");
            }

            return resultTypes.SingleOrDefault();
        }

        private static MethodInfo GetConfigureServicesMethod(Type type)
        {
            var method = type.GetMethod("ConfigureServices", new[] { typeof(IDotvvmServiceCollection) });
            if (method == null)
            {
                throw new ArgumentException($"Type '{type}' is missing the "
                    + "'void ConfigureServices IDotvvmServiceCollection services)'.");
            }
            return method;
        }

        private static void InvokeConfigureServices(MethodInfo method, IServiceCollection collection)
        {
            if (method.IsStatic)
            {
                method.Invoke(null, new object[] { new DotvvmServiceCollection(collection) });
            }
            else
            {
                var instance = Activator.CreateInstance(method.DeclaringType!);
                method.Invoke(instance, new object[] { new DotvvmServiceCollection(collection) });
            }
        }
    }
}
