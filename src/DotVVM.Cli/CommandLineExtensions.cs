using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DotVVM.Cli;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace System.CommandLine
{
    public static class CommandLineExtensions
    {
        public const string VerboseAlias = "--verbose";
        public const string TargetArg = "target";

        public static ILoggerFactory Factory = new NullLoggerFactory();
        
        private static readonly Option<bool> verboseOption = new Option<bool>(
                aliases: new[] { "-v", VerboseAlias },
                description: "Print more verbose output");

        public static void AddVerboseOption(this Command command)
        {
            command.AddGlobalOption(verboseOption);
        }

        public static CommandLineBuilder UseLogging(this CommandLineBuilder builder)
        {
            return builder.UseMiddleware(async (c, next) =>
            {
                var logLevel = c.ParseResult.ValueForOption(verboseOption)
                    ? LogLevel.Debug
                    : LogLevel.Information;
                Factory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(logLevel));
                var loggerName = $"{c.ParseResult.CommandResult.Command.Name}";
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
                    var logger = Factory.CreateLogger("project metadata");
                    var metadata = await ProjectFile.GetProjectMetadata(target, logger);
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
    }
}
