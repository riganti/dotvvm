using System;

namespace DotVVM.Cli
{
    public class ProjectMetadata
    {
        public ProjectMetadata(
            string projectName,
            string projectDirectory,
            string rootNamespace,
            string packageVersion)
        {
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
            RootNamespace = rootNamespace;
            PackageVersion = packageVersion;
        }

        public string ProjectName { get; }

        public string ProjectDirectory { get; }

        public string RootNamespace { get; }

        /// <summary>
        /// Version of the DotVVM package or assembly.
        /// </summary>
        public string PackageVersion { get; }

        public static Exception? IsJsonValid(ProjectMetadataJson json)
        {
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
                json.ProjectName!,
                json.ProjectDirectory!,
                json.RootNamespace!,
                json.PackageVersion!);
        }

        public ProjectMetadataJson ToJson()
        {
            return new ProjectMetadataJson
            {
                ProjectName = ProjectName,
                ProjectDirectory = ProjectDirectory,
                RootNamespace = RootNamespace,
                PackageVersion = PackageVersion
            };
        }
    }
}
