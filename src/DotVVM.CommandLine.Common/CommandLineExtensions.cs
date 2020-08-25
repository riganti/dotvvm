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
        public const string MSBuildOutputAlias = "--msbuild-output";
        public const string TargetArg = "target";

        public static ILoggerFactory Factory = new NullLoggerFactory();
        
        private static readonly Option<bool> verboseOption = new Option<bool>(
            aliases: new[] { "-v", VerboseAlias },
            description: "Print more verbose output");

        private static readonly Option<bool> debuggerBreakOption = new Option<bool>(
            alias: DebuggerBreakAlias,
            description: "Breaks to let a debugger attach to the process");

        private static readonly Option<bool> msbuildOutputOption = new Option<bool>(
            alias: MSBuildOutputAlias,
            description: "Show output from MSBuild invocations");

        private static readonly Argument<FileSystemInfo> targetArgument = new Argument<FileSystemInfo>(
            name: TargetArg,
            getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
            description: "Path to a DotVVM project");

        public static void AddVerboseOption(this Command command)
        {
            command.AddGlobalOption(verboseOption);
        }

        public static void AddDebuggerBreakOption(this Command command)
        {
            command.AddGlobalOption(debuggerBreakOption);
        }

        public static void AddMSBuildOutputOption(this Command command)
        {
            command.AddGlobalOption(msbuildOutputOption);
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
                    var logger = Factory.CreateLogger("metadata");
                    var msbuild = MSBuild.Create();
                    if (msbuild is null)
                    {
                        logger.LogError("MSBuild could not be found.");
                        c.ResultCode = 1;
                        return;
                    }

                    logger.LogDebug($"Found the '{msbuild}' MSBuild.");
                    c.BindingContext.AddService(_ => msbuild);
                    var msbuildOutput = c.ParseResult.ValueForOption(msbuildOutputOption);
                    var metadata = await ProjectMetadata.LoadOrCreate(target, msbuild, msbuildOutput, logger);
                    if (metadata is object)
                    {
                        c.BindingContext.AddService(_ => metadata);
                    }
                    else
                    {
                        c.ResultCode = 1;
                        return;
                    }
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
                var target = current.Children.FirstOrDefault(c => c.Symbol is Argument<FileSystemInfo> arg
                    && arg.Name == TargetArg);
                if (target is object)
                {
                    return result.ValueForArgument((Argument<FileSystemInfo>)target.Symbol);
                }
                current = current.Parent as CommandResult;
            }
            return null;
        }

        private static string GetCommandPath(CommandResult result)
        {
            var names = new List<string>();
            CommandResult? current = result;
            while(current is object)
            {
                names.Add(current.Symbol.Name);
                current = current.Parent as CommandResult;
            }
            names.Reverse();
            return string.Join(" ", names);
        }
    }
}
