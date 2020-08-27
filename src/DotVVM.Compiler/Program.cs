using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.CommandLine;
using DotVVM.Compiler.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Compiler
{
    public static class Program
    {
        public static int CompileViews(
            Assembly assembly,
            CompilerOptions options,
            ILogger logger)
        {
            var compiler = new ViewStaticCompiler(assembly, options, logger);
            var result = compiler.Execute();
            
            return result.Files.Count == 0 ? 0 : 1;
        }

        public static int ExportConfiguration(
            Assembly assembly,
            CompilerOptions options,
            ILogger logger)
        {
            var config = ConfigurationInitializer
                .InitDotVVM(assembly, options.WebSitePath, null, collection => { });
            var result = new CompilationResult
            {
                Configuration = config
            };
            LogResult(result, logger);
            return 0;
        }

        public static void LogResult(CompilationResult result, ILogger logger)
        {
            foreach (var file in result.Files)
            {
                foreach (var error in file.Value.Errors)
                {
                    logger.LogError($"{file.Key}: {error.Message}");
                }
            }
        }

        public static async Task<int> Run(
            string? assemblyName,
            string? applicationPath,
            bool jsonIn,
            bool jsonOut,
            ILogger logger)
        {
            var options = new CompilerOptions();
            if (jsonIn)
            {
                using var stdin = Console.OpenStandardInput();
                options = await JsonSerializer.DeserializeAsync<CompilerOptions>(stdin);
            }
            options.WebSiteAssembly = assemblyName ?? options.WebSiteAssembly;
            options.WebSitePath = applicationPath ?? options.WebSitePath;
            options.ApplyDefaults();

            if (options.WebSiteAssembly is null)
            {
                logger.LogCritical("A name of the assembly of the DotVVM project is required.");
                return 1;
            }

            if (options.WebSitePath is null)
            {
                logger.LogCritical("A path to the DotVVM project is required.");
                return 1;
            }

            var websiteAssembly = Assembly.Load(options.WebSiteAssembly);
            return options.FullCompile || options.CheckBindingErrors
                ? CompileViews(websiteAssembly, options, logger)
                : ExportConfiguration(websiteAssembly, options, logger);

            // if (jsonOut || options.ConfigOutputPath is object)
            // {
            //     var serializedResult = JsonSerializer.Serialize(
            //         value: result,
            //         options: new JsonSerializerOptions
            //         {
            //             WriteIndented = true
            //         });
            //     if (jsonOut)
            //     {
            //         Console.WriteLine(serializedResult);
            //     }
            //     if (options.ConfigOutputPath is object)
            //     {
            //         if (options.ConfigOutputPath is object)
            //         {
            //             var file = new FileInfo(options.ConfigOutputPath);
            //             if (!file.Directory.Exists)
            //             {
            //                 file.Directory.Create();
            //             }
            //             File.WriteAllText(file.FullName, serializedResult);
            //         }
            //     }
            // }
        }

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Compiler");
            rootCmd.AddOption(new Option<string>("--assembly-name", "Name of the assembly with DotvvmStartup"));
            rootCmd.AddOption(new Option<string>("--application-path", "Path to the parent of Controls, Views, etc."));
            rootCmd.AddOption(new Option<bool>("--json-in", "Read options from stdin in JSON"));
            // rootCmd.AddOption(new Option<bool>("--json-out", "Write results to stdout in JSON"));
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
