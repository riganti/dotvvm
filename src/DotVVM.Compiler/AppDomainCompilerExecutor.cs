using System;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Compilation.Static;

namespace DotVVM.Compiler
{
    public class AppDomainCompilerExecutor : MarshalByRefObject, ICompilerExecutor
    {
        public AppDomainCompilerExecutor()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
                if (string.IsNullOrEmpty(e.Name))
                {
                    return null;
                }

                var assemblyName = new AssemblyName(e.Name);

                return ResolveAssembly(assemblyName, ".dll") ?? ResolveAssembly(assemblyName, ".exe");
            };
        }

        public bool ExecuteCompile(CompilerArgs args)
        {
            var assembly = Assembly.LoadFile(args.AssemblyFile.FullName);
            var inner = new DefaultCompilerExecutor(assembly);
            return inner.ExecuteCompile(args);
        }

        private Assembly? ResolveAssembly(AssemblyName assemblyName, string extension)
        {
            var projectRelatedPath = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase!,
                assemblyName.Name + extension);
            if (File.Exists(projectRelatedPath))
            {
                return Assembly.LoadFrom(projectRelatedPath);
            }

            var compilerRelatedPath = Path.Combine(
                Path.GetDirectoryName(typeof(AppDomainCompilerExecutor).Assembly.Location)!,
                assemblyName.Name + extension);
            if (File.Exists(compilerRelatedPath))
            {
                return Assembly.LoadFrom(compilerRelatedPath);
            }

            // Don't worry about the missing DotVVM.Framework.resources assembly. It's just the runtime looking for
            // resources.
            return null;
        }
    }
}
