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
            string outputPath,
            string rootNamespace,
            ImmutableArray<NuGetFramework> targetFrameworks,
            string packageVersion,
            string projectFilePath)
        {
            AssemblyName = assemblyName;
            OutputPath = outputPath;
            RootNamespace = rootNamespace;
            TargetFrameworks = targetFrameworks;
            PackageVersion = packageVersion;
            ProjectFilePath = projectFilePath;
        }

        public string AssemblyName { get; }

        public string OutputPath { get; }

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

            bool WriteMetadata(MSBuild? msbuild)
            {
                return msbuild is object && msbuild.TryInvoke(
                    project: new FileInfo(csprojPath),
                    properties: new Dictionary<string, string> {
                        ["CustomBeforeMicrosoftCommonTargets"] = targetsPath,
                        ["IsCrossTargetingBuild"] = "false"
                    },
                    targets: new[] { WriteDotvvmProjectMetadataTarget },
                    verbosity: "quiet",
                    logger: logger);
            }

            if (!WriteMetadata(MSBuild.CreateFromVS()) && !WriteMetadata(MSBuild.CreateFromSdk()))
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
            else if (raw?.OutputPath is null)
            {
                logger.LogError($"{nameof(raw.OutputPath)} is null.");
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
                raw!.OutputPath,
                raw!.RootNamespace,
                raw!.TargetFrameworks.Select(t => NuGetFramework.Parse(t)).ToImmutableArray(),
                raw!.PackageVersion,
                raw!.ProjectFilePath);
        }

        public const string ProjectFileExtension = ".csproj";

        public static FileInfo? FindProjectFile(string target)
        {
            var file = new FileInfo(target);
            if (file.Exists && file.Extension == ProjectFileExtension)
            {
                return file;
            }

            var dir = new DirectoryInfo(target);
            if (dir.Exists)
            {
                var projectFiles = dir.GetFiles($"*{ProjectFileExtension}");
                if (projectFiles.Length == 1)
                {
                    return projectFiles[0];
                }
            }

            return null;
        }

        // The following class should be in sync with the DotVVMConmandLine.targets file.
        private class DotvvmProjectMetadata
        {
            public string? AssemblyName { get; set; }

            public string? OutputPath { get; set; }

            public string? RootNamespace { get; set; }

            public string?[]? TargetFrameworks { get; set; }

            public string? PackageVersion { get; set; }

            public string? ProjectFilePath { get; set; }
        }
    }
}
