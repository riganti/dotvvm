using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DotVVM.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public static class Shims
    {
        public const string GeneratorNotice = "NOTICE: This file has been generated automatically.";
        public const string Netcoreapp = "netcoreapp3.1";

        public static string GetShimProject(
            string project,
            string targetFramework,
            string dotvvmVersion,
            string programFile,
            string appPackage,
            string? appPath)
        {
            var sb = new StringBuilder();
            sb.Append(
$@"<Project Sdk=""Microsoft.NET.Sdk"">

  <!-- {GeneratorNotice} -->

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{targetFramework}</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include=""{programFile}"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""{project}"" />
  </ItemGroup>");
            if (appPath is null)
            {
                sb.Append(
$@"
  <ItemGroup>
    <PackageReference Include=""{appPackage}"" Version=""{dotvvmVersion}"" />
  </ItemGroup>
");
            }
            else
            {
                sb.Append(
$@"
  <ItemGroup>
    <ProjectReference Include=""{appPath}"" />
  </ItemGroup>
");
            }

            sb.Append(
$@"
</Project>
");
            return sb.ToString();
        }

        public static string GetShimProgram(
            string shimName,
            string appProgramClass)
        {
            // TODO: Add a new entry point to the Compiler.
            return
$@"
namespace DotVVM.{shimName}.Shim
{{
    public static class Program
    {{
        public static int Main(string[] args)
        {{
            return {appProgramClass}.Main(args);
        }}
    }}
}}
";
        }

        public static bool TryCreateRunShim(
            string shimName,
            FileSystemInfo target,
            FileSystemInfo? app,
            IEnumerable<string> args,
            Func<ShimCreationContext, FileInfo?> createShim,
            bool isDebug,
            MSBuild msbuild,
            bool shouldShowMSBuild,
            ILogger? logger)
        {
            logger ??= NullLogger.Instance;
            var project = ProjectFile.FindProjectFile(target);
            if (project is null)
            {
                logger.LogCritical("No project file could be found.");
                return false;
            }
            else
            {
                logger.LogDebug($"Found the '{project}' project file.");
            }

            FileInfo? appProject = null;
            if (app is object)
            {
                appProject = ProjectFile.FindProjectFile(app);
                if (appProject is null)
                {
                    logger.LogError($"{shimName} could not be found at '{app}'. Ignoring.");
                }
            }

            var dotvvmDir = DotvvmProject.CreateDotvvmDirectory(project);

            var context = new ShimCreationContext(project, dotvvmDir, appProject);
            var shim = createShim(context);
            if (shim is null)
            {
                return false;
            }

            var configuration = isDebug ? "Debug" : "Release";
            logger.LogInformation($"Building a {shimName} shim.");
            if (!msbuild.TryBuild(shim, configuration, shouldShowMSBuild, logger))
            {
                logger.LogCritical($"Failed to build a {shimName} shim.");
                return false;
            }

            var executablePath = $"bin/{configuration}/{Netcoreapp}/{shimName}.dll";
            var executable = new FileInfo(Path.Combine(shim.DirectoryName, executablePath));
            return TryRunShim(executable, args, logger);
        }

        public static bool TryRunShim(
            FileInfo shim,
            IEnumerable<string> args,
            ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;
            if (!shim.Exists)
            {
                logger.LogCritical($"No executable could not be found at '{shim}'.");
                return false;
            }

            var sb = new StringBuilder();
            sb.Append(shim.FullName);
            sb.Append(' ');
            sb.Append(string.Join(" ", args.Select(s => $"\"{s}\"")));
            var processInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = sb.ToString()
            };
            var process = Process.Start(processInfo);
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public static FileInfo CreateBasicShim(
            ShimCreationContext context,
            string shimName,
            string shimProjectFile,
            string shimTargetFramework,
            string shimProgramFile,
            string appPackage,
            string appPackageVersion,
            string appProgramClass)
        {
            string? appPath = null;
            if (context.App is object)
            {
                appPath = Names.GetRelativePath(context.DotvvmDirectory.FullName, context.App.FullName);
            }

            var shimFile = new FileInfo(Path.Combine(context.DotvvmDirectory.FullName, shimProjectFile));

            File.WriteAllText(
                path: shimFile.FullName,
                contents: GetShimProject(
                    project: Names.GetRelativePath(context.DotvvmDirectory.FullName, context.Project.FullName),
                    targetFramework: shimTargetFramework,
                    dotvvmVersion: appPackageVersion,
                    programFile: shimProgramFile,
                    appPackage: appPackage,
                    appPath: appPath));

            File.WriteAllText(
                path: Path.Combine(context.DotvvmDirectory.FullName, shimProgramFile),
                contents: GetShimProgram(
                    shimName: shimName,
                    appProgramClass: appProgramClass));

            return shimFile;
        }

        public static NuGetFramework GetSuitableTargetFramework(IEnumerable<NuGetFramework> frameworks)
        {
            var netCoreApp = frameworks
                .Where(f => f.Framework == FrameworkConstants.FrameworkIdentifiers.NetCoreApp)
                .OrderBy(f => f.Version.Minor)
                .OrderBy(f => f.Version.Major)
                .FirstOrDefault();
            if (netCoreApp is object)
            {
                return netCoreApp;
            }

            var net = frameworks
                .Where(f => f.Framework == FrameworkConstants.FrameworkIdentifiers.Net)
                .OrderBy(f => f.Version.Minor)
                .OrderBy(f => f.Version.Minor)
                .FirstOrDefault();
            if (net is object)
            {
                return net;
            }

            return FrameworkConstants.CommonFrameworks.NetCoreApp21;
        }
    }
}
