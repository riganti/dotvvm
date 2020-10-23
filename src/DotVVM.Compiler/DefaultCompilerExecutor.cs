using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    [Serializable]
    public class DefaultCompilerExecutor : MarshalByRefObject, ICompilerExecutor
    {
        public void ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir, string? rootNamespace)
        {
            var assembly = Assembly.LoadFile(assemblyFile.FullName);
            var projectDirPath = projectDir?.FullName ?? Directory.GetCurrentDirectory();
            var configuration = StaticViewCompiler.CreateConfiguration(assembly, projectDirPath);
            var compiler = configuration.ServiceProvider.GetRequiredService<StaticViewCompiler>();
            var views = compiler.CompileAllViews();
            var logger = new TextReportLogger();
            using var err = Console.OpenStandardError();
            logger.Log(err, views.SelectMany(s => s.Reports));
        }
    }
}
