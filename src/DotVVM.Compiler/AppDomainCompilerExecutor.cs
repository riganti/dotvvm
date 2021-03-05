using System;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Compilation.Static;

namespace DotVVM.Compiler
{
    public class AppDomainCompilerExecutor : MarshalByRefObject, ICompilerExecutor
    {
        private readonly DefaultCompilerExecutor inner = new();

        public AppDomainCompilerExecutor()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Name))
                {
                    return null;
                }

                var assemblyName = new AssemblyName(e.Name);

                var projectRelatedPath = Path.Combine(
                        AppDomain.CurrentDomain.SetupInformation.ApplicationBase!,
                        assemblyName.Name + ".dll");
                if (File.Exists(projectRelatedPath))
                {
                    return Assembly.LoadFrom(projectRelatedPath);
                }

                var compilerRelatedPath = Path.Combine(
                    Path.GetDirectoryName(typeof(AppDomainCompilerExecutor).Assembly.Location)!,
                    assemblyName.Name + ".dll");
                if (File.Exists(compilerRelatedPath))
                {
                    return Assembly.LoadFrom(compilerRelatedPath);
                }

                // Don't worry about the missing DotVVM.Framework.resources assembly. It's just the runtime looking for
                // resources.
                return null;
            };
        }

        public bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir)
        {
            return inner.ExecuteCompile(assemblyFile, projectDir);
        }
    }
}
