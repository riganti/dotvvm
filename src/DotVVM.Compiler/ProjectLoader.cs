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
