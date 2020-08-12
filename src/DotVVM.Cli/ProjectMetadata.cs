using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DotVVM.Cli
{
    public class ProjectMetadata
    {
        public ProjectMetadata(
            FileInfo path,
            string projectName,
            string projectDirectory,
            string rootNamespace,
            string packageVersion,
            ImmutableArray<ApiClientDefinition> apiClients)
        {
            Path = path;
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
            RootNamespace = rootNamespace;
            PackageVersion = packageVersion;
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

        public ImmutableArray<ApiClientDefinition> ApiClients { get; }

        public ProjectMetadata WithApiClients(ImmutableArray<ApiClientDefinition> apiClients)
        {
            return new ProjectMetadata(
                Path,
                ProjectName,
                ProjectDirectory,
                RootNamespace,
                PackageVersion,
                apiClients);
        }

        public static Exception? IsJsonValid(ProjectMetadataJson json)
        {
            if (json.MetadataFilePath is null)
            {
                return new ArgumentException($"{nameof(json.MetadataFilePath)} is null.", nameof(json));
            }
            if (json.ProjectName is null)
            {
                return new ArgumentException($"{nameof(json.ProjectName)} is null.", nameof(json));
            }
            if (json.ProjectDirectory is null)
            {
                return new ArgumentException($"{nameof(json.ProjectDirectory)} is null.", nameof(json));
            }
            if (json.RootNamespace is null)
            {
                return new ArgumentException($"{nameof(json.RootNamespace)} is null.", nameof(json));
            }
            if (json.PackageVersion is null)
            {
                return new ArgumentException($"{nameof(json.PackageVersion)} is null.", nameof(json));
            }
            return null;
        }

        public static ProjectMetadata FromJson(ProjectMetadataJson json)
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
                json.ApiClients?.ToImmutableArray() ?? ImmutableArray.Create<ApiClientDefinition>());
        }

        public ProjectMetadataJson ToJson()
        {
            return new ProjectMetadataJson
            {
                MetadataFilePath = Path.FullName,
                ProjectName = ProjectName,
                ProjectDirectory = ProjectDirectory,
                RootNamespace = RootNamespace,
                PackageVersion = PackageVersion,
                ApiClients = ApiClients.ToList()
            };
        }
    }
}
