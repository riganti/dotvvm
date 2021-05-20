using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Static;

namespace DotVVM.Compiler
{
    public class DefaultCompilerExecutor : ICompilerExecutor
    {
        public bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir)
        {
            var assembly = Assembly.LoadFile(assemblyFile.FullName);
            var projectDirPath = projectDir?.FullName ?? Directory.GetCurrentDirectory();
            var reports = StaticViewCompiler.CompileAll(assembly, projectDirPath);
            var logger = new DefaultCompilationReportLogger();
            using var err = Console.OpenStandardError();
            logger.Log(err, reports);
            return reports.Length == 0;
        }
    }
}
