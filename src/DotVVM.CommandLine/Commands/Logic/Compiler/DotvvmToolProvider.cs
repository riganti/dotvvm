using System.IO;
using System.Linq;
using DotVVM.Utils.ProjectService;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public abstract class DotvvmToolProvider
    {
        public static string CombineNugetPath(IResolvedProjectMetadata metadata, string mainModuleRelativePath)
        {
            var nugetPath = metadata.DotvvmPackageNugetFolders.FirstOrDefault(s => File.Exists(Path.Combine(s, mainModuleRelativePath)));
            return Path.Combine(nugetPath, mainModuleRelativePath);
        }
    }
}