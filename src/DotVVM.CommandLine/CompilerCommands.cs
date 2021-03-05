using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            lintCmd.AddOption(new Option<bool>("--no-build", "Don't build the MSBuild project."));
            lintCmd.AddOption(new Option<string>(
                "--configuration",
                () => "Debug",
                "The configuration used to build the project."));
            lintCmd.AddOption(new Option<string>("--framework", "The target framework used to build the project."));
            lintCmd.Handler = CommandHandler.Create(typeof(CompilerCommands).GetMethod(nameof(HandleLint))!);
            command.AddCommand(lintCmd);
        }

        public static int HandleLint(
            DotvvmProject project,
            bool noBuild,
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

            if (!noBuild)
            {
                // TODO: If the target framework is the .NET Framework, use VS's MSBuild.
                var msbuild = MSBuild.CreateFromSdk();
                var buildSuccess = msbuild.TryBuild(
                    project: new FileInfo(project.ProjectFilePath),
                    configuration: configuration,
                    targetFramework: framework,
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
            var targetFramework = NuGetFramework.Parse(framework);
            var executable = "dotnet";
            if (targetFramework.IsDesktop())
            {
                executable = Path.Combine(cliDirectory, "tools/net461/any/DotVVM.Compiler.exe");
            }
            else
            {
                var compilerDir = Path.Combine(cliDirectory, "tools/netcoreapp3.1/any");
                compilerArgs.Add("exec");
                compilerArgs.Add(Path.Combine(compilerDir, "DotVVM.Compiler.dll"));
            }

            var projectDir = Path.GetDirectoryName(project.ProjectFilePath)!;
            var outputDir = Path.Combine(projectDir, project.OutputPath, configuration, framework);
            if (!Directory.Exists(outputDir))
            {
                outputDir = Directory.GetParent(outputDir).FullName;
            }

            compilerArgs.Add(Path.Combine(outputDir, $"{project.AssemblyName}.dll"));
            compilerArgs.Add(projectDir);

            var pinfo = new ProcessStartInfo {
                FileName = executable,
                UseShellExecute = false,
                Arguments = string.Join(' ', compilerArgs)
            };

            var process = System.Diagnostics.Process.Start(pinfo);
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}
