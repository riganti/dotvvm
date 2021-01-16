using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation.Static;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    public class DefaultCompilerExecutor : ICompilerExecutor
    {
        public bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir, string? rootNamespace)
        {
            var assembly = Assembly.LoadFile(assemblyFile.FullName);
            ReplaceDefaultDependencyContext(assembly);
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

        private static void ReplaceDefaultDependencyContext(Assembly projectAssembly)
        {
#if NET461
            return;
#else
            var projectContext = Microsoft.Extensions.DependencyModel.DependencyContext.Load(projectAssembly);
            var mergedContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default.Merge(projectContext);
            var fields = typeof(Microsoft.Extensions.DependencyModel.DependencyContext)
                 .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)!;
            foreach (var field in fields)
            {
                field.SetValue(
                    Microsoft.Extensions.DependencyModel.DependencyContext.Default,
                    field.GetValue(mergedContext));
            }
#endif
        }
    }
}
