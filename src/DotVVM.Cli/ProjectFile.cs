using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Cli
{
    public static class ProjectFile
    {
        public const string ProjectFileExtension = ".csproj";
        public const string DotvvmMetadataFile = ".dotvvm.json";
        public const string DotvvmPackage = "DotVVM";
        public const string DotvvmAssembly = "DotVVM.Framework";

        public static FileInfo? FindProjectFile(FileSystemInfo target)
        {
            if (!target.Exists)
            {
                return null;
            }

            if (target is FileInfo file && file.Extension == ProjectFileExtension)
            {
                return file;
            }

            if (target is DirectoryInfo dir)
            {
                var projectFiles = dir.GetFiles($"*{ProjectFileExtension}");
                if (projectFiles.Length == 1)
                {
                    return projectFiles[0];
                }
            }

            return null;
        }

        public static FileInfo? FindProjectMetadata(FileSystemInfo target)
        {
            if (!target.Exists)
            {
                return null;
            }

            var directory = target switch
            {
                DirectoryInfo dir => dir,
                FileInfo file => file.Directory,
                _ => throw new NotImplementedException()
            };
            var metadata = new FileInfo(Path.Combine(directory.FullName, DotvvmMetadataFile));
            return metadata.Exists ? metadata : null;
        }

        public static async Task<DotvvmProjectMetadata> LoadProjectMetadata(FileInfo file)
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<DotvvmProjectMetadata>(stream);
        }

        public static async Task<DotvvmProjectMetadata?> GetProjectMetadata(
            FileSystemInfo target,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var file = FindProjectMetadata(target);
            if (file is null)
            {
                return await CreateProjectMetadata(target, logger, errorLevel);
            }

            return await LoadProjectMetadata(file);
        }

        public static async Task<DotvvmProjectMetadata?> CreateProjectMetadata(
            FileSystemInfo target,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var projectFile = FindProjectFile(target);
            if (projectFile is null)
            {
                logger.Log(errorLevel, $"No project could be found at '{target}'.");
                return null;
            }

            var msbuildInstance = MSBuildLocator.RegisterDefaults();
            if (msbuildInstance is null || string.IsNullOrEmpty(msbuildInstance.MSBuildPath))
            {
                logger.Log(errorLevel, $"Could not load MSBuild libraries.");
                return null;
            }

            var metadata = CreateProjectMetadataFromMSBuild(projectFile, logger, errorLevel);
            if (metadata is null)
            {
                return null;
            }

            await SaveProjectMetadata(
                file: new FileInfo(Path.Combine(projectFile.DirectoryName, DotvvmMetadataFile)),
                metadata: metadata);
            return metadata;
        }

        public static async Task SaveProjectMetadata(FileInfo file, DotvvmProjectMetadata metadata)
        {
            using var stream = file.Open(FileMode.Create, FileAccess.Write);
            metadata.MetadataFilePath = file.FullName;
            await JsonSerializer.SerializeAsync(stream, metadata);
        }

        private static DotvvmProjectMetadata? CreateProjectMetadataFromMSBuild(
            FileInfo projectFile,
            ILogger logger,
            LogLevel errorLevel)
        {
            var project = Project.FromFile(projectFile.FullName, new ProjectOptions
            {
                LoadSettings = ProjectLoadSettings.IgnoreInvalidImports
                    | ProjectLoadSettings.IgnoreMissingImports
            });
            return new DotvvmProjectMetadata
            {
                Version = 2, // TODO: Why?
                ProjectDirectory = projectFile.DirectoryName,
                RootNamespace = project.GetPropertyValue("RootNamespace"),
                ProjectName = project.GetPropertyValue("AssemblyName"),
                PackageVersion = GetDotvvmVersion(project),
            };
        }
        private static string? GetDotvvmVersion(Project project)
        {
            var package = project.GetItems("PackageReference")
                .FirstOrDefault(p => p.EvaluatedInclude == DotvvmPackage);
            if (package is object)
            {
                return package.GetMetadataValue("Version");
            }

            var reference = project.GetItems("Reference")
                .Select(r => new AssemblyName(r.EvaluatedInclude))
                .FirstOrDefault(n => n.Name == DotvvmAssembly);
            if (reference is object && reference.Version is object)
            {
                return reference.Version.ToString();
            }

            return null;
        }
    }
}
