using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public class DotvvmProject
    {
        private const string MetadataFilename = "dotvvm-cli.json";
        private const string WriteDotvvmProjectMetadataTarget = "_WriteDotvvmProjectMetadata";
        private const string ScratchDirectory = "obj";
        // NB: You must always change both this constant and the embedded resource's name.
        private const string TargetsFilename = "DotVVMCommandLine.targets";

        private DotvvmProject(
            string assemblyName,
            string rootNamespace,
            ImmutableArray<NuGetFramework> targetFrameworks,
            string packageVersion,
            string projectFilePath)
        {
            AssemblyName = assemblyName;
            RootNamespace = rootNamespace;
            TargetFrameworks = targetFrameworks;
            PackageVersion = packageVersion;
            ProjectFilePath = projectFilePath;
        }

        public string AssemblyName { get; }

        public string RootNamespace { get; }

        public ImmutableArray<NuGetFramework> TargetFrameworks { get; }

        public string PackageVersion { get; }

        public string ProjectFilePath { get; }

        public static DotvvmProject? FromCsproj(string csprojPath, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var targetsPath = Path.Combine(Path.GetDirectoryName(csprojPath)!, ScratchDirectory, TargetsFilename);
            using var embeddedTargets = typeof(DotvvmProject).Assembly
                .GetManifestResourceStream($"DotVVM.CommandLine.Resources.{TargetsFilename}")!;
            using var reader = new StreamReader(embeddedTargets);
            File.WriteAllText(targetsPath, reader.ReadToEnd());

            var args = new List<string> {
                "msbuild",
                "/verbosity:quiet",
                "/nologo",
                $"/target:{WriteDotvvmProjectMetadataTarget}",
                $"/property:CustomBeforeMicrosoftCommonTargets='{targetsPath}';IsCrossTargetingBuild=false"
            };

            var pinfo = new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = string.Join(' ', args),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var process = Process.Start(pinfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                logger.LogError("The project metadata could not be obtained.");
                return null;
            }

            var metadataPath = Path.Combine(Path.GetDirectoryName(csprojPath)!, ScratchDirectory, MetadataFilename);
            var metadataText = File.ReadAllText(metadataPath);
            var rawMetadata = JsonSerializer.Deserialize<DotvvmProjectMetadata>(metadataText);
            return FromJson(metadataText);
        }

        public static DotvvmProject? FromJson(string json, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;
            var raw = JsonSerializer.Deserialize<DotvvmProjectMetadata>(json);
            if (raw is null)
            {
                logger.LogError("The project metadata could not be deserialized.");
                return null;
            }
            else if (raw?.AssemblyName is null)
            {
                logger.LogError($"{nameof(raw.AssemblyName)} is null.");
                return null;
            }
            else if (raw?.RootNamespace is null)
            {
                logger.LogError($"{nameof(raw.RootNamespace)} is null.");
                return null;
            }
            else if (raw?.PackageVersion is null)
            {
                logger.LogError($"{nameof(raw.PackageVersion)} is null.");
                return null;
            }
            else if (raw?.TargetFrameworks is null || raw?.TargetFrameworks.Contains(null!) == true)
            {
                logger.LogError($"{nameof(TargetFrameworks)} is null or contains null.");
                return null;
            }
            else if (raw?.ProjectFilePath is null)
            {
                logger.LogError($"{nameof(raw.ProjectFilePath)} is null.");
                return null;
            }

            return new DotvvmProject(
                raw!.AssemblyName,
                raw!.RootNamespace,
                raw!.TargetFrameworks.Select(t => NuGetFramework.Parse(t)).ToImmutableArray(),
                raw!.PackageVersion,
                raw!.ProjectFilePath);
        }

        // The following class should be in sync with the DotVVMConmandLine.targets file.
        private class DotvvmProjectMetadata
        {
            public string? AssemblyName { get; set; }

            public string? RootNamespace { get; set; }

            public string?[]? TargetFrameworks { get; set; }

            public string? PackageVersion { get; set; }

            public string? ProjectFilePath { get; set; }
        }
    }
}
