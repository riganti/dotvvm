#if NET461
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DotVVM.Compiler
{
    [Serializable]
    public class AppDomainExecutor : MarshalByRefObject
    {
        public void ExecuteCompile(
            FileInfo assembly,
            DirectoryInfo? projectDir,
            string? rootNamespace)
        {
            var projectAssembly = Assembly.LoadFrom(assembly.FullName);
            Program.Compile(projectAssembly, projectDir, rootNamespace);
        }
    }
}
#endif
