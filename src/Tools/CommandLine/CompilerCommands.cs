using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public static class CompilerCommands
    {
        public static void AddCompilerCommands(this Command command)
        {
            var lintCmd = new Command("lint", "Look for compiler errors in Views and Markup Controls");
            lintCmd.AddTargetArgument();
            var noBuildOption = new Option<bool>("--no-build")
            {
                Description = "Don't build the MSBuild project."
            };
            var noColorOption = new Option<bool>("--no-color")
            {
                Description = "Disable ANSI colors in diagnostic output."
            };
            var verboseBuildOutputOption = new Option<bool>("--verbose-build-output")
            {
                Description = "Show MSBuild output for restore and build."
            };
            var configurationOption = new Option<string>("--configuration")
            {
                DefaultValueFactory = _ => "Debug",
                Description = "The configuration used to build the project."
            };
            var frameworkOption = new Option<string>("--framework")
            {
                Description = "The target framework used to build the project."
            };
            lintCmd.AddRange(noBuildOption, noColorOption, verboseBuildOutputOption, configurationOption, frameworkOption);
            lintCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                return HandleLint(
                    project,
                    parseResult.GetValue(noBuildOption),
                    parseResult.GetValue(noColorOption),
                    parseResult.GetValue(verboseBuildOutputOption),
                    parseResult.GetValue(configurationOption) ?? "Debug",
                    parseResult.GetValue(frameworkOption),
                    logger);
            });
            command.Subcommands.Add(lintCmd);
        }

        public static int HandleLint(
            DotvvmProject project,
            bool noBuild,
            bool noColor,
            bool verboseBuildOutput,
            string configuration,
            string? framework,
            ILogger logger)
        {
            framework ??= project.TargetFrameworks.FirstOrDefault()?.GetShortFolderName();
            if (framework is null)
            {
                logger.LogError("A target framework could not be determined automatically. "
                    + "Please use --framework.");
                return 1;
            }

            var targetFramework = NuGetFramework.Parse(framework);
            if (!noBuild)
            {
                var msbuild = MSBuild.CreateForNuGetFramework(targetFramework);
                if (msbuild is null)
                {
                    logger.LogError("No MSBuild executable could be found.");
                    return 1;
                }
                var buildSuccess = msbuild.TryBuild(
                    project: new FileInfo(project.ProjectFilePath),
                    configuration: configuration,
                    targetFramework: framework,
                    showOutput: verboseBuildOutput,
                    logger: logger);
                if (!buildSuccess)
                {
                    logger.LogError("The project could not be built. "
                        + "Please check for compiler errors using 'dotnet build' or Visual Studio.'");
                    return 1;
                }
            }

            var compilerArgs = new List<string>();

            var cliDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
            var executable = "dotnet";
            if (targetFramework.IsDesktop())
            {
                executable = Path.Combine(cliDirectory, "tools/net472/any/DotVVM.Compiler.exe");
#if DEBUG
                if (!File.Exists(executable))
                {
                    // When running the CLI from source, use the locally built .NET Framework compiler.
                    executable = Path.Combine(cliDirectory, "../../../../Compiler/bin/Debug/net472/DotVVM.Compiler.exe");
                }
#endif
                if (!File.Exists(executable))
                {
                    throw new Exception($"DotVVM Compiler wasn't found at '{executable}'. Please note that DotVVM compiler is not supported in versions prior to DotVVM 5.0.");
                }
            }
            else
            {
                var compilerDir = Path.Combine(cliDirectory, "tools/net8.0/any");
                var compilerDll = Path.Combine(compilerDir, "DotVVM.Compiler.dll");
#if DEBUG
                if (!File.Exists(compilerDll))
                {
                    // When running the CLI from source, use the locally built .NET compiler.
                    compilerDir = Path.Combine(cliDirectory, "../../../../Compiler/bin/Debug/net8.0");
                    compilerDll = Path.Combine(compilerDir, "DotVVM.Compiler.dll");
                }
#endif
                if (!File.Exists(compilerDll))
                {
                    throw new Exception($"DotVVM Compiler wasn't found at '{compilerDll}'. Please note that DotVVM compiler is not supported in versions prior to DotVVM 5.0.");
                }

                compilerArgs.Add("exec");
                compilerArgs.Add(compilerDll);
            }

            var projectDir = Path.GetDirectoryName(project.ProjectFilePath)!;
            var outputDir = Path.Combine(projectDir, project.OutputPath, configuration, framework);
            while (!Directory.Exists(outputDir))
            {
                outputDir = Directory.GetParent(outputDir).NotNull().FullName;
            }

            if (noColor)
            {
                compilerArgs.Add("--no-color");
            }
            compilerArgs.Add(Path.Combine(outputDir, $"{project.AssemblyName}.dll"));
            compilerArgs.Add(projectDir);

            var pinfo = new ProcessStartInfo {
                FileName = executable,
                UseShellExecute = false
            };
            foreach (var a in compilerArgs)
            {
                pinfo.ArgumentList.Add(a);
            }

            var process = System.Diagnostics.Process.Start(pinfo).NotNull();
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}
