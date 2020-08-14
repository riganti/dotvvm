using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Utils.ConfigurationHost.Initialization;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Cli
{
    public static class DotvvmProject
    {
        public const string DotvvmMetadataFile = ".dotvvm.json";
        public const string DotvvmPackage = "DotVVM";
        public const string DotvvmAssembly = "DotVVM.Framework";
        public const string FallbackDotvvmVersion = "2.4.0.1";

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

        public static async Task<ProjectMetadata?> LoadProjectMetadata(
            FileInfo file,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Debug)
        {
            logger ??= NullLogger.Instance;

            try
            {
                using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var json = await JsonSerializer.DeserializeAsync<ProjectMetadataJson>(stream);
                var error = ProjectMetadata.IsJsonValid(json);
                if (error is object)
                {
                    logger.Log(errorLevel, error, "DotVVM metadata are not valid.");
                    return null;
                }
                return ProjectMetadata.FromJson(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static async Task<ProjectMetadata?> GetProjectMetadata(
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

            logger.LogDebug($"Found DotVVM metadata at '{file}'.");
            var metadata = await LoadProjectMetadata(file, logger);
            if (metadata is null)
            {
                return await CreateProjectMetadata(target, logger, errorLevel);
            }
            return metadata;
        }

        public static async Task<ProjectMetadata?> CreateProjectMetadata(
            FileSystemInfo target,
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
            var msbuildInstance = MSBuildLocator.RegisterDefaults();
            if (msbuildInstance is null || string.IsNullOrEmpty(msbuildInstance.MSBuildPath))
            {
                logger.Log(errorLevel, $"Could not load MSBuild libraries.");
                return null;
            }

            logger.LogDebug($"Using MSBuild at '{msbuildInstance.MSBuildPath}' for project file inspection.");
            var metadata = CreateProjectMetadataFromMSBuild(projectFile);
            if (metadata is null)
            {
                return null;
            }

            await SaveProjectMetadata(metadata);
            logger.LogDebug($"Saved DotVVM metadata to '{metadata.Path}'.");
            return metadata;
        }

        public static async Task SaveProjectMetadata(ProjectMetadata metadata)
        {
            using var stream = metadata.Path.Open(FileMode.Create, FileAccess.Write);
            var json = metadata.ToJson();
            json.Version = 2; // TODO: Why?
            await JsonSerializer.SerializeAsync(stream, json, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static DotvvmConfiguration GetConfiguration(string projectName, string projectDirectory)
        {
            return ConfigurationHost.InitDotVVM(
                webSiteAssembly: Assembly.Load(projectName),
                webSitePath: projectDirectory,
                servicesRegistration: c => c.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());
        }

        private static ProjectMetadata? CreateProjectMetadataFromMSBuild(FileInfo projectFile)
        {
            var project = Project.FromFile(projectFile.FullName, new ProjectOptions
            {
                LoadSettings = ProjectLoadSettings.IgnoreInvalidImports
                    | ProjectLoadSettings.IgnoreMissingImports
            });
            return new ProjectMetadata(
                path: new FileInfo(Path.Combine(projectFile.DirectoryName, DotvvmMetadataFile)),
                projectName: project.GetPropertyValue("AssemblyName"),
                projectDirectory: projectFile.DirectoryName,
                rootNamespace: project.GetPropertyValue("RootNamespace"),
                packageVersion: GetDotvvmVersion(project),
                uiTestProjectPath: null,
                uiTestProjectRootNamespace: null,
                apiClients: ImmutableArray.Create<ApiClientDefinition>());
        }
        private static string GetDotvvmVersion(Project project)
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

            return FallbackDotvvmVersion;
        }
    }
}
