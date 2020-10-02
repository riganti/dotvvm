using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using DotVVM.CommandLine;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotVVM.Compiler
{
    public static class Program
    {
        private const string IDotvvmCacheAdapterName
            = "DotVVM.Framework.Runtime.Caching.IDotvvmCacheAdapter, DotVVM.Framework";
        private const string SimpleDictionaryCacheAdapterName
            = "DotVVM.Framework.Testing.SimpleDictionaryCacheAdapter, DotVVM.Framework";


        public static void Run(
            FileInfo assembly,
            DirectoryInfo? projectDir,
            string? rootNamespace,
            ILogger logger)
        {
#if NETCOREAPP3_1
            var dependencyResolver = new AssemblyDependencyResolver(assembly.FullName);
            AssemblyLoadContext.Default.Resolving += (c, n) =>
            {
                var path = dependencyResolver.ResolveAssemblyToPath(n);
                return path is object
                    ? c.LoadFromAssemblyPath(path)
                    : null;
            };
            var projectAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.FullName);
            Compile(projectAssembly, projectDir, rootNamespace, logger);
#elif NETCOREAPP2_1
            var plugin = McMaster.NETCore.Plugins.PluginLoader.CreateFromAssemblyFile(
                assembly.FullName,
                new Type[] { typeof(ILogger) });
            var projectAssembly = plugin.LoadDefaultAssembly();
            var thisAssembly = plugin.LoadAssemblyFromPath(typeof(Program).Assembly.Location);
            thisAssembly.GetType(typeof(Program).FullName).GetMethod(nameof(Compile))
                .Invoke(null, new object?[] { projectAssembly, projectDir, rootNamespace, logger });
#endif
        }

        public static void Compile(
            Assembly assembly,
            DirectoryInfo? projectDir,
            string? rootNamespace,
            ILogger logger)
        {
            var webSitePath = projectDir?.FullName ?? Directory.GetCurrentDirectory();
            var configuration = DotvvmProject.GetConfiguration(assembly, webSitePath, services =>
            {
                services.AddSingleton<IControlResolver, StaticViewControlResolver>();
                services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                services.AddSingleton(new RefObjectSerializer());

                // NB: IDotvvmCacheAdapter is not in v2.0.0 that's why it's hacked this way.
                var iCacheAdapter = Type.GetType(IDotvvmCacheAdapterName);
                if (iCacheAdapter is object)
                {
                    services.AddSingleton(iCacheAdapter, Type.GetType(SimpleDictionaryCacheAdapterName));
                }

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
            foreach (var view in views)
            {
                foreach (var report in view.Reports)
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
