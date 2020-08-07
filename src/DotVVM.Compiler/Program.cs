using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Cli;
using DotVVM.Compiler.Compilation;
using DotVVM.Utils.ConfigurationHost.Initialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Compiler
{
    public static class Program
    {
        private static ILoggerFactory Logging = new NullLoggerFactory();

        public static async Task<int> Run(
            bool debuggerBreak,
            string? assemblyName,
            string? applicationPath,
            bool jsonIn,
            bool jsonOut,
            ILogger? logger = null)
        {
            logger ??= Logging.CreateLogger("Compiler");

            if (debuggerBreak)
            {
                logger.LogDebug("Breaking for debugger.");
                Debugger.Break();
            }

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

            CompilationResult result;
            if (options.FullCompile || options.CheckBindingErrors)
            {
                var compiler = new ViewStaticCompiler(logger)
                {
                    Options = options
                };
                result = compiler.Execute();
            }
            else
            {
                var assembly = Assembly.Load(options.WebSiteAssembly);
                var config = ConfigurationInitializer
                    .InitDotVVM(assembly, options.WebSitePath, null, collection => { });
                result = new CompilationResult
                {
                    Configuration = config
                };
            }

            ConfigurationSerialization.PreInit();

            if (jsonOut || options.ConfigOutputPath is object)
            {
                var serializedResult = JsonSerializer.Serialize(
                    value: result,
                    options: new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                if (jsonOut)
                {
                    Console.WriteLine(serializedResult);
                }
                if (options.ConfigOutputPath is object)
                {
                    if (options.ConfigOutputPath is object)
                    {
                        var file = new FileInfo(options.ConfigOutputPath);
                        if (!file.Directory.Exists)
                        {
                            file.Directory.Create();
                        }
                        await File.WriteAllTextAsync(file.FullName, serializedResult);
                    }
                }
            }
            else
            {
                foreach (var file in result.Files)
                {
                    foreach (var error in file.Value.Errors)
                    {
                        logger.LogError($"{file.Key}: {error.Message}");
                    }
                }
            }
            return 0;
        }

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Compiler");
            rootCmd.AddOption(new Option<bool>("--debugger-break", "Breaks to let a debugger attach to the process"));
            rootCmd.AddOption(new Option<string>("--assembly-name", "Name of the assembly with DotvvmStartup"));
            rootCmd.AddOption(new Option<string>("--application-path", "Path to the parent of Controls, Views, etc."));
            rootCmd.AddOption(new Option<bool>("--json-in", "Read options from stdin in JSON"));
            rootCmd.AddOption(new Option<bool>("--json-out", "Write results to stdout in JSON"));
            rootCmd.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run))!);
            Logging = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
            var exitCode = rootCmd.Invoke(args);
            Logging.Dispose();
            return exitCode;
        }
    }
}
