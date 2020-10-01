using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.CommandLine;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace DotVVM.Compiler
{
    public static class Program
    {
        public static void RegisterAssemblyDeps(FileInfo assembly)
        {
            var depsFile = new FileInfo(Path.Combine(
                assembly.DirectoryName,
                $"{Path.GetFileNameWithoutExtension(assembly.Name)}.deps.json"));
            if (!depsFile.Exists)
            {
                return;
            }

            DependencyContext? context = null;
            using(var stream = depsFile.OpenRead())
            using(var reader = new DependencyContextJsonReader())
            {
                context = reader.Read(stream);
            }
        }

        public static void Run(
            FileInfo assembly,
            DirectoryInfo? projectDir,
            string? rootNamespace,
            ILogger logger)
        {
            var projectAssembly = Assembly.LoadFrom(assembly.FullName);
            var webSitePath = projectDir?.FullName ?? Directory.GetCurrentDirectory();
            var configuration = DotvvmProject.GetConfiguration(projectAssembly, webSitePath, services =>
            {
                services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                services.AddSingleton(new RefObjectSerializer());
                // services.AddSingleton<IDotvvmCacheAdapter, SimpleDictionaryCacheAdapter>();
                // TODO: LAST PARAMETER
                var bindingCompiler = new AssemblyBindingCompiler(
                    assemblyName: null,
                    className: null,
                    outputFileName: null,
                    configuration: null);
                services.AddSingleton<IBindingCompiler>(bindingCompiler);
                services.AddSingleton<IExpressionToDelegateCompiler>(bindingCompiler.GetExpressionToDelegateCompiler());
            });
            var compiler = new StaticViewCompiler(configuration, false);
            var views = compiler.GetAllViews();
            foreach(var view in views)
            {
                foreach(var report in view.Reports)
                {
                    logger.LogError($"{report.ViewPath}({report.Line},{report.Column}): {report.Message}");
                }
            }
        }

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Compiler");
            rootCmd.AddOption(new Option<FileInfo>(
                aliases: new[] { "-a", "--assembly" },
                description: "Path to the assembly of the DotVVM project")
            {
                IsRequired = true
            });
            rootCmd.AddOption(new Option<DirectoryInfo>(
                alias: "--project-dir",
                description: "The directory of the DotVVM project"));
            rootCmd.AddOption(new Option<string>(
                alias: "--root-namespace",
                description: "The root namespace of the DotVVM project"));
            rootCmd.AddVerboseOption();
            rootCmd.AddDebuggerBreakOption();
            rootCmd.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run))!);

            new CommandLineBuilder(rootCmd)
                .UseDefaults()
                .UseLogging()
                .UseDebuggerBreak()
                .Build();
            return rootCmd.Invoke(args);
        }
    }
}
