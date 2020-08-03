using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Tool
{
    public static class Compiler
    {
        public const string CompilerExecutable = "Compiler.dll";

        public static void AddCompiler(Command command)
        {
            var compileCmd = new Command("compile", "Invokes the DotVVM compiler");
            compileCmd.AddArgument(new Argument<FileSystemInfo>(
                name: "target",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project"));
            compileCmd.AddArgument(new Argument<string[]>(
                name: "compilerArgs",
                description: "Arguments passed to the compiler"));
            compileCmd.AddOption(new Option<FileSystemInfo>(
                alias: "--compiler",
                description: "Path to the source of DotVVM.Compiler"));
            compileCmd.AddOption(new Option<bool>(
                alias: "--debug",
                description: "Build the compiler shim with the Debug configuration"));
            compileCmd.Handler = CommandHandler.Create(typeof(Compiler).GetMethod(nameof(ExecuteCommand))!);
            command.AddCommand(compileCmd);
        }

        public static int ExecuteCommand(
            FileSystemInfo target,
            string[]? compilerArgs,
            FileSystemInfo? compiler,
            bool debug)
        {
            var logger = Program.Logging.CreateLogger("Compiler");

            var project = ProjectFile.FindProjectFile(target);
            if (project is null)
            {
                logger.LogCritical("No project file could be found.");
                return 1;
            }
            else
            {
                logger.LogDebug($"Found the '{project}' project file.");
            }

            string? compilerPath = null;
            if (compiler is object)
            {
                var compilerProject = ProjectFile.FindProjectFile(compiler);
                if (compilerProject is null)
                {
                    logger.LogError($"DotVVM.Compiler could not be found at '{compiler}'. Ignoring.");
                }
                else
                {
                    compilerPath = compilerProject.FullName;
                }
            }

            var msbuild = MSBuild.Create();
            if (msbuild is null)
            {
                logger.LogCritical("MSBuild could not be found.");
                return 1;
            }
            else
            {
                logger.LogDebug($"Using the '{msbuild}' msbuild.");
            }

            var shim = CreateCompilerShim(project, compilerPath);
            var configuration = debug ? "Debug" : "Release";
            if (!msbuild.TryBuild(shim, configuration, logger))
            {
                logger.LogCritical("Failed to build the compiler shim.");
                return 1;
            }

            var compilerExePath = $"bin/{configuration}/{Templates.Netcoreapp}/{CompilerExecutable}";
            var compilerExe = new FileInfo(Path.Combine(shim.DirectoryName, compilerExePath));
            if (!compilerExe.Exists)
            {
                logger.LogCritical($"The compiler shim executable could not be found at '{compilerExe}'.");
                return 1;
            }

            var sb = new StringBuilder();
            sb.Append(compilerExe.FullName);
            if (compilerArgs is object)
            {
                sb.Append(' ');
                sb.AppendJoin(' ', compilerArgs.Select(s => $"\"{s}\""));
            }
            var processInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = sb.ToString()
            };
            var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit();
            return process.ExitCode;
        }

        public static FileInfo CreateCompilerShim(FileInfo projectFile, string? compilerPath = null)
        {
            var dotvvmDir = new DirectoryInfo(Path.Combine(projectFile.DirectoryName, Templates.DotvvmDirectory));
            if (!dotvvmDir.Exists)
            {
                dotvvmDir.Create();
            }

            if (compilerPath is object)
            {
                compilerPath = Path.GetRelativePath(dotvvmDir.FullName, compilerPath);
            }

            var shimFile = new FileInfo(Path.Combine(dotvvmDir.FullName, Templates.CompilerShimProjectFile));

            File.WriteAllText(
                path: shimFile.FullName,
                contents: Templates.GetCompilerShimProject(
                    project: Path.GetRelativePath(dotvvmDir.FullName, projectFile.FullName),
                    compilerReference: compilerPath));

            File.WriteAllText(
                path: Path.Combine(dotvvmDir.FullName, Templates.CompilerShimProgramFile),
                contents: Templates.GetCompilerShimProgram());

            return shimFile;
        }
    }
}
