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
            string executable;
            if (targetFramework.IsDesktop())
            {
                executable = FindNetFwCompilerExecutable(project, cliDirectory)
                    ?? throw new Exception($"DotVVM Compiler (for .NET Framework) could not be found in the NuGet package cache. "
                        + "Please ensure the DotVVM NuGet package is properly installed.");
            }
            else
            {
                var compilerDll = FindNetCompilerDll(project, cliDirectory)
                    ?? throw new Exception($"DotVVM Compiler could not be found in the NuGet package cache. "
                        + "Please ensure the DotVVM NuGet package is properly installed.");

                compilerArgs.Add("exec");
                compilerArgs.Add(compilerDll);
                executable = "dotnet";
            }

            var projectDir = Path.GetDirectoryName(project.ProjectFilePath)!;
            var outputRoot = Path.Combine(projectDir, project.OutputPath);
            var assemblyName = $"{project.AssemblyName}.dll";
            var assemblyPath = targetFramework.IsDesktop()
                ? Path.Combine(outputRoot, assemblyName)
                : Path.Combine(outputRoot, configuration, framework, assemblyName);

            if (noColor)
            {
                compilerArgs.Add("--no-color");
            }
            compilerArgs.Add(assemblyPath);
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

        private static string? FindNetFwCompilerExecutable(DotvvmProject project, string cliDirectory)
        {
            // Look for the compiler in the DotVVM NuGet package (tools/netfw/DotVVM.Compiler.exe)
            const string compilerRelativePath = "DotVVM.Compiler.exe";
            foreach (var folder in GetNuGetPackageFolders(project))
            {
                var exe = Path.Combine(folder, "dotvvm", project.PackageVersion, "tools", "netfw", compilerRelativePath);
                if (File.Exists(exe))
                    return exe;
            }
#if DEBUG
            // When running the CLI from source, use the locally built .NET Framework compiler.
            var debugExe = Path.Combine(cliDirectory, "../../../../Compiler/bin/Debug/net472/DotVVM.Compiler.exe");
            if (File.Exists(debugExe))
                return debugExe;
#endif
            return null;
        }

        private static string? FindNetCompilerDll(DotvvmProject project, string cliDirectory)
        {
            // Look for the compiler in the DotVVM NuGet package (tools/net/DotVVM.Compiler.dll)
            const string compilerRelativePath = "DotVVM.Compiler.dll";
            foreach (var folder in GetNuGetPackageFolders(project))
            {
                var dll = Path.Combine(folder, "dotvvm", project.PackageVersion, "tools", "net", compilerRelativePath);
                if (File.Exists(dll))
                    return dll;
            }
#if DEBUG
            // When running the CLI from source, use the locally built .NET compiler.
            var debugDll = Path.Combine(cliDirectory, "../../../../Compiler/bin/Debug/net8.0/DotVVM.Compiler.dll");
            if (File.Exists(debugDll))
                return debugDll;
#endif
            return null;
        }

        private static IEnumerable<string> GetNuGetPackageFolders(DotvvmProject project)
        {
            // Use the package folders reported by NuGet restore for the user's project
            if (!string.IsNullOrEmpty(project.NuGetPackageFolders))
            {
                foreach (var folder in project.NuGetPackageFolders.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = folder.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        yield return trimmed;
                }
            }

            // Fall back to the default NuGet global packages folder
            var envPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(envPackages))
            {
                yield return envPackages;
            }
            else
            {
                var defaultFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".nuget", "packages");
                yield return defaultFolder;
            }
        }
    }
}
