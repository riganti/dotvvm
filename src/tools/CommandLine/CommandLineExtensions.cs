using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DotVVM.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace System.CommandLine
{
    public static class CommandLineExtensions
    {
        public const string VerboseAlias = "--verbose";
        public const string DebuggerBreakAlias = "--debugger-break";
        public const string TargetArg = "target";

        public static ILoggerFactory Factory = new NullLoggerFactory();

        private static readonly Option<bool> verboseOption = new Option<bool>(
            aliases: new[] { "-v", VerboseAlias },
            description: "Print more verbose output");

        private static readonly Option<bool> debuggerBreakOption = new Option<bool>(
            alias: DebuggerBreakAlias,
            description: "Breaks to let a debugger attach to the process");

        private static readonly Argument<FileSystemInfo> targetArgument = new Argument<FileSystemInfo>(
            name: TargetArg,
            description: "Path to a DotVVM project")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        public static void AddRange(this Command command, params Symbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                command.Add(symbol);
            }
        }

        public static void AddVerboseOption(this Command command)
        {
            command.AddGlobalOption(verboseOption);
        }

        public static void AddDebuggerBreakOption(this Command command)
        {
            command.AddGlobalOption(debuggerBreakOption);
        }

        public static void AddTargetArgument(this Command command)
        {
            command.AddArgument(targetArgument);
        }

        public static CommandLineBuilder UseLogging(this CommandLineBuilder builder)
        {
            return builder.UseMiddleware(async (c, next) =>
            {
                var logLevel = c.ParseResult.ValueForOption(verboseOption)
                    ? LogLevel.Debug
                    : LogLevel.Information;
                Factory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(logLevel));
                var loggerName = GetCommandPath(c.ParseResult.CommandResult);
                c.BindingContext.AddService(_ => Factory.CreateLogger(loggerName));
                await next(c);
                Factory.Dispose();
            });
        }

        public static CommandLineBuilder UseDotvvmMetadata(this CommandLineBuilder builder)
        {
            return builder.UseMiddleware(async (c, next) =>
            {
                var target = FindTarget(c.ParseResult);
                if (target is object)
                {
                    var logger = Factory.CreateLogger("Project Metadata");
                    var csproj = DotvvmProject.FindProjectFile(target.FullName);
                    if (csproj is null)
                    {
                        logger.LogError($"No project could be found in '{target}'.");
                        c.ResultCode = 1;
                        return;
                    }

                    var project = DotvvmProject.FromCsproj(csproj.FullName, logger);
                    if (project is null)
                    {
                        c.ResultCode = 1;
                        return;
                    }

                    c.BindingContext.AddService(_ => project);
                }
                await next(c);
            });
        }

        public static CommandLineBuilder UseDebuggerBreak(this CommandLineBuilder builder)
        {
            return builder.UseMiddleware(async (c, next) =>
            {
                var shouldBreak = c.ParseResult.ValueForOption<bool>(DebuggerBreakAlias);
                if (shouldBreak)
                {
                    var logger = Factory.CreateLogger("debugging");
                    var pid = Diagnostics.Process.GetCurrentProcess().Id;
                    logger.LogInformation($"Started with PID '{pid}'. Waiting for debugger to attach.");
                    Debugger.Break();
                }
                await next(c);
            });
        }

        private static FileSystemInfo? FindTarget(ParseResult result)
        {
            CommandResult? current = result.CommandResult;
            while (current is object)
            {
                var target = current.Command.Arguments.FirstOrDefault(c => c.Name == TargetArg);
                if (target is object)
                {
                    var fsInfo = result.ValueForArgument((Argument<FileSystemInfo>)target);
                    fsInfo ??= new DirectoryInfo(Environment.CurrentDirectory);
                    return fsInfo;
                }
                current = current.Parent as CommandResult;
            }
            return null;
        }

        private static string GetCommandPath(CommandResult result)
        {
            var names = new List<string>();
            CommandResult? current = result;
            while (current is object)
            {
                names.Add(current.Symbol.Name);
                current = current.Parent as CommandResult;
            }
            names.Reverse();
            return string.Join(" ", names);
        }
    }
}
