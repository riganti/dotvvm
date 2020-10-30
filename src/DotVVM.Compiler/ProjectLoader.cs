using System;
using System.IO;
using System.Reflection;

namespace DotVVM.Compiler
{
    public static class ProjectLoader
    {
        public static ICompilerExecutor GetExecutor(string assemblyPath)
        {
#if NETCOREAPP3_1
            var dependencyResolver = new System.Runtime.Loader.AssemblyDependencyResolver(assemblyPath);
            System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (c, n) =>
            {
                var path = dependencyResolver.ResolveAssemblyToPath(n);
                return path is object
                    ? c.LoadFromAssemblyPath(path)
                    : null;
            };

            _ = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            return new DefaultCompilerExecutor();

#elif NETCOREAPP2_1
            // NB: This currently *almost* works for .NET Core 2.1. It tries to load a "DotVVM.Framework.resources"
            //     assembly, which does not exist.
            var builder = new McMaster.NETCore.Plugins.Loader.AssemblyLoadContextBuilder();
            builder.SetMainAssemblyPath(assembly.FullName);
            builder.SetDefaultContext(new NullLoadContext());
            var baseDir = Path.GetDirectoryName(assembly.FullName);
            var assemblyFileName = Path.GetFileNameWithoutExtension(assembly.FullName);
            var depsJsonFile = Path.Combine(baseDir, assemblyFileName + ".deps.json");
            if (File.Exists(depsJsonFile))
            {
                McMaster.NETCore.Plugins.Loader.DependencyContextExtensions.AddDependencyContext(builder, depsJsonFile);
            }

            var pluginRuntimeConfigFile = Path.Combine(baseDir, assemblyFileName + ".runtimeconfig.json");
            McMaster.NETCore.Plugins.Loader.RuntimeConfigExtensions.TryAddAdditionalProbingPathFromRuntimeConfig(
                builder: builder,
                runtimeConfigPath: pluginRuntimeConfigFile,
                includeDevConfig: true,
                error: out _);
            var loader = builder.Build();
            loader.LoadFromAssemblyPath(assembly.FullName);
            AssemblyLoadContext.Default.Resolving += (c, n) =>
            {
                var sideAssembly = loader.LoadFromAssemblyName(n);
                return sideAssembly is object && sideAssembly != typeof(NullLoadContext).Assembly
                    ? c.LoadFromAssemblyPath(sideAssembly.Location)
                    : null;
            };
            _ = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            return new DefaultCompilerExecutor();

#elif NET461
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(assemblyPath)
            };
            var configPath = assemblyPath + ".config";
            if (File.Exists(configPath))
            {
                setup.ConfigurationFile = configPath;
            }
            var domain = AppDomain.CreateDomain("DotVVM.Compiler.AppDomain", null, setup);
            //var compilerPath = typeof(ProjectLoader).Assembly.Location;
            //var newCompilerPath = Path.Combine(setup.ApplicationBase, Path.GetFileName(compilerPath));
            //File.Copy(compilerPath, newCompilerPath, true);
            return (ICompilerExecutor)domain.CreateInstanceFromAndUnwrap(
                assemblyName: typeof(AppDomainCompilerExecutor).Assembly.Location,
                typeName: typeof(AppDomainCompilerExecutor).FullName); ;
#else
#error Fix TargetFrameworks.
#endif
        }
    }
}
