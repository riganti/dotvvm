using System.Collections.Generic;

namespace DotVVM.Utils.ConfigurationHost.Lookup
{
    public class SearchResult : IResult
    {
        public CsprojVersion CsprojVersion { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public string CsprojFullName { get; set; }
        public string AssemblyName { get; set; }
        public bool RunDotvvmCompiler { get; set; }
        public List<PackageVersion> DotvvmPackagesVersions { get; set; }
    }
}