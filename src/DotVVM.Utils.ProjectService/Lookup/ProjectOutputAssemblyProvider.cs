using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public static class ProjectOutputAssemblyProvider
    {
        public static string GetAssemblyPath(FileInfo file, string assemblyName, TargetFramework target, string configuration = "Debug")
        {
            var bin = Path.Combine(file.DirectoryName, "bin");

            var targetPath = Path.Combine(bin, configuration, target.TranslateToFolderName());
            var assemblyFullName = Path.Combine(targetPath, assemblyName);
            if (File.Exists(assemblyFullName + ".dll"))
                return assemblyFullName + ".dll";
            if (File.Exists(assemblyFullName + ".exe"))
                return assemblyFullName + ".exe";
            return null;
        }
    }
}
