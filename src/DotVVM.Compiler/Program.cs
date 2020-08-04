using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using DotVVM.Compiler.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Compiler
{
    public static class Program
    {
        private static ILoggerFactory Logging = new NullLoggerFactory();

        public static void Run(
            bool debuggerBreak,
            string assemblyName,
            string applicationPath,
            ILogger? logger = null)
        {
            logger ??= Logging.CreateLogger("Compiler");

            if (debuggerBreak)
            {
                logger.LogDebug("Breaking for debugger.");
                Debugger.Break();
            }

            var options = new CompilerOptions
            {
                WebSiteAssembly = assemblyName,
                WebSitePath = applicationPath
            };
            var compiler = new ViewStaticCompiler
            {
                Options = options
            };
            var result = compiler.Execute();

            foreach(var file in result.Files)
            {
                foreach(var error in file.Value.Errors)
                {
                    logger.LogInformation($"{file.Key}: {error}");
                }
            }
        }

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Compiler");
            rootCmd.AddOption(new Option<bool>("--debugger-break", "Breaks to let a debugger attach to the process"));
            rootCmd.AddOption(new Option<string>("--assembly-name", "Name of the assembly with DotvvmStartup"));
            rootCmd.AddOption(new Option<string>("--application-path", "Path to the parent of Controls, Views, etc."));
            rootCmd.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run))!);
            Logging = LoggerFactory.Create(b => b.AddConsole());
            var exitCode = rootCmd.Invoke(args);
            Logging.Dispose();
            return exitCode;
        }
    }
}
