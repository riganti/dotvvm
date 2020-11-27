using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Build
{
    public class MsBuildBuildOperation : BuildOperation
    {
        private string MsBuildPath { get; }

        public MsBuildBuildOperation(string msBuildPath)
        {
            SupportedCsprojVersion = CsprojVersion.OlderProjectSystem;
            MsBuildPath = msBuildPath;
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return $" \"{metadata.CsprojFullName}\" /restore /v:m /p:OutDir={Constants.BuildPath}";
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, MsBuildPath, arguments);
        }
    }
}