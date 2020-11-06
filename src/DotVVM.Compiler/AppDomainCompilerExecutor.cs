using System;
using System.IO;
using System.Reflection;

namespace DotVVM.Compiler
{
    public class AppDomainCompilerExecutor : MarshalByRefObject, ICompilerExecutor
    {
        private readonly DefaultCompilerExecutor inner = new DefaultCompilerExecutor();

        public AppDomainCompilerExecutor()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Name))
                {
                    return null;
                }

                var assemblyName = new AssemblyName(e.Name);
                var assemblyPath = Path.Combine(
                        AppDomain.CurrentDomain.SetupInformation.ApplicationBase!,
                        assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

                // Don't worry about the missing DotVVM.Framework.resources assembly, mate. The runtime defaults to
                // invariant resource culture anyway and find the proper resources in the already loaded
                // DotVVM.Framework assembly.
                return null;
            };
        }

        public void ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir, string? rootNamespace)
        {
            inner.ExecuteCompile(assemblyFile, projectDir, rootNamespace);
        }
    }
}
