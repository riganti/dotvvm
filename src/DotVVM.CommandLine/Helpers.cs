using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine
{
    public class Helpers
    {

        public static string AskForValue(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

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

        public static string GetDothtmlFileRelativePath(DotvvmProjectMetadata dotvvmProjectMetadata, string file)
        {
            var relativePath = Helpers.GetRelativePathFrom(dotvvmProjectMetadata.ProjectDirectory, file);
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

    }
}
