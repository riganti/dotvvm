using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.Utils.ProjectService
{
    public interface IResult
    {
        CsprojVersion CsprojVersion { get; set; }
        TargetFramework TargetFramework { get; set; }
        string CsprojFullName { get; set; }
        string AssemblyName { get; set; }
        bool RunDotvvmCompiler { get; set; }
        List<PackageVersion> DotvvmPackagesVersions { get; set; }
    }
}