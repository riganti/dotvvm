using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Tool
{
    public static class Compiler
    {
        public static void AddCompiler(Command command)
        {
            var compileCmd = new Command("compile", "Invokes the DotVVM compiler.");
            compileCmd.AddArgument(new Argument<FileSystemInfo>(
                name: "target",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project."));
            compileCmd.AddOption(new Option<FileSystemInfo>(
                alias: "--compiler",
                description: "Path to the source of DotVVM.Compiler."));
            compileCmd.AddOption(new Option<bool>(
                alias: "--debug",
                description: "Builds the compiler shim with the Debug configuration."));
            compileCmd.Handler = CommandHandler.Create(typeof(Compiler).GetMethod(nameof(ExecuteCommand))!);
            command.AddCommand(compileCmd);
        }

        public static int ExecuteCommand(FileSystemInfo target, FileSystemInfo? compiler, bool debug)
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
            return msbuild.TryBuild(shim, debug ? "Debug" : "Release", logger) ? 0 : 1;
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
