using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.Cli
{
    public static class ProjectFile
    {
        public const string ProjectFileExtension = ".csproj";
        public const string DotvvmvMetadataFile = ".dotvvm.json";

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
            var metadata = new FileInfo(Path.Combine(directory.FullName, DotvvmvMetadataFile));
            return metadata.Exists ? metadata : null;
        }

        public static async Task<DotvvmProjectMetadata> LoadProjectMetadata(FileInfo file)
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<DotvvmProjectMetadata>(stream);
        }

        public static async Task<DotvvmProjectMetadata?> LoadProjectMetadata(
            FileSystemInfo target,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var file = FindProjectMetadata(target);
            if (file is null)
            {
                logger.Log(errorLevel, "No DotVVM metadata file could be found.");
                return null;
            }

            return await LoadProjectMetadata(file);
        }

        public static async Task SaveProjectMetadata(FileInfo file, DotvvmProjectMetadata metadata)
        {
            using var stream = file.Open(FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, metadata);
        }
    }
}
