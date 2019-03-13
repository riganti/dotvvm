using System.Collections.Generic;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class StatisticsResult : IResult
    {
        public CsprojVersion CsprojVersion { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public string CsprojFullName { get; set; }
        public string AssemblyName { get; set; }
        public bool RunDotvvmCompiler { get; set; }
        public List<OperationResult> OperationResults { get; set; }
        public List<PackageVersion> DotvvmPackagesVersions { get; set; }
    }
}