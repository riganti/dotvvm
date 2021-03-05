using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Static;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    public class DefaultCompilerExecutor : ICompilerExecutor
    {
        public bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir)
        {
            var assembly = Assembly.LoadFile(assemblyFile.FullName);
            var projectDirPath = projectDir?.FullName ?? Directory.GetCurrentDirectory();
            var configuration = StaticViewCompiler.CreateConfiguration(assembly, projectDirPath);
            var compiler = configuration.ServiceProvider.GetRequiredService<StaticViewCompiler>();
            var views = compiler.CompileAllViews();
            var logger = new TextReportLogger();
            using var err = Console.OpenStandardError();
            var reports = views.SelectMany(s => s.Reports).ToList();
            logger.Log(err, reports);
            return reports.Count == 0;
        }
    }
}
