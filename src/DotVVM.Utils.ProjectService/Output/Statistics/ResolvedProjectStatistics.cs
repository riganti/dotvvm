using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class ResolvedProjectStatistics : IResolvedProjectMetadata
    {
        public CsprojVersion CsprojVersion { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public string CsprojFullName { get; set; }
        public string AssemblyName { get; set; }
        public bool RunDotvvmCompiler { get; set; }
        public List<OperationResult> OperationResults { get; set; }
        public List<ProjectDependency> DotvvmProjectDependencies { get; set; }
        public string AssemblyPath { get; set; }
        public string ProjectRootDirectory { get; set; }
        public List<string> DotvvmPackageNugetFolders { get; set; }
    }
}
