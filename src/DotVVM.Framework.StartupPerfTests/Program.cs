using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Process = System.Diagnostics.Process;

namespace DotVVM.Framework.StartupPerfTests
{
    class Program
    {
        private static TextWriter logWriter;

        static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Argument<FileInfo>(
                    "project",
                    "Path to the project file"
                ) { Arity = ArgumentArity.ExactlyOne },
                new Option<TestTarget>(
                    new [] { "-t", "--type" },
                    "Type of the project - use 'owin' or 'aspnetcore'"
                ) { Required = true },
                new Option<int>(
                    new [] { "-r", "--repeat" },
                    () => 1,
                    "How many times the operation should be repeated."
                ),
                new Option<string>(
                    new [] { "-u", "--url" },
                    () => "",
                    "Relative URL in the app that should be tested."
                ),
                new Option<bool>(
                    new [] { "-v", "--verbose" },
                    () => false,
                    "Diagnostics output"
                )
            }; 
            rootCommand.Handler = CommandHandler.Create<FileInfo, TestTarget, int, string, bool>((project, type, repeat, url, verbose) =>
            {
                new StartupPerformanceTest(project, type, repeat, url, verbose).HandleCommand();
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
