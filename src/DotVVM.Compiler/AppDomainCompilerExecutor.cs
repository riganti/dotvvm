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
                var assemblyName = new AssemblyName(e.Name);
                try
                {
                    return Assembly.LoadFrom(Path.Combine(
                        AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                        assemblyName.Name + ".dll"));
                }
                catch(FileNotFoundException)
                {
                    return null;
                }
            };
        }

        public void ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir, string? rootNamespace)
        {
            inner.ExecuteCompile(assemblyFile, projectDir, rootNamespace);
        }
    }
}
