using System.Collections.Generic;
using System.CommandLine.Parsing;
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

        private static readonly Option<bool> verboseOption = new Option<bool>(VerboseAlias, "-v")
        {
            Description = "Print more verbose output"
        };

        private static readonly Option<bool> debuggerBreakOption = new Option<bool>(DebuggerBreakAlias)
        {
            Description = "Breaks to let a debugger attach to the process"
        };

        private static readonly Argument<FileSystemInfo> targetArgument = new Argument<FileSystemInfo>(TargetArg)
        {
            Description = "Path to a DotVVM project",
            Arity = ArgumentArity.ZeroOrOne
        };

        public static void AddRange(this Command command, params Symbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                switch (symbol)
                {
                    case Argument argument:
                        command.Arguments.Add(argument);
                        break;
                    case Option option:
                        command.Options.Add(option);
                        break;
                    case Command subcommand:
                        command.Subcommands.Add(subcommand);
                        break;
                }
            }
        }

        public static void AddVerboseOption(this Command command)
        {
            command.Options.Add(verboseOption);
        }

        public static void AddDebuggerBreakOption(this Command command)
        {
            command.Options.Add(debuggerBreakOption);
        }

        public static void AddTargetArgument(this Command command)
        {
            command.Arguments.Add(targetArgument);
        }

        public static bool TryGetProject(ParseResult result, out DotvvmProject project, out ILogger logger)
        {
            var logLevel = result.GetValue(verboseOption)
                ? LogLevel.Debug
                : LogLevel.Information;
            Factory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(logLevel));
            logger = Factory.CreateLogger(GetCommandPath(result.CommandResult));

            var target = FindTarget(result);
            if (target is null)
            {
                project = null!;
                return false;
            }

            var csproj = DotvvmProject.FindProjectFile(target.FullName);
            if (csproj is null)
            {
                logger.LogError($"No project could be found in '{target}'.");
                project = null!;
                return false;
            }

            project = DotvvmProject.FromCsproj(csproj.FullName, logger)!;
            return project is not null;
        }

        private static FileSystemInfo? FindTarget(ParseResult result)
        {
            CommandResult? current = result.CommandResult;
            while (current is object)
            {
                var target = current.Command.Arguments.FirstOrDefault(c => c.Name == TargetArg);
                if (target is object)
                {
                    var fsInfo = result.GetValue((Argument<FileSystemInfo>)target);
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
                names.Add(current.Command.Name);
                current = current.Parent as CommandResult;
            }
            names.Reverse();
            return string.Join(" ", names);
        }
    }
}
