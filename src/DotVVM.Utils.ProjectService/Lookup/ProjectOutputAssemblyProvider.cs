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
            if ((target & (TargetFramework.NetStandard | TargetFramework.NetCoreApp)) > 0)
            {
                var targetPath = Path.Combine(bin, configuration, target.TranslateToFolderName());
                var assemblyFullName = Path.Combine(targetPath, assemblyName);
                if (File.Exists(assemblyFullName + ".dll"))
                    return assemblyFullName + ".dll";
                if (File.Exists(assemblyFullName + ".exe"))
                    return assemblyFullName + ".exe";
            }

            if ((target & TargetFramework.NetFramework) > 0)
            {
                throw new NotImplementedException();
            }
            return null;
        }
    }
}
