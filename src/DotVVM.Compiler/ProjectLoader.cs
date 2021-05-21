using System;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Compilation.Static;

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
            return new DependencyContextCompilerExecutor();

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
            return (ICompilerExecutor)domain.CreateInstanceFromAndUnwrap(
                assemblyName: typeof(AppDomainCompilerExecutor).Assembly.Location,
                typeName: typeof(AppDomainCompilerExecutor).FullName);
#else
#error Fix TargetFrameworks.
#endif
        }
    }
}
