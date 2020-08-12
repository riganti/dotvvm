using System;
using System.IO;
using System.Runtime.InteropServices;
using DotVVM.Cli;

namespace DotVVM.CommandLine.Core
{
    public static class PathHelpers
    {
        public static string GetRelativePathFrom(string basePath, string fullPath)
        {
            basePath = Path.GetFullPath(basePath);
            fullPath = Path.GetFullPath(fullPath);

            if (!fullPath.StartsWith(fullPath))
            {
                throw new Exception($"The {fullPath} is not inside {basePath}!");
            }

            return fullPath.Substring(basePath.Length).TrimStart('/', '\\');
        }

        public static string GetDothtmlFileRelativePath(ProjectMetadataJson dotvvmProjectMetadata, string file)
        {
            var relativePath = PathHelpers.GetRelativePathFrom(dotvvmProjectMetadata.ProjectDirectory, file);
            if (relativePath.StartsWith("views/", StringComparison.CurrentCultureIgnoreCase)
                || relativePath.StartsWith("views\\", StringComparison.CurrentCultureIgnoreCase))
            {
                relativePath = relativePath.Substring("views/".Length);
            }
            return relativePath;
        }

        public static string CreateTypeNameFromPath(string relativePath)
        {
            return relativePath.Replace("/", ".").Replace("\\", ".");
        }

        public static string CreatePathFromTypeName(string typeName)
        {
            return typeName.Replace('.', Path.DirectorySeparatorChar);
        }

        public static string GetNamespaceFromFullType(string fullTypeName)
        {
            var index = fullTypeName.LastIndexOf('.');
            return index < 0 ? "" : fullTypeName.Substring(0, index);
        }

        public static string GetTypeNameFromFullType(string fullTypeName)
        {
            var index = fullTypeName.LastIndexOf('.');
            return index < 0 ? fullTypeName : fullTypeName.Substring(index + 1);
        }

        public static string TrimFileExtension(string path)
        {
            var lastDotIndex = path.LastIndexOf('.');
            var lastSlashIndex = path.LastIndexOfAny(new[] { '/', '\\' });
            if (lastDotIndex >= 0 && lastDotIndex > lastSlashIndex)
            {
                return path.Substring(0, lastDotIndex);
            }
            else
            {
                return path;
            }
        }

        public static bool IsCurrentDirectory(string directory)
        {
            return ComparePaths(directory, Directory.GetCurrentDirectory());
        }

        private static string NormalizePath(string directory)
        {
            return Path.GetFullPath(directory.TrimEnd('/', '\\'));
        }

        private static bool ComparePaths(string path1, string path2)
        {
            var comparison = PathComparison;
            return String.Equals(NormalizePath(path1), NormalizePath(path2), comparison);
        }

        public static string EnsureFileExtension(string path, string extension)
        {
            if (!path.EndsWith("." + extension, PathComparison))
            {
                path += "." + extension;
            }
            return path;
        }

        public static string ChangeExtension(string path, string extension)
        {
            return TrimFileExtension(path) + "." + extension;
        }

        private static StringComparison PathComparison
        {
            get
            {
#if DotNetCore
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;
#else
                return StringComparison.OrdinalIgnoreCase;
#endif
            }
        }
    }
}
