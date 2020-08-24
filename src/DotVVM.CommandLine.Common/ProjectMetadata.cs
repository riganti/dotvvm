using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DotVVM.CommandLine
{
    public class ProjectMetadata
    {
        public ProjectMetadata(
            FileInfo path,
            string projectName,
            string projectDirectory,
            string rootNamespace,
            string packageVersion,
            ImmutableArray<string> targetFrameworks,
            string? uiTestProjectPath,
            string? uiTestProjectRootNamespace,
            ImmutableArray<ApiClientDefinition> apiClients)
        {
            Path = path;
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
            RootNamespace = rootNamespace;
            PackageVersion = packageVersion;
            TargetFrameworks = targetFrameworks;
            UITestProjectPath = uiTestProjectPath;
            UITestProjectRootNamespace = uiTestProjectRootNamespace;
            ApiClients = apiClients;
        }

        public FileInfo Path { get; }

        public string ProjectName { get; }

        public string ProjectDirectory { get; }

        public string RootNamespace { get; }

        /// <summary>
        /// Version of the DotVVM package or assembly.
        /// </summary>
        public string PackageVersion { get; }

        public ImmutableArray<string> TargetFrameworks { get; set; }

        public string? UITestProjectPath { get; }

        public string? UITestProjectRootNamespace { get; }

        public ImmutableArray<ApiClientDefinition> ApiClients { get; }

        public ProjectMetadata WithApiClients(ImmutableArray<ApiClientDefinition> apiClients)
        {
            return new ProjectMetadata(
                Path,
                ProjectName,
                ProjectDirectory,
                RootNamespace,
                PackageVersion,
                TargetFrameworks,
                UITestProjectPath,
                UITestProjectRootNamespace,
                apiClients);
        }

        public ProjectMetadata WithUITestProject(string uiTestProjectPath, string uiTestProjectRootNamespace)
        {
            return new ProjectMetadata(
                Path,
                ProjectName,
                ProjectDirectory,
                RootNamespace,
                PackageVersion,
                TargetFrameworks,
                uiTestProjectPath,
                uiTestProjectRootNamespace,
                ApiClients);
        }

        public static Exception? IsJsonValid(ProjectMetadataJsonOld json)
        {
            if (json.MetadataFilePath is null || !File.Exists(json.MetadataFilePath))
            {
                return new ArgumentException(
                    $"{nameof(json.MetadataFilePath)} is null or does not exist.",
                    nameof(json));
            }
            if (json.ProjectName is null)
            {
                return new ArgumentException($"{nameof(json.ProjectName)} is null.", nameof(json));
            }
            if (json.ProjectDirectory is null || !Directory.Exists(json.ProjectDirectory))
            {
                return new ArgumentException(
                    $"{nameof(json.ProjectDirectory)} is null or does not exist.",
                    nameof(json));
            }
            if (json.RootNamespace is null)
            {
                return new ArgumentException($"{nameof(json.RootNamespace)} is null.", nameof(json));
            }
            if (json.PackageVersion is null)
            {
                return new ArgumentException($"{nameof(json.PackageVersion)} is null.", nameof(json));
            }
            if (json.TargetFrameworks is null || json.TargetFrameworks.Contains(null))
            {
                return new ArgumentException(
                    $"{nameof(json.TargetFrameworks)} is null or contains null.",
                    nameof(json));
            }
            return null;
        }

        public static ProjectMetadata FromJson(ProjectMetadataJsonOld json)
        {
            var error = IsJsonValid(json);
            if (error is object)
            {
                throw error;
            }

            return new ProjectMetadata(
                new FileInfo(json.MetadataFilePath),
                json.ProjectName!,
                json.ProjectDirectory!,
                json.RootNamespace!,
                json.PackageVersion!,
                json.TargetFrameworks.ToImmutableArray()!,
                json.UITestProjectPath,
                json.UITestProjectRootNamespace,
                json.ApiClients?.ToImmutableArray() ?? ImmutableArray.Create<ApiClientDefinition>());
        }

        public ProjectMetadataJsonOld ToJson()
        {
            return new ProjectMetadataJsonOld
            {
                MetadataFilePath = Path.FullName,
                ProjectName = ProjectName,
                ProjectDirectory = ProjectDirectory,
                RootNamespace = RootNamespace,
                PackageVersion = PackageVersion,
                TargetFrameworks = TargetFrameworks.ToList(),
                UITestProjectPath = UITestProjectPath,
                UITestProjectRootNamespace = UITestProjectRootNamespace,
                ApiClients = ApiClients.ToList()
            };
        }
    }
}
