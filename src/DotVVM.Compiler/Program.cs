using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;

namespace DotVVM.Compiler
{
    public static class Program
    {
        public static void Run(FileInfo assembly, DirectoryInfo? projectDir, string? rootNamespace)
        {
            ProjectLoader.GetExecutor(assembly.FullName).ExecuteCompile(assembly, projectDir, rootNamespace);
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
