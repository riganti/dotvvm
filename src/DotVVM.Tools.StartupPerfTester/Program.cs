using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DotVVM.Tools.StartupPerfTester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand("Measures the performance of DotVVM startup")
            {
                new Argument<FileInfo>(
                    "project",
                    "Path to the project file"
                ) { Arity = ArgumentArity.ExactlyOne },
                new Option<TestTarget>(
                    new [] { "-t", "--type" },
                    "Type of the project"
                ) { IsRequired = true },
                new Option<int>(
                    new [] { "-r", "--repeat" },
                    () => 1,
                    "How many times the operation should be repeated"
                ),
                new Option<string>(
                    new [] { "-u", "--url" },
                    () => "",
                    "Relative URL in the app that should be tested"
                ),
                new Option<bool>(
                    new [] { "-v", "--verbose" },
                    () => false,
                    "Diagnostics output"
                ),
                new Option<int>(
                    new[] { "--timeout" },
                    () => 10,
                    "Timeout for the HTTP interface to start listening"
                )
            };
            rootCommand.Name = StartupPerformanceTest.CommandName;
            rootCommand.Handler = CommandHandler.Create<FileInfo, TestTarget, int, string, bool, int>((project, type, repeat, url, verbose, timeout) =>
            {
                using var test = new StartupPerformanceTest(project, type, repeat, url, verbose, timeout);
                test.HandleCommand();
            });
            rootCommand.Invoke(args);
        }
    }

    public enum TestTarget
    {
        Owin,
        AspNetCore
    }
}
