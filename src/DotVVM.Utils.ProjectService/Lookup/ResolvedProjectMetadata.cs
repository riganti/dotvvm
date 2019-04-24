using System.Collections.Generic;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class ResolvedProjectMetadata : IResolvedProjectMetadata
    {
        public CsprojVersion CsprojVersion { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public string CsprojFullName { get; set; }
        public string AssemblyName { get; set; }
        public bool RunDotvvmCompiler { get; set; }
        public List<PackageVersion> DotvvmPackagesVersions { get; set; }
        public string AssemblyPath { get; set; }
        public string ProjectRootDirectory { get; set; }
        public string DotvvmPackageNugetFolder { get; set; }
    }
}
