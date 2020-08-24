using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public class ProjectMetadata
    {
        public const string MetadataFilename = "metadata.json";
        public const string WriteDotvvmMetadataTarget = "_WriteProjectMetadata";

        public ProjectMetadata(
            string assemblyName,
            string rootNamespace,
            ImmutableArray<NuGetFramework> targetFrameworks,
            string packageVersion,
            string projectFilePath,
            string metadataFilePath)
        {
            AssemblyName = assemblyName;
            RootNamespace = rootNamespace;
            TargetFrameworks = targetFrameworks;
            PackageVersion = packageVersion;
            ProjectFilePath = projectFilePath;
            MetadataFilePath = metadataFilePath;
        }

        public string AssemblyName { get; }

        public string RootNamespace { get; }

        public ImmutableArray<NuGetFramework> TargetFrameworks { get; }

        public string PackageVersion { get; }

        public string ProjectFilePath { get; }

        public string MetadataFilePath { get; }

        public static bool TryCreateFromJson(
            ProjectMetadataJson json,
            [NotNullWhen(true)] out ProjectMetadata? metadata,
            [NotNullWhen(false)] out string? error)
        {
            error = null;
            metadata = null;

            if (json.MetadataFilePath is null || !File.Exists(json.MetadataFilePath))
            {
                error = $"{nameof(json.MetadataFilePath)} is null or does not exist.";
            }
            if (json.ProjectFilePath is null || !File.Exists(json.ProjectFilePath))
            {
                error = $"{nameof(json.ProjectFilePath)} is null or does not exist.";
            }
            if (json.AssemblyName is null)
            {
                error = $"{nameof(json.AssemblyName)} is null.";
            }
            if (json.RootNamespace is null)
            {
                error = $"{nameof(json.RootNamespace)} is null.";
            }
            if (json.PackageVersion is null)
            {
                error = $"{nameof(json.PackageVersion)} is null.";
            }
            if (json.TargetFrameworks is null || json.TargetFrameworks.Contains(null!))
            {
                error = $"{nameof(TargetFrameworks)} is null or contains null.";
            }
            if (error is object)
            {
                return false;
            }

            metadata = new ProjectMetadata(
                json.AssemblyName!,
                json.RootNamespace!,
                json.TargetFrameworks.Select(t => NuGetFramework.Parse(t)).ToImmutableArray(),
                json.PackageVersion!,
                json.ProjectFilePath!,
                json.MetadataFilePath!);
            return true;
        }

        public static FileInfo? Find(FileSystemInfo target)
        {
            if (!target.Exists)
            {
                return null;
            }

            var metadata = DotvvmProject.GetCliFile(target, MetadataFilename);
            return metadata.Exists ? metadata : null;
        }

        public static async Task<ProjectMetadata?> Load(
            FileInfo file,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Debug)
        {
            logger ??= NullLogger.Instance;

            try
            {
                using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var json = await JsonSerializer.DeserializeAsync<ProjectMetadataJson>(stream);
                if (TryCreateFromJson(json, out var metadata, out var error))
                {
                    return metadata;
                }
                else
                {
                    logger.Log(errorLevel, error);
                    return null;
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static async Task<ProjectMetadata?> LoadOrCreate(
            FileSystemInfo target,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var file = Find(target);
            if (file is null)
            {
                logger.LogDebug($"Creating DotVVM metadata at '{file}'.");
                file = Create(target, true, logger, errorLevel);
                if (file is null)
                {
                    return null;
                }
            }
            else
            {
                logger.LogDebug($"Found DotVVM metadata at '{file}'.");
            }

            return await Load(file, logger);
        }

        public static FileInfo? Create(
            FileSystemInfo target,
            bool showMSBuildOutput = false,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var projectFile = ProjectFile.FindProjectFile(target);
            if (projectFile is null)
            {
                logger.Log(errorLevel, $"No project could be found at '{target}'.");
                return null;
            }

            logger.LogDebug($"Found a project file at '{projectFile}'.");
            var msbuild = MSBuild.Create();
            if (msbuild is null)
            {
                logger.Log(errorLevel, "Could not found an MSBuild executable.");
                return null;
            }
            logger.LogDebug($"Found the '{msbuild}' MSBuild executable.");

            var metadataFile = WriteDotvvmMetadata(msbuild, projectFile, showMSBuildOutput, logger, errorLevel);
            if (metadataFile is null)
            {
                return null;
            }
            return metadataFile;
        }

        private static FileInfo? WriteDotvvmMetadata(
            MSBuild msbuild,
            FileInfo project,
            bool showMSBuildOutput,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var writeMetadataProjectFile = DotvvmProject.GetCliFile(project, $"{WriteDotvvmMetadataTarget}.proj");
            File.WriteAllText(writeMetadataProjectFile.FullName, GetWriteDotvvmMetadataProject());

            var success = msbuild.TryInvoke(
                project: project,
                properties: new[]
                {
                    new KeyValuePair<string, string>(
                        "CustomBeforeMicrosoftCommonTargets",
                        writeMetadataProjectFile.FullName),
                    new KeyValuePair<string, string>("IsCrossTargetingBuild", "false")
                },
                targets: new[] { WriteDotvvmMetadataTarget },
                showOutput: showMSBuildOutput,
                logger: logger);
            if (!success)
            {
                logger.Log(errorLevel, $"The DotVVM metadata of '{project}' could not be determined.");
                return null;
            }

            return DotvvmProject.GetCliFile(project, MetadataFilename);
        }

        private static string GetWriteDotvvmMetadataProject()
        {
            using var stream = typeof(DotvvmProject).Assembly
                .GetManifestResourceStream("DotVVM.CommandLine.Common.WriteProjectMetadata.targets");
            if (stream is null)
            {
                throw new InvalidOperationException("Could not read the embedded WriteProjectMetadata.targets file.");
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
