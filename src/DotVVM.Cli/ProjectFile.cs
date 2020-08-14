using System.IO;

namespace DotVVM.Cli
{
    public static class ProjectFile
    {
        public const string ProjectFileExtension = ".csproj";

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
    }
}
